using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using NeoSmart.AsyncLock;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.View
{
    internal class ScreenRecorder: MajComponent
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

            if(currentHeight != _screenHeight || currentWidth != _screenWidth)
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
        async UniTask StartFFmpegAsync(string exportPath,string exportedAudioPath)
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
                if(_ffmpegProcess is not null)
                {
                    while(_ffmpegProcess.HasExited)
                    {
                        await UniTask.Yield();
                    }
                }
            }
        }
    }
}
