using Cysharp.Text;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Diagnostics
{
    public static class MajDebug
    {
        public static long MaxLogSize { get; set; } = 500L * 1024 * 1024; // 500 MB
        public static LogLevel MinLogLevel { get; set; } = LogLevel.Debug;

        static ILogger _unityLogger = null!;
        static TextWriter? _logWriter;
        static readonly CancellationTokenSource _cancellationTokenSource = new();

        readonly static Utf16PreparedFormat<DateTime, LogLevel> LOG_OUTPUT_FORMAT = ZString.PrepareUtf16<DateTime, LogLevel>("[{0:yyyy-MM-dd HH:mm:ss.ffff}][{1}]");

        readonly static object _lockObject = new();
        readonly static ConcurrentQueue<GameLog> _logQueue = new();


        public static void SetLogWriter(TextWriter? writer, bool disposeOld = true)
        {
            lock (_lockObject)
            {
                if (disposeOld && _logWriter != null)
                {
                    _logWriter.Flush();
                    _logWriter.Dispose();
                }
                _logWriter = writer;
            }
        }

        public static void FlushLog()
        {
            lock (_lockObject)
            {
                if (_logWriter == null)
                {
                    return;
                }
                var sb = ZString.CreateStringBuilder();
                try
                {
                    WriteLogIntoStream(ref sb);
                    _logWriter.Flush();
                }
                finally
                {
                    sb.Dispose();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            _unityLogger = UnityEngine.Debug.unityLogger;

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                LogException(args.Exception);
                args.SetObserved();
            };

            Application.quitting += () =>
            {
                _cancellationTokenSource.Cancel();
                FlushLog();
            };

            StartLogWritebackTask();

            Application.logMessageReceivedThreaded += (string condition, string stackTrace, LogType type) =>
            {
                var sb = ZString.CreateStringBuilder();
                sb.Append(condition);
                var log = new GameLog()
                {
                    Date = DateTime.Now,
                    Condition = sb,
                    StackTrace = stackTrace,
                    Level = ToMajdataLogLevel(type),
                    IsFromUnityLogger = true
                };
                _logQueue.Enqueue(log);
            };
        }

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Log<T>(string? tag, T obj, LogLevel level)
        {
            var sb = ZString.CreateStringBuilder();
            if (!string.IsNullOrEmpty(tag))
            {
                sb.Append('[');
                sb.Append(tag);
                sb.Append(']');
            }
            sb.Append(obj);
            if (obj is Exception)
            {
                sb.AppendLine();
            }
            var log = new GameLog()
            {
                Date = DateTime.Now,
                Condition = sb,
#if UNITY_EDITOR || DEBUG
                StackTrace = obj is not Exception ? GetStackTrack() : string.Empty,
#else
                StackTrace = string.Empty,
#endif
                Level = level
            };
            _logQueue.Enqueue(log);
        }

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogDebug<T>(T obj) => Log(null, obj, LogLevel.Debug);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogDebug<T>(string tag, T obj) => Log(tag, obj, LogLevel.Debug);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogInfo<T>(T obj) => Log(null, obj, LogLevel.Info);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogInfo<T>(string tag, T obj) => Log(tag, obj, LogLevel.Info);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning<T>(T obj) => Log(null, obj, LogLevel.Warning);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning<T>(string tag, T obj) => Log(tag, obj, LogLevel.Warning);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError<T>(T obj) => Log(null, obj, LogLevel.Error);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError<T>(string tag, T obj) => Log(tag, obj, LogLevel.Error);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException<T>(T obj) where T : Exception => Log(null, obj, LogLevel.Error);

        [HideInCallstack]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException<T>(string tag, T obj) where T : Exception
            => Log(tag, obj, LogLevel.Error);


        static string GetStackTrack()
        {
            return new StackTrace(3, true).ToString();
        }

        static void StartLogWritebackTask()
        {
            Task.Factory.StartNew(() =>
            {
                var currentThread = Thread.CurrentThread;
                currentThread.Priority = System.Threading.ThreadPriority.Lowest;
                currentThread.IsBackground = true;

                var token = _cancellationTokenSource.Token;
                var sb = ZString.CreateStringBuilder();
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            WriteLogIntoStream(ref sb);
                        }
                        finally
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                finally
                {
                    sb.Dispose();
                    lock (_lockObject)
                    {
                        _logWriter?.Dispose();
                        _logWriter = null;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        static void WriteLogIntoStream(ref Utf16ValueStringBuilder sb)
        {
            lock (_lockObject)
            {
                if (_logWriter is null)
                {
                    while (_logQueue.Count > 10000 && _logQueue.TryDequeue(out var staleLog))
                    {
                        staleLog.Condition.Dispose();
                    }
                    return;
                }

                var hasWritten = false;
                while (_logQueue.TryDequeue(out var log))
                {
                    using var condition = log.Condition;

                    if (log.Level < MinLogLevel)
                    {
                        continue;
                    }

                    if (MaxLogSize > 0 && _logWriter is StreamWriter sw && sw.BaseStream.CanSeek)
                    {
                        if (sw.BaseStream.Position > MaxLogSize)
                        {
                            continue;
                        }
                    }

                    LOG_OUTPUT_FORMAT.FormatTo(ref sb, log.Date, log.Level);
                    sb.Append(condition.AsSpan());

                    if (!string.IsNullOrEmpty(log.StackTrace))
                    {
                        sb.AppendLine();
                        sb.Append(log.StackTrace);
                        sb.AppendLine();
                    }

                    _logWriter.WriteLine(sb.AsSpan());
                    hasWritten = true;

#if UNITY_EDITOR || DEBUG
                    if (!log.IsFromUnityLogger)
                    {
                        _unityLogger.Log(ToUnityLogLevel(log.Level), sb.ToString());
                    }
#endif
                    sb.Clear();
                }

                if (hasWritten)
                {
                    _logWriter.Flush();
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

        readonly struct GameLog
        {
            public DateTime Date { get; init; }
            public Utf16ValueStringBuilder Condition { get; init; }
            public string? StackTrace { get; init; }
            public LogLevel Level { get; init; }
            public bool IsFromUnityLogger { get; init; }
        }
    }
}