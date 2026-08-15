using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools.Logging
{
    internal sealed class LogScope : ILogScope
    {
        private static List<LogScope> s_ActiveScopes = new List<LogScope>();

        private readonly object m_Lock = new object();
        private bool m_Disposed;
        private bool m_NeedToProcessLogs;

        public Queue<LogMatch> ExpectedLogs { get; set; }
        public List<LogEvent> AllLogs { get; }
        public List<LogEvent> FailingLogs { get; }
        public bool IgnoreFailingMessages { get; set; }
        public bool IsNUnitException { get; private set; }
        public bool IsNUnitSuccessException { get; private set; }
        public bool IsNUnitInconclusiveException { get; private set; }
        public bool IsNUnitIgnoreException { get; private set; }
        public string NUnitExceptionMessage { get; private set; }

        public static LogScope Current
        {
            get
            {
                if (s_ActiveScopes.Count == 0)
                    throw new InvalidOperationException("No log scope is available");
                return s_ActiveScopes[0];
            }
        }

        public static bool HasCurrentLogScope()
        {
            return s_ActiveScopes.Count > 0;
        }

        public LogScope()
        {
            AllLogs = new List<LogEvent>();
            FailingLogs = new List<LogEvent>();
            ExpectedLogs = new Queue<LogMatch>();
            IgnoreFailingMessages = false;
            Activate();
        }

        private void Activate()
        {
            s_ActiveScopes.Insert(0, this);
            Application.logMessageReceivedThreaded -= AddLog;
            // UDR0004: The AddLog subscription is managed through IDisposable:
            // Dispose() -> Deactivate() removes it at test end.
            // ResetStaticsOnLoad() (AfterAssembliesLoaded) intentionally
            // does not unsubscribe, so that logs emitted during
            // Fast Enter Play Mode initialization
            // (e.g. IProcessSceneWithReport, MonoBehaviour.Start) are captured before
            // EnsureActive() re-adds the scope to s_ActiveScopes. There is no stale-
            // subscriber risk because each LogScope is always disposed before the next
            // test creates a new one.
#pragma warning disable UDR0004
            Application.logMessageReceivedThreaded += AddLog;
#pragma warning restore UDR0004
        }

        private void Deactivate()
        {
            Application.logMessageReceivedThreaded -= AddLog;
            s_ActiveScopes.Remove(this);
        }

        public void AddLog(string message, string stacktrace, LogType type)
        {
            lock (m_Lock)
            {
                m_NeedToProcessLogs = true;
                var log = new LogEvent
                {
                    LogType = type,
                    Message = message,
                    StackTrace = stacktrace,
                };

                AllLogs.Add(log);

                if (IsNUnitResultStateException(stacktrace, type))
                {
                    if (message.StartsWith("SuccessException"))
                    {
                        IsNUnitException = true;
                        IsNUnitSuccessException = true;
                        if (message.StartsWith("SuccessException: "))
                        {
                            NUnitExceptionMessage = message.Substring("SuccessException: ".Length);
                            return;
                        }
                    }
                    else if (message.StartsWith("InconclusiveException"))
                    {
                        IsNUnitException = true;
                        IsNUnitInconclusiveException = true;
                        if (message.StartsWith("InconclusiveException: "))
                        {
                            NUnitExceptionMessage = message.Substring("InconclusiveException: ".Length);
                            return;
                        }
                    }
                    else if (message.StartsWith("IgnoreException"))
                    {
                        IsNUnitException = true;
                        IsNUnitIgnoreException = true;
                        if (message.StartsWith("IgnoreException: "))
                        {
                            NUnitExceptionMessage = message.Substring("IgnoreException: ".Length);
                            return;
                        }
                    }
                }

                if (IsFailingLog(type) && !IgnoreFailingMessages)
                {
                    FailingLogs.Add(log);
                }
            }
        }

        private static bool IsNUnitResultStateException(string stacktrace, LogType logType)
        {
            if (logType != LogType.Exception)
                return false;

            return string.IsNullOrEmpty(stacktrace) || stacktrace.StartsWith("NUnit.Framework.Assert.");
        }

        private static bool IsFailingLog(LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return true;
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            m_Disposed = true;

            if (disposing)
            {
                Deactivate();
            }
        }

        public bool AnyFailingLogs()
        {
            ProcessExpectedLogs();
            return FailingLogs.Any();
        }

        public void EvaluateLogScope(bool endOfScopeCheck)
        {
            ProcessExpectedLogs();
            if (FailingLogs.Any())
            {
                var failureInWrongOrder = FailingLogs.FirstOrDefault(log => ExpectedLogs.Any(expected => expected.Matches(log)));
                if (failureInWrongOrder != null)
                {
                    var nextExpected = ExpectedLogs.Peek();
                    throw new OutOfOrderExpectedLogMessageException(failureInWrongOrder, nextExpected);
                }

                var failingLog = FailingLogs.First();
                throw new UnhandledLogMessageException(failingLog);
            }
            if (endOfScopeCheck && ExpectedLogs.Any())
            {
                throw new UnexpectedLogMessageException(ExpectedLogs.Peek());
            }
        }

        public void ProcessExpectedLogs()
        {
            lock (m_Lock)
            {
                if (!m_NeedToProcessLogs || !ExpectedLogs.Any())
                    return;

                LogMatch expectedLog = null;
                foreach (var logEvent in AllLogs)
                {
                    if (!ExpectedLogs.Any())
                        break;
                    if (logEvent.IsHandled)
                    {
                        continue;
                    }
                    if (expectedLog == null && ExpectedLogs.Any())
                        expectedLog = ExpectedLogs.Peek();

                    if (expectedLog != null && expectedLog.Matches(logEvent))
                    {
                        ExpectedLogs.Dequeue();
                        logEvent.IsHandled = true;
                        if (FailingLogs.Any(expectedLog.Matches))
                        {
                            var failingLog = FailingLogs.First(expectedLog.Matches);
                            FailingLogs.Remove(failingLog);
                        }
                        expectedLog = null;
                    }
                }
                m_NeedToProcessLogs = false;
            }
        }

        public void NoUnexpectedReceived()
        {
            lock (m_Lock)
            {
                ProcessExpectedLogs();

                var unhandledLog = AllLogs.FirstOrDefault(x => !x.IsHandled);
                if (unhandledLog != null)
                {
                    throw new UnhandledLogMessageException(unhandledLog);
                }
            }
        }

        /// <summary>
        /// Re-activates this scope if it was cleared from the active scope list (e.g., by
        /// Fast Enter Play Mode's SubsystemRegistration reset). Calling this when the scope
        /// is already active is a no-op.
        /// </summary>
        internal void EnsureActive()
        {
            if (!s_ActiveScopes.Contains(this))
            {
                s_ActiveScopes.Insert(0, this);
            }
        }

#if UNITY_EDITOR
        // AfterAssembliesLoaded is used (not SubsystemRegistration) so that active log scopes
        // are still capturing when auto-generated statics cleanup runs at SubsystemRegistration.
        // That cleanup emits logs (e.g. "EnterPlayModeCount: 0") that tests expect via LogAssert.Expect.
        // By clearing at AfterAssembliesLoaded, those logs land in AllLogs before we deactivate the
        // scope, so EnsureActive() can re-register the scope and the match succeeds at test end.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ResetStaticsOnLoad()
        {
            s_ActiveScopes?.Clear();
        }
#endif
    }
}
