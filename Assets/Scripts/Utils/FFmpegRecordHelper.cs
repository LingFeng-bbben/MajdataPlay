using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using ManagedBass.Asio;
using ManagedBass.Wasapi;
using NeoSmart.AsyncLock;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
    public class FFmpegRecordHelper : IRecordHelper
    {
        private static WavRecorder wavRecorder;
        public bool Recording { get; set; } = false;
        public bool Connected { get; set; } = true;

        private bool _disposed = false;

        public void StartRecord()
        {
            Recording = true;
            StartRecordWav();
        }

        public void StopRecord()
        {
            Recording = false;
            StopRecordWav();
        }

        private static void StartRecordWav()
        {
            wavRecorder ??= new("D:/test.wav", 32);
            AudioManager.OnBassProcessExtraLogic += wavRecorder.HandleData;
        }

        private static void StopRecordWav()
        {
            if (wavRecorder == null) return;
            AudioManager.OnBassProcessExtraLogic -= wavRecorder.HandleData;
            wavRecorder.Finish();
            wavRecorder.Dispose();
            wavRecorder = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                StopRecordWav();
                // 以后可能需要的释放资源
            }

            _disposed = true;
        }

        private class WavRecorder : IDisposable
        {
            private FileStream _fileStream;
            private readonly int _sampleRate;
            private readonly int _channels;
            private readonly int _bitsPerSample;
            private bool _isRecording;
            private int _dataSize;

            public WavRecorder(string filePath, int bitsPerSample)
            {
                if (MajInstances.GameManager.Setting.Audio.Backend == SoundBackendType.Wasapi)
                {
                    BassWasapi.GetInfo(out var wasapiInfo);
                    _sampleRate = wasapiInfo.Frequency;
                    _channels = wasapiInfo.Channels;
                }
                else if (MajInstances.GameManager.Setting.Audio.Backend == SoundBackendType.Asio)
                {
                    _channels = BassAsio.Info.Inputs;
                    _sampleRate = (int)BassAsio.Rate;
                }
                _bitsPerSample = bitsPerSample;

                try
                {
                    _fileStream = new FileStream(filePath, FileMode.Create);
                    WriteHeader();
                    _isRecording = true;
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Failed to create WAV file: {e.Message}");
                    Dispose();
                }
            }

            public void Start() { }

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

            public void HandleData(IntPtr buffer, int length, IntPtr user)
            {
                if (!_isRecording || _fileStream == null) return;

                try
                {
                    byte[] data = new byte[length];
                    Marshal.Copy(buffer, data, 0, length);
                    _fileStream.Write(data, 0, length);
                    _dataSize += length;
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Error writing audio data: {e.Message}");
                    Dispose();
                }
            }

            public void Finish()
            {
                if (!_isRecording || _fileStream == null) return;

                try
                {
                    _isRecording = false;
                    _fileStream.Flush();

                    // Update RIFF size
                    _fileStream.Seek(4, SeekOrigin.Begin);
                    WriteInt(_dataSize + 36); // 36 = total header size

                    // Update data chunk size
                    _fileStream.Seek(40, SeekOrigin.Begin);
                    WriteInt(_dataSize);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Error finalizing WAV file: {e.Message}");
                }
                finally
                {
                    Dispose();
                }
            }

            private void WriteShort(ushort value)
            {
                _fileStream.Write(BitConverter.GetBytes(value), 0, 2);
            }

            private void WriteInt(int value)
            {
                _fileStream.Write(BitConverter.GetBytes(value), 0, 4);
            }

            private void WriteString(string value)
            {
                _fileStream.Write(Encoding.ASCII.GetBytes(value), 0, value.Length);
            }

            public void Dispose()
            {
                _fileStream?.Dispose();
                _fileStream = null;
            }
        }

        private class ScreenRecorder : MajComponent
        {
            public bool IsRecording { get; private set; }

            int _screenWidth = 1920;
            int _screenHeight = 1080;

            Process? _ffmpegProcess = null;

            readonly AsyncLock _recodingSyncLock = new();
            readonly ConcurrentQueue<byte[]> _capturedScreenData = new();

            const string FFMPEG_ARGUMENTS = "-hide_banner -y -f rawvideo -vcodec rawvideo -pix_fmt rgba -s \"{0}x{1}\" -r 60 -i \\\\.\\pipe\\majdataRec -i \"{2}\" -vf \"vflip\" -c:v libx264 -preset fast -pix_fmt yuv420p -t \"{4:0.0000}\" -b:a 320k -c:a aac \"{3}\"";
            readonly string FFMPEG_PATH = Path.Combine(MajEnv.AssetsPath, "ffmpeg.exe");

            internal void OnLateUpdate()
            {
                if (!IsRecording)
                    return;
                var currentSoultion = Screen.currentResolution;
                var currentWidth = currentSoultion.width;
                var currentHeight = currentSoultion.height;

                if (currentHeight != _screenHeight || currentWidth != _screenWidth)
                {
                    Screen.SetResolution(_screenWidth, _screenHeight, false);
                }

                _capturedScreenData.Enqueue(ScreenCapture.CaptureScreenshotAsTexture().GetRawTextureData());
            }
            internal async UniTask StartRecordingAsync(string exportPath)
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
                }
            }
            async UniTask StartFFmpegAsync(string exportPath, string exportedAudioPath)
            {
                var task = Task.Run(() =>
                {
                    var args = string.Format(FFMPEG_ARGUMENTS, _screenWidth, _screenHeight, exportedAudioPath, int.MaxValue);
                    var startinfo = new ProcessStartInfo(FFMPEG_PATH, args);
                    startinfo.UseShellExecute = false;
                    startinfo.CreateNoWindow = true;
                    //startinfo.WorkingDirectory = maidata_path;
                    startinfo.EnvironmentVariables.Add("FFREPORT", "file=out.log:level=24");

                    _ffmpegProcess = Process.Start(startinfo);
                });

                while (!task.IsCompleted)
                    await UniTask.Yield();
            }
            async Task FramePresentAsync()
            {
                await Task.Run(async () =>
                {
                    using var pipeServer = new NamedPipeServerStream("majdataRec", PipeDirection.Out);
                    await pipeServer.WaitForConnectionAsync();
                    do
                    {
                        while (_capturedScreenData.TryDequeue(out var data))
                        {
                            await pipeServer.WriteAsync(data);
                        }
                        if (IsRecording)
                        {
                            await Task.Yield();
                        }
                        else
                        {
                            break;
                        }
                    } while (!(_ffmpegProcess?.HasExited ?? true) && pipeServer.IsConnected);
                    MajDebug.LogWarning("FFmpeg has exited");
                });
            }
            internal async UniTask StopRecordingAsync()
            {
                var task = _recodingSyncLock.LockAsync();
                while (!task.IsCompleted)
                {
                    await UniTask.Yield();
                }
                using (task.Result)
                {
                    if (!IsRecording)
                        return;
                    IsRecording = false;
                    if (_ffmpegProcess is not null)
                    {
                        while (_ffmpegProcess.HasExited)
                        {
                            await UniTask.Yield();
                        }
                    }
                }
            }
        }
    }
}
