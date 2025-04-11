using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using ManagedBass.Asio;
using ManagedBass.Wasapi;
using NeoSmart.AsyncLock;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
#nullable enable
namespace MajdataPlay.Recording
{
    public class FFmpegRecorder : IRecorder
    {
        public bool IsRecording { get; private set; } = false;
        public bool IsConnected { get; private set; } = true;


        bool _isDisposed = false;

        string _timestamp = string.Empty;
        string _wavPath = Path.Combine(MajEnv.RecordOutputsPath, $"{_defaultTimestamp}out.wav");
        string _mp4Path = Path.Combine(MajEnv.RecordOutputsPath, $"{_defaultTimestamp}out.mp4");
        string _outputPath = Path.Combine(MajEnv.RecordOutputsPath, $"MajdataPlay_gameplay_{_defaultTimestamp}.mp4");

        readonly WavRecorder _wavRecorder = new(32);
        readonly ScreenRecorder _screenRecorder = new();
        readonly static string _defaultTimestamp = $"{DateTime.UnixEpoch:yyyy-MM-dd_HH_mm_ss}";
        readonly static string FFMPEG_PATH = Path.Combine(MajEnv.AssetsPath, "ffmpeg.exe");


        public void StartRecord()
        {
            StopRecordAsync().Wait();
        }
        public async Task StartRecordAsync()
        {
            EnsureIsOpen();
            if (IsRecording)
            {
                return;
            }
            IsRecording = true;
            _timestamp = $"{DateTime.Now:yyyy-MM-dd_HH_mm_ss}";
            _wavPath = Path.Combine(MajEnv.RecordOutputsPath, $"{_timestamp}out.wav");
            _mp4Path = Path.Combine(MajEnv.RecordOutputsPath, $"{_timestamp}out.mp4");
            _outputPath = Path.Combine(MajEnv.RecordOutputsPath, $"MajdataPlay_gameplay_{_timestamp}.mp4");

            await _screenRecorder.StartRecordingAsync(_mp4Path);
            _wavRecorder.Start();
        }
        public void StopRecord()
        {
            StopRecordAsync().Wait();
        }
        public async Task StopRecordAsync()
        {
            EnsureIsOpen();
            IsRecording = false;
            _wavRecorder.Stop();
            using (var fileStream = File.Create(_wavPath))
            {
                await _wavRecorder.ExportAsync(fileStream);
            }
            await _screenRecorder.StopRecordingAsync();
            await Task.Run(() =>
            {
                var args = $"-y -i {_mp4Path} -i {_wavPath} -c:v copy -c:a aac -strict experimental -shortest {_outputPath}";
                var startInfo = new ProcessStartInfo(FFMPEG_PATH, args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = new Process { StartInfo = startInfo };
                p.Start();
                p.WaitForExit();
                _timestamp = string.Empty;
            });
        }
        public void OnLateUpdate()
        {
            EnsureIsOpen();
            if(!IsRecording)
            {
                return;
            }
            _wavRecorder.OnLateUpdate();
        }
        public void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }
            StopRecord();
            _wavRecorder.Dispose();
            _isDisposed = true;
        }
        void EnsureIsOpen()
        {
            if(_isDisposed)
            {
                throw new ObjectDisposedException(nameof(FFmpegRecorder));
            }
        }
        class WavRecorder : IDisposable
        {
            int _dataSize = 0;
            bool _isRecording = false;
            bool _isDisposed = false;
            
            readonly int _sampleRate;
            readonly int _channels;
            readonly int _bitsPerSample;
            readonly MemoryStream _stream = new(4096 * 10_0000);
            readonly ConcurrentQueue<Sample> _cachedSamples = new();

            public WavRecorder(int bitsPerSample)
            {
                var audioBackend = MajInstances.GameManager.Setting.Audio.Backend;
                if (audioBackend == SoundBackendType.Wasapi)
                {
                    BassWasapi.GetInfo(out var wasapiInfo);
                    _sampleRate = wasapiInfo.Frequency;
                    _channels = wasapiInfo.Channels;
                }
                else if (audioBackend == SoundBackendType.Asio)
                {
                    _channels = BassAsio.Info.Inputs;
                    _sampleRate = (int)BassAsio.Rate;
                }
                _bitsPerSample = bitsPerSample;
            }

            public void Start()
            {
                EnsureIsOpen();
                if (_isRecording)
                {
                    return;
                }
                try
                {
                    WriteHeader();
                    _isRecording = true;
                    AudioManager.OnBassProcessExtraLogic += HandleData;
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Failed to create WAV file: {e.Message}");
                    Stop();
                }
            }
            public void Stop()
            {
                AudioManager.OnBassProcessExtraLogic -= HandleData;
                EnsureIsOpen();
                WriteSampleIntoStream();
                Finish();
                _dataSize = 0;
                _stream.Position = 0;
            }
            public void OnLateUpdate()
            {
                EnsureIsOpen();
                if (_isRecording)
                    return;
                WriteSampleIntoStream();
            }
            public void Export(Stream stream)
            {
                EnsureIsOpen();
                if (_isRecording)
                {
                    Stop();
                }
                if(_dataSize == 0)
                {
                    return;
                }
                _stream.CopyTo(stream);
            }
            public async Task ExportAsync(Stream stream)
            {
                EnsureIsOpen();
                if (_isRecording)
                {
                    Stop();
                }
                if (_dataSize == 0)
                {
                    return;
                }
                await _stream.CopyToAsync(stream);
            }
            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }
                AudioManager.OnBassProcessExtraLogic -= HandleData;
                if (_isRecording)
                {
                    Stop();
                }
                _isDisposed = true;
                _stream?.Dispose();
            }



            void EnsureIsOpen()
            {
                if(_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(WavRecorder));
                }
            }
            void WriteSampleIntoStream()
            {
                lock (_cachedSamples)
                {
                    while (_cachedSamples.TryDequeue(out var sample))
                    {
                        using (sample)
                        {
                            var buffer = sample.Buffer;
                            if (buffer.IsEmpty)
                            {
                                continue;
                            }
                            _stream.Write(buffer);
                            _dataSize += buffer.Length;
                        }
                    }
                }
            }
            private void WriteHeader()
            {
                // RIFF header
                WriteString("RIFF");
                WriteInt(0); // Placeholder for file size
                WriteString("WAVE");

                // fmt chunk
                WriteString("fmt ");
                WriteInt(16); // PCM chunk size
                WriteShort((ushort)(_bitsPerSample == 32 ? 3 : 1)); // Audio format (3 = IEEE float)
                WriteShort((ushort)_channels);
                WriteInt(_sampleRate);
                WriteInt(_sampleRate * _channels * (_bitsPerSample / 8)); // Byte rate
                WriteShort((ushort)(_channels * (_bitsPerSample / 8))); // Block align
                WriteShort((ushort)_bitsPerSample);

                // data chunk header
                WriteString("data");
                WriteInt(0); // Placeholder for data size
            }
            private void HandleData(IntPtr buffer, int length, IntPtr user)
            {
                if (!_isRecording || _stream == null)
                    return;

                try
                {
                    _cachedSamples.Enqueue(new Sample(buffer, length));
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Error writing audio data: {e.Message}");
                    Dispose();
                }
            }
            private void Finish()
            {
                if (!_isRecording) 
                    return;

                try
                {
                    _isRecording = false;

                    // Update RIFF size
                    _stream.Seek(4, SeekOrigin.Begin);
                    WriteInt(_dataSize + 36); // 36 = total header size

                    // Update data chunk size
                    _stream.Seek(40, SeekOrigin.Begin);
                    WriteInt(_dataSize);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Error finalizing WAV file: {e.Message}");
                }
            }
            private unsafe void WriteShort(ushort value)
            {
                var ptr = stackalloc byte[2];
                *(short*)ptr = (short)value;

                _stream.Write(new ReadOnlySpan<byte>(ptr, 2));
            }
            private unsafe void WriteInt(int value)
            {
                var ptr = stackalloc byte[4];
                *(int*)ptr = (int)value;

                _stream.Write(BitConverter.GetBytes(value), 0, 4);
            }
            private void WriteString(string value)
            {
                _stream.Write(Encoding.ASCII.GetBytes(value), 0, value.Length);
            }
            struct Sample: IDisposable
            {
                public ReadOnlySpan<byte> Buffer
                {
                    get
                    {
                        if (_isDisposed)
                        {
                            throw new ObjectDisposedException(nameof(Sample));
                        }
                        var buffer = _buffer.AsSpan();
                        return buffer.Slice(0, _length);
                    }
                }
                bool _isDisposed;
                readonly byte[] _buffer;
                readonly int _length;
                public unsafe Sample(IntPtr buffer, int length)
                {
                    var data = new ReadOnlySpan<byte>((void*)buffer, length);
                    _length = length;
                    var pooledBuffer = ArrayPool<byte>.Shared.Rent(length);
                    data.CopyTo(pooledBuffer);
                    _buffer = pooledBuffer;
                    _isDisposed = false;
                }
                public void Dispose()
                {
                    if(_isDisposed)
                    {
                        return;
                    }
                    _isDisposed = true;
                    ArrayPool<byte>.Shared.Return(_buffer);
                }
            }
        }
        class ScreenRecorder
        {
            public bool IsRecording { get; private set; } = false;

            int _screenWidth = 1920;
            int _screenHeight = 1080;

            string _exportPath = Path.Combine(MajEnv.RecordOutputsPath, $"{_defaultTimestamp}out.mp4");
            Coroutine? _captureScreenTask = null;

            readonly AsyncLock _recodingSyncLock = new();
            readonly static string _defaultTimestamp = $"{DateTime.UnixEpoch:yyyy-MM-dd_HH_mm_ss}";

            public async UniTask StartRecordingAsync(string exportPath)
            {
                var task = _recodingSyncLock.LockAsync();
                while (!task.IsCompleted)
                {
                    await UniTask.Yield();
                }

                using (task.Result)
                {
                    var resolution = Screen.currentResolution;
                    var width = resolution.width;
                    var height = resolution.height;
                    if (width % 2 != 0)
                        width++;
                    if (height % 2 != 0)
                        height++;
                    if (width < 128)
                        width = 128;
                    if (height < 128)
                        height = 128;
                    Screen.SetResolution(width, height, false);
                    _screenWidth = width;
                    _screenHeight = height;
                    IsRecording = true;
                    _exportPath = exportPath;
                    _captureScreenTask = MajInstances.GameManager.StartCoroutine(CaptureScreenAsync());
                }
            }
            public async UniTask StopRecordingAsync()
            {
                var task = _recodingSyncLock.LockAsync();
                while (!task.IsCompleted)
                {
                    await UniTask.Yield();
                }
                using (task.Result)
                {
                    IsRecording = false;
                    if(_captureScreenTask is not null)
                    {
                        MajInstances.GameManager.StopCoroutine(_captureScreenTask);
                    }
                    MajInstances.GameManager.ApplyScreenConfig();
                }
            }

            IEnumerator CaptureScreenAsync()
            {
                var wait4EndOfFramePromise = new WaitForEndOfFrame();
                using (var pipeServer = new NamedPipeServerStream("MajdataPlayRec", PipeDirection.Out))
                {
                    var args =
                        $"-hide_banner -y -f rawvideo -vcodec rawvideo -pix_fmt rgba -s \"{_screenWidth}x{_screenHeight}\" -r 60 -i \\\\.\\pipe\\MajdataPlayRec -vf \"vflip\" -c:v libx264 -preset fast -pix_fmt yuv420p -t \"{int.MaxValue:0.0000}\" \"{_exportPath}\"";
                    var startinfo = new ProcessStartInfo(FFMPEG_PATH, args)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    startinfo.EnvironmentVariables.Add("FFREPORT", "file=out.log:level=24");

                    var p = Process.Start(startinfo);
                    var task = pipeServer.WaitForConnectionAsync();
                    while(!task.IsCompleted)
                    {
                        yield return wait4EndOfFramePromise;
                    }                    
                    const double FRAME_LENGTH_MSEC = 1.0 / 60.0 * 1000;
                    var lastPresentTexture = new Texture2D(0, 0);
                    var lastPresentTime = MajTimeline.UnscaledTime;
                    var behaviour = MajInstances.GameManager;

                    while (pipeServer.IsConnected && IsRecording && !p.HasExited)
                    {
                        yield return wait4EndOfFramePromise;
                        try
                        {
                            var now = MajTimeline.UnscaledTime;
                            var frameInterval = now - lastPresentTime;

                            Profiler.BeginSample("Capture Screenshot");
                            lastPresentTexture = ScreenCapture.CaptureScreenshotAsTexture();
                            Profiler.EndSample();
                            var data = lastPresentTexture.GetRawTextureData<byte>();
                            for (var i = 0; i < frameInterval.TotalMilliseconds % FRAME_LENGTH_MSEC; i++)
                            {
                                pipeServer.Write(data);
                            }
                            lastPresentTexture.Reinitialize(0, 0);
                        }
                        catch
                        {
                            // ignore single frame catching failed
                        }
                        finally
                        {
                            lastPresentTime = MajTimeline.UnscaledTime;
                        }
                        MajDebug.Log("Capturing");
                    }
                    p.WaitForExit();
                    MajDebug.Log("FFmpeg has exited");
                }
            }
        }
    }
}
