using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Editor;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    internal class NoteUpdater : MonoBehaviour
    {
        public double PreUpdateElapsedMs => IntPreUpdateElapsedMs;
        public double UpdateElapsedMs => IntUpdateElapsedMs;
        public double FixedUpdateElapsedMs => IntFixedUpdateElapsedMs;
        public double LateUpdateElapsedMs => IntLateUpdateElapsedMs;

        protected ReadOnlyMemory<NoteInfo> Components = ReadOnlyMemory<NoteInfo>.Empty;
        protected ReadOnlyMemory<NoteInfo> PreUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
        protected ReadOnlyMemory<NoteInfo> UpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
        protected ReadOnlyMemory<NoteInfo> FixedUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
        protected ReadOnlyMemory<NoteInfo> LateUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;

        NoteInfo[] _rentedArrayForComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForPreUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForFixedUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForLateUpdatebleComponents = Array.Empty<NoteInfo>();

        [ReadOnlyField]
        [SerializeField]
        protected double IntPreUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        protected double IntUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        protected double IntFixedUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        protected double IntLateUpdateElapsedMs = 0;

        readonly static List<MonoBehaviour> SHARED_CACHE_LIST = new(64);
        public virtual async UniTask InitAsync()
        {
            await UniTask.SwitchToMainThread();
            var children = transform.GetChildren();

            using RentedList<NoteInfo> noteComponents = new();
            using RentedList<NoteInfo> preUpdatableComponents = new();
            using RentedList<NoteInfo> updatableComponents = new();
            using RentedList<NoteInfo> fixedUpdatableComponents = new();
            using RentedList<NoteInfo> lateUpdatableComponents = new();
            using RentedList<MonoBehaviour> components = new();

            foreach (var child in children)
            {
                child.GetComponents<MonoBehaviour>(SHARED_CACHE_LIST);
                if (SHARED_CACHE_LIST.Count != 0)
                {
                    components.AddRange(SHARED_CACHE_LIST);
                }
                SHARED_CACHE_LIST.Clear();
            }
            await UniTask.SwitchToThreadPool();
            foreach (var component in components)
            {
                var noteInfo = new NoteInfo(component);
                if (noteInfo.IsValid)
                {
                    if (noteInfo.IsUpdatable)
                    {
                        updatableComponents.Add(noteInfo);
                    }
                    if (noteInfo.IsFixedUpdatable)
                    {
                        fixedUpdatableComponents.Add(noteInfo);
                    }
                    if (noteInfo.IsLateUpdatable)
                    {
                        lateUpdatableComponents.Add(noteInfo);
                    }
                    if (noteInfo.IsPreUpdatable)
                    {
                        preUpdatableComponents.Add(noteInfo);
                    }
                    noteComponents.Add(noteInfo);
                }
                else
                {
                    noteInfo.Dispose();
                }
            }
            
            _rentedArrayForComponents = Pool<NoteInfo>.RentArray(noteComponents.Count, true);
            _rentedArrayForPreUpdatebleComponents = Pool<NoteInfo>.RentArray(preUpdatableComponents.Count, true);
            _rentedArrayForUpdatebleComponents = Pool<NoteInfo>.RentArray(updatableComponents.Count, true);
            _rentedArrayForFixedUpdatebleComponents = Pool<NoteInfo>.RentArray(fixedUpdatableComponents.Count, true);
            _rentedArrayForLateUpdatebleComponents = Pool<NoteInfo>.RentArray(lateUpdatableComponents.Count, true);

            noteComponents.CopyTo(_rentedArrayForComponents);
            preUpdatableComponents.CopyTo(_rentedArrayForPreUpdatebleComponents);
            updatableComponents.CopyTo(_rentedArrayForUpdatebleComponents);
            fixedUpdatableComponents.CopyTo(_rentedArrayForFixedUpdatebleComponents);
            lateUpdatableComponents.CopyTo(_rentedArrayForLateUpdatebleComponents);

            Components = _rentedArrayForComponents.AsMemory(0, noteComponents.Count);
            PreUpdatebleComponents = _rentedArrayForPreUpdatebleComponents.AsMemory(0, preUpdatableComponents.Count);
            UpdatebleComponents = _rentedArrayForUpdatebleComponents.AsMemory(0, updatableComponents.Count);
            FixedUpdatebleComponents = _rentedArrayForFixedUpdatebleComponents.AsMemory(0, fixedUpdatableComponents.Count);
            LateUpdatebleComponents = _rentedArrayForLateUpdatebleComponents.AsMemory(0, lateUpdatableComponents.Count);
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

        internal virtual void Clear()
        {
            foreach (var component in Components.Span)
            {
                component.Dispose();
            }

            Components = ReadOnlyMemory<NoteInfo>.Empty;
            PreUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
            UpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
            FixedUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
            LateUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;

            Pool<NoteInfo>.ReturnArray(_rentedArrayForComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForPreUpdatebleComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForUpdatebleComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForFixedUpdatebleComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForLateUpdatebleComponents, true);

            _rentedArrayForComponents = Array.Empty<NoteInfo>();
            _rentedArrayForPreUpdatebleComponents = Array.Empty<NoteInfo>();
            _rentedArrayForUpdatebleComponents = Array.Empty<NoteInfo>();
            _rentedArrayForFixedUpdatebleComponents = Array.Empty<NoteInfo>();
            _rentedArrayForLateUpdatebleComponents = Array.Empty<NoteInfo>();
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnPreUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var preUpdatebleComponents = PreUpdatebleComponents.Span;
            var len = preUpdatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = preUpdatebleComponents[i];
                try
                {
                    component.OnPreUpdate();
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                }
            }

            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            IntPreUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var updatebleComponents = UpdatebleComponents.Span;
            var len = updatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = updatebleComponents[i];
                try
                {
                    component.OnUpdate();
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                }
            }

            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            IntUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnFixedUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var fixedUpdatebleComponents = FixedUpdatebleComponents.Span;
            var len = fixedUpdatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = fixedUpdatebleComponents[i];
                try
                {
                    component.OnFixedUpdate();
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                }
            }
            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            IntFixedUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnLateUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var lateUpdatebleComponents = LateUpdatebleComponents.Span;
            var len = lateUpdatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = lateUpdatebleComponents[i];
                try
                {
                    component.OnLateUpdate();
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                }
            }

            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            IntLateUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
    }
}
