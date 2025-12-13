using Cysharp.Text;
using MajdataPlay.Settings;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        readonly static Utf16PreparedFormat<DateTime, LogLevel> LOG_OUTPUT_FORMAT = ZString.PrepareUtf16<DateTime, LogLevel>("[{0:yyyy-MM-dd HH:mm:ss.ffff}][{1}] ");

        static bool _isInited = false;
        readonly static object _initLock = new();

        static MajDebug()
        {
            _unityLogger = Debug.unityLogger;
        }
        internal static void Init()
        {
            if (_isInited)
            {
                return;
            }
            lock(_initLock)
            {
                if (_isInited)
                {
                    return;
                }
                _isInited = true;
            }
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                LogException(args.Exception);
                args.SetObserved();
            };
            StartLogWritebackTask();
            MajEnv.OnApplicationQuit += OnApplicationQuit;
            MajEnv.OnSave += OnSave;
            Application.logMessageReceivedThreaded += (string condition, string stackTrace, LogType type) =>
            {
                var sb = ZString.CreateStringBuilder();
                sb.Append(condition);
                var log = new GameLog()
                {
                    Date = DateTime.Now,
                    Condition = sb,
                    StackTrace = stackTrace,
                    Level = ToMajdataLogLevel(type)
                };
                _logQueue.Enqueue(log);
            };
        }


        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Log<T>(T obj, LogLevel level)
        {
            var sb = ZString.CreateStringBuilder();
            sb.Append(obj);
            if(obj is Exception)
            {
                sb.AppendLine();
            }
            var log = new GameLog()
            {
                Date = DateTime.Now,
                Condition = sb,
                StackTrace = string.Empty,
                Level = level
            };
#if UNITY_EDITOR || (UNITY_ANDROID && DEBUG)
            _unityLogger.Log(ToUnityLogLevel(level), sb.ToString());
#endif
#if UNITY_EDITOR || DEBUG
            if (obj is not Exception)
            {
                log.StackTrace = GetStackTrack();
            }
#endif
            _logQueue.Enqueue(log);
        }
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogDebug<T>(T obj)
        {
            Log(obj, LogLevel.Debug);
        }
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogInfo<T>(T obj)
        {
            Log(obj, LogLevel.Info);
        }
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning<T>(T obj)
        {
            Log(obj, LogLevel.Warning);
        }
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError<T>(T obj)
        {
            Log(obj, LogLevel.Error);
        }
        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException<T>(T obj) where T: Exception
        {
            Log(obj, LogLevel.Error);
        }
        
        static void OnApplicationQuit()
        {
            try
            {
                if (_fileStream is null)
                {
                    return;
                }
                var sb = ZString.CreateStringBuilder();
                try
                {
                    WriteLog(ref sb);
                }
                finally
                {
                    _fileStream.Dispose();
                    sb.Dispose();
                }
            }
            finally
            {
                MajEnv.OnApplicationQuit -= OnApplicationQuit;
            }
        }
        static void OnSave()
        {
            if (_fileStream is null)
            {
                return;
            }
            var sb = ZString.CreateStringBuilder();
            WriteLog(ref sb);
        }
        static string GetStackTrack()
        {
            return new StackTrace(3, true).ToString();
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
                _fileStream.AutoFlush = true;
                var sb = ZString.CreateStringBuilder();
                try
                {
                    while (true)
                    {
                        try
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            WriteLog(ref sb);
                        }
                        finally
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                finally
                {
                    _fileStream.Dispose();
                    sb.Dispose();
                }
            }, TaskCreationOptions.LongRunning);
        }
        static void WriteLog(ref Utf16ValueStringBuilder sb)
        {
            if(_fileStream is null)
            {
                throw new InvalidOperationException("Log file stream is not initialized. Ensure that StartLogWritebackTask has been called.");
            }
            lock (_lockObject)
            {
                while (_logQueue.TryDequeue(out var log))
                {
                    using var condition = log.Condition;
                    if(log.Level < (MajEnv.Settings?.Debug.DebugLevel ?? LogLevel.Debug))
                    {
                        continue;
                    }
                    LOG_OUTPUT_FORMAT.FormatTo(ref sb, log.Date, log.Level);
                    sb.Append(condition.AsSpan());
                    if (!string.IsNullOrEmpty(log.StackTrace))
                    {
                        sb.AppendLine();
                        sb.Append(log.StackTrace);
                        sb.AppendLine();
                    }
                    _fileStream.WriteLine(sb.AsSpan());
                    sb.Clear();
                }
            }
        }
        static LogType ToUnityLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => LogType.Log,
                LogLevel.Info => LogType.Log,
                LogLevel.Warning => LogType.Warning,
                LogLevel.Error => LogType.Error,
                LogLevel.Fatal => LogType.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
            };
        }
        static LogLevel ToMajdataLogLevel(LogType level)
        {
            return level switch
            {
                LogType.Log => LogLevel.Info,
                LogType.Warning => LogLevel.Warning,
                LogType.Error => LogLevel.Error,
                LogType.Exception => LogLevel.Error,
                _ => LogLevel.Debug
            };
        }
        struct GameLog
        {
            public DateTime Date { get; init; }
            public Utf16ValueStringBuilder Condition { get; init; }
            public string? StackTrace { get; set; }
            public LogLevel Level { get; init; }
        }
        internal enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }
    }
}
