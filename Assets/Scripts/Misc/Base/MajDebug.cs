using Cysharp.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;
#nullable enable
namespace MajdataPlay
{
    public static class MajDebug
    {
        readonly static object _lockObject = new();
        readonly static ConcurrentQueue<GameLog> _logQueue = new();
        readonly static ILogger _unityLogger;
        static StreamWriter? _fileStream;

        static MajDebug()
        {
            _unityLogger = Debug.unityLogger;
            TaskScheduler.UnobservedTaskException += (sender,args) =>
            {
                LogException(args.Exception);
                args.SetObserved();
            };
            StartLogWritebackTask();
            MajEnv.OnApplicationQuit += OnApplicationQuit;
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
            try
            {
                lock (_lockObject)
                {
                    if (_fileStream is null)
                        return;
                    using (_fileStream)
                    {
                        foreach (var log in _logQueue)
                        {
                            _fileStream.WriteLine($"[{log.Date:yyyy-MM-dd HH:mm:ss}][{log.Level}] {log.Condition}\n{log.StackTrace}\n");
                        }
                    }
                }
            }
            finally
            {
                MajEnv.OnApplicationQuit -= OnApplicationQuit;
            }
        }
        static string GetStackTrack()
        {
            return new StackTrace(2, true).ToString();
        }
        static void StartLogWritebackTask()
        {
            Task.Factory.StartNew(() =>
            {
                var currentThread = Thread.CurrentThread;
                var token = MajEnv.GlobalCT;
                var oldLogPath = MajEnv.LogPath+ ".old";
                if (!Directory.Exists(MajEnv.LogsPath))
                    Directory.CreateDirectory(MajEnv.LogsPath);
                if (File.Exists(oldLogPath))
                    File.Delete(oldLogPath);
                if (File.Exists(MajEnv.LogPath))
                    File.Move(MajEnv.LogPath, oldLogPath);

                currentThread.Priority = System.Threading.ThreadPriority.Lowest;
                currentThread.IsBackground = true;
                _fileStream = new StreamWriter(MajEnv.LogPath, append: true, encoding: Encoding.UTF8);
                using (_fileStream)
                {
                    while (true)
                    {
                        try
                        {
                            lock (_lockObject)
                            {
                                if (token.IsCancellationRequested)
                                    return;
                                while (_logQueue.TryDequeue(out var log))
                                {
                                    var msg = ZString.Format("[{0:yyyy-MM-dd HH:mm:ss.ffff}][{1}] {2}\n{3}\n", log.Date, log.Level, log.Condition, log.StackTrace);
                                    _fileStream.WriteLine(msg);
                                }
                            }
                        }
                        finally
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
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
