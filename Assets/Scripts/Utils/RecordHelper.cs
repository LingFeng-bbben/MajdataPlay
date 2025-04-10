using MajdataPlay.Recording;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Utils
{
    internal static class RecordHelper
    {
        public static bool IsRecording
        {
            get
            {
                return _recorder?.IsRecording ?? false;
            }
        }
        public static bool IsConnected
        {
            get
            {
                return _recorder.IsConnected;
            }
        }
        public static bool IsEnabled
        {
            get
            {
                return MajEnv.UserSettings.Game.RecordMode == RecordMode.TrackStart;
            }
        }

        static IRecorder _recorder;
        static RecordHelper()
        {
            var recorderType = MajEnv.UserSettings.Game.Recorder;
            _recorder = recorderType switch
            {
                BuiltInRecorder.FFmpeg => new FFmpegRecorder(),
                BuiltInRecorder.OBS => new OBSRecorder(),
                _ => throw new ArgumentOutOfRangeException()
            };
            MajEnv.OnApplicationQuit += OnApplicationQuit;
        }
        public static void StartRecord()
        {
            if(!IsEnabled)
            {
                return;
            }
            _recorder.StartRecord();
        }
        public static async Task StartRecordAsync()
        {
            if (!IsEnabled)
            {
                return;
            }
            await _recorder.StartRecordAsync();
        }
        public static void StopRecord()
        {
            if (!IsEnabled)
            {
                return;
            }
            _recorder.StopRecord();
        }
        public static async Task StopRecordAsync()
        {
            if (!IsEnabled)
            {
                return;
            }
            await _recorder.StopRecordAsync();
        }
        internal static void OnLateUpdate()
        {
            _recorder.OnLateUpdate();
        }
        private static void OnApplicationQuit()
        {
            _recorder.StopRecordAsync();
        }
    }
}
