using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using ManagedBass.Asio;
using ManagedBass.Wasapi;
using NeoSmart.AsyncLock;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Recording
{
    public class FFmpegRecorder : MonoBehaviour, IRecorder
    {
        private static WavRecorder wavRecorder;
        private static ScreenRecorder screenRecorder;
        private static string FFMPEG_PATH = Path.Combine(MajEnv.AssetsPath, "ffmpeg.exe");
        private static string time = string.Empty;
        private static string wavPath => Path.Combine(MajEnv.RecordOutputsPath, $"{time}out.wav");
        private static string mp4Path => Path.Combine(MajEnv.RecordOutputsPath, $"{time}out.mp4");
        private static string outputPath => Path.Combine(MajEnv.RecordOutputsPath, $"{time}output.mp4");
        public bool IsRecording { get; set; } = false;
        public bool IsConnected { get; set; } = true;

        public async void StartRecord()
        {
            time = $"{DateTime.Now:yyyy-MM-dd_HH_mm_ss}";
            IsRecording = true;
            wavRecorder ??= new(wavPath, 32);
            if (screenRecorder == null)
            {
                GameObject recorder = new("ScreenRecorder");
                screenRecorder = recorder.AddComponent<ScreenRecorder>();
            }

            await screenRecorder.StartRecordingAsync(mp4Path);
            wavRecorder.Start();
        }

        public async void StopRecord()
        {
            IsRecording = false;
            await screenRecorder.StopRecordingAsync();
            wavRecorder?.Stop();
            Destroy(screenRecorder.gameObject);
            screenRecorder = null;
            wavRecorder?.Dispose();
            wavRecorder = null;
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                var args =
                    $"-y -i {mp4Path} -i {wavPath} -c:v copy -c:a aac -strict experimental -shortest {outputPath}";
                var startInfo = new ProcessStartInfo(FFMPEG_PATH, args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = new Process { StartInfo = startInfo };
                p.Start();
                p.WaitForExit();
                time = string.Empty;
            });
        }

        public void Dispose()
        {
            StopRecord();
            IsConnected = false;
            GC.SuppressFinalize(this);
        }

        private class WavRecorder : IDisposable
        {
            private FileStream _fileStream;
            private readonly string _filePath;
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
                _filePath = filePath;
            }

            public void Start()
            {
                try
                {
                    _fileStream = new FileStream(_filePath, FileMode.Create);
                    WriteHeader();
                    _isRecording = true;
                    AudioManager.OnBassProcessExtraLogic += wavRecorder.HandleData;
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"Failed to create WAV file: {e.Message}");
                    Dispose();
                }
            }

            public void Stop()
            {
                AudioManager.OnBassProcessExtraLogic -= wavRecorder.HandleData;
                wavRecorder.Finish();
                Dispose();
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
                if (!_isRecording || _fileStream == null)
                    return;

                try
                {
                    var data = new byte[length];
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

            private void Finish()
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

        internal class ScreenRecorder : MajComponent
        {
            public bool IsRecording { get; private set; }

            int _screenWidth = 1920;
            int _screenHeight = 1080;

            readonly AsyncLock _recodingSyncLock = new();

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
                    StartCoroutine(CaptureScreen(mp4Path));
                }
            }
            public async UniTask StopRecordingAsync()
            {
                IsRecording = false;
                MajInstances.GameManager.ApplyScreenConfig();
            }

            private IEnumerator CaptureScreen(string exportPath)
            {
                byte[] data;
                var texture = new Texture2D(0, 0);
                using (var pipeServer = new NamedPipeServerStream("MajdataPlayRec", PipeDirection.Out))
                {
                    var args =
                        $"-hide_banner -y -f rawvideo -vcodec rawvideo -pix_fmt rgba -s \"{_screenWidth}x{_screenHeight}\" -r 60 -i \\\\.\\pipe\\MajdataPlayRec -vf \"vflip\" -c:v libx264 -preset fast -pix_fmt yuv420p -t \"{int.MaxValue:0.0000}\" \"{exportPath}\"";
                    var startinfo = new ProcessStartInfo(FFMPEG_PATH, args)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    startinfo.EnvironmentVariables.Add("FFREPORT", "file=out.log:level=24");

                    var p = Process.Start(startinfo);
                    pipeServer.WaitForConnection();

                    const double targetFrameInterval = 1.0 / 60.0;
                    var stopwatch = Stopwatch.StartNew();
                    double nextFrameTime = 0;

                    using (var bw = new BinaryWriter(pipeServer))
                    {
                        while (pipeServer.IsConnected && IsRecording && !p.HasExited)
                        {
                            var currentTime = stopwatch.Elapsed.TotalSeconds;

                            if (currentTime < nextFrameTime)
                            {
                                var sleepTime = nextFrameTime - currentTime;
                                if (sleepTime > 0)
                                    yield return new WaitForSecondsRealtime((float)sleepTime);
                                continue;
                            }

                            nextFrameTime += targetFrameInterval;

                            yield return new WaitForEndOfFrame();

                            try
                            {
                                texture.Reinitialize(0, 0);
                                texture = ScreenCapture.CaptureScreenshotAsTexture();

                                data = texture.GetRawTextureData();
                                bw.Write(data, 0, data.Length);
                                bw.Flush();
                            }
                            catch
                            {
                                // ignore single frame catching failed
                            }
                        }
                    }
                    p.WaitForExit();
                }
            }
        }
    }
}
