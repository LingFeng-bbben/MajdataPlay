using MajdataPlay.Recording;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Utils
{
    internal static class RecordHelper
    {
        static IRecorder? _recorder;

        static RecordHelper()
        {
            var recorderType = MajEnv.UserSettings.Game.Recorder;
            _recorder = recorderType switch
            {
                BuiltInRecorder.FFmpeg => new FFmpegRecorder(),
                BuiltInRecorder.OBS => new OBSRecorder(),
                _ => null
            };
            MajEnv.OnApplicationQuit += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            if ((MajInstances.RecordHelper?.IsRecording ?? false)
                && MajInstances.RecordHelper.IsConnected)
                MajInstances.RecordHelper.StopRecord();
        }
    }
}
