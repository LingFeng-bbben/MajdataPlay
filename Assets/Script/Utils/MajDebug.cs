using Cysharp.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class MajDebug
    {
        readonly static Queue<GameLog> _logQueue = new(2048);
        readonly static ILogger _unityLogger;

        static MajDebug()
        {
            _unityLogger = Debug.unityLogger;
            
            LogWriteback();
#if !(UNITY_EDITOR || DEBUG)
            Application.logMessageReceivedThreaded += (string condition, string stackTrace, LogType type) =>
            {
                MajDebug.Log($"{condition}\n{stackTrace}", type);
            };
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log<T>(T obj,LogType logLevel = LogType.Log)
        {
            var message = obj?.ToString() ?? string.Empty;
            var log = new GameLog()
            {
                Date = DateTime.Now,
                Condition = message,
                StackTrace = string.Empty,
                Level = logLevel
            };

#if UNITY_EDITOR || DEBUG
            _unityLogger.Log(logLevel, message);
            if(obj is not Exception)
                log.StackTrace = GetStackTrack();
#endif
            _logQueue.Enqueue(log);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError<T>(T obj) => Log(obj, LogType.Error);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException<T>(T obj) where T: Exception => Log(obj, LogType.Exception);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning<T>(T obj) => Log(obj, LogType.Warning);
        public static void OnApplicationQuit()
        {
            foreach (var log in _logQueue)
                File.AppendAllText(MajEnv.LogPath, $"[{log.Date:yyyy-MM-dd HH:mm:ss}][{log.Level}] {log.Condition}\n{log.StackTrace}\n");
        }
        static string GetStackTrack()
        {
            return new StackTrace(2, true).ToString();
        }
        static async Task LogWriteback()
        {
            var token = MajEnv.GlobalCT;
            var oldLogPath = Path.Combine(MajEnv.AssestPath, "MajPlayRuntime.log");
            if (!Directory.Exists(MajEnv.LogsPath))
                Directory.CreateDirectory(MajEnv.LogsPath);
            if (File.Exists(oldLogPath))
                File.Delete(oldLogPath);
            if (File.Exists(MajEnv.LogPath))
                File.Delete(MajEnv.LogPath);
           
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                if (_logQueue.Count == 0)
                {
                    await Task.Delay(50);
                    continue;
                }
                var log = _logQueue.Dequeue();
                var msg = ZString.Format("[{0:yyyy-MM-dd HH:mm:ss.ffff}][{1}] {2}\n{3}\n", log.Date, log.Level, log.Condition, log.StackTrace);
                await File.AppendAllTextAsync(MajEnv.LogPath, msg);
            }
        }
        struct GameLog
        {
            public DateTime Date { get; set; }
            public string? Condition { get; set; }
            public string? StackTrace { get; set; }
            public LogType Level { get; set; }
        }
    }
}
