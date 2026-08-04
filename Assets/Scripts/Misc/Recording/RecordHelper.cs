using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Settings;

#nullable enable
namespace MajdataPlay.Recording
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
#if UNITY_STANDALONE
                return MajEnv.Settings.Game.RecordMode == RecordModeOption.OBSTrigger;
#else
                return false;
#endif
            }
        }

        static IRecorder _recorder;
        static RecordHelper()
        {
            _recorder = new OBSRecorder();
            GameManager.OnAppQuit += OnApplicationQuit;
        }
        public static void StartRecord(string filename)
        {
            if(!IsEnabled || !_recorder.IsConnected)
            {
                return;
            }
            _recorder.SetOutputName(filename);
            _recorder.StartRecord();
        }
        public static async Task StartRecordAsync(string filename)
        {
            if (!IsEnabled || !_recorder.IsConnected)
            {
                return;
            }
            _recorder.SetOutputName(filename);
            await _recorder.StartRecordAsync();
        }
        public static void StopRecord()
        {
            if (!IsEnabled || !_recorder.IsConnected)
            {
                return;
            }
            _recorder.StopRecord();
        }
        public static async Task StopRecordAsync()
        {
            if (!IsEnabled || !_recorder.IsConnected)
            {
                return;
            }
            await _recorder.StopRecordAsync();
        }
        internal static void OnLateUpdate()
        {
            _recorder.OnLateUpdate();
        }
        private static void OnApplicationQuit(object? sender, EventArgs? e)
        {
            if (!IsEnabled || !_recorder.IsConnected)
            {
                return;
            }
            _recorder.StopRecord();
            GameManager.OnAppQuit -= OnApplicationQuit;
        }
    }
}
