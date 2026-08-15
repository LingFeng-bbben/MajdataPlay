using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Unity.VisualScripting
{
    public static class UnityThread
    {
        public static Thread thread = Thread.CurrentThread;

        public static Action<Action> editorAsync;

        public static bool allowsAPI => !Serialization.isUnitySerializing && Thread.CurrentThread == thread;

        public static ConcurrentQueue<Action> pendingQueue = new ConcurrentQueue<Action>();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {
            thread = Thread.CurrentThread;
            editorAsync = null;
            pendingQueue = new ConcurrentQueue<Action>();
        }

        [Conditional("UNITY_EDITOR")]
        public static void EditorAsync(Action action)
        {
            if (editorAsync == null)
                pendingQueue.Enqueue(action);
            else
                editorAsync.Invoke(action);
        }
    }
}
