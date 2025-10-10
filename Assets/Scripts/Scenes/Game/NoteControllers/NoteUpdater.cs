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
        public double PreUpdateElapsedMs => _preUpdateElapsedMs;
        public double UpdateElapsedMs => _updateElapsedMs;
        public double FixedUpdateElapsedMs => _fixedUpdateElapsedMs;
        public double LateUpdateElapsedMs => _lateUpdateElapsedMs;

        ReadOnlyMemory<NoteInfo> _preUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
        ReadOnlyMemory<NoteInfo> _updatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
        ReadOnlyMemory<NoteInfo> _fixedUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
        ReadOnlyMemory<NoteInfo> _lateUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;

        NoteInfo[] _rentedArrayForPreUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForFixedUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _rentedArrayForLateUpdatebleComponents = Array.Empty<NoteInfo>();

        [ReadOnlyField]
        [SerializeField]
        double _preUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _updateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _fixedUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _lateUpdateElapsedMs = 0;

        readonly static List<MonoBehaviour> SHARED_CACHE_LIST = new(64);
        public async UniTask InitAsync()
        {
            await UniTask.SwitchToMainThread();
            var children = transform.GetChildren();

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
                }
                else
                {
                    noteInfo.Dispose();
                }
            }
            
            _rentedArrayForPreUpdatebleComponents = Pool<NoteInfo>.RentArray(preUpdatableComponents.Count, true);
            _rentedArrayForUpdatebleComponents = Pool<NoteInfo>.RentArray(updatableComponents.Count, true);
            _rentedArrayForFixedUpdatebleComponents = Pool<NoteInfo>.RentArray(fixedUpdatableComponents.Count, true);
            _rentedArrayForLateUpdatebleComponents = Pool<NoteInfo>.RentArray(lateUpdatableComponents.Count, true);

            preUpdatableComponents.CopyTo(_rentedArrayForPreUpdatebleComponents);
            updatableComponents.CopyTo(_rentedArrayForUpdatebleComponents);
            fixedUpdatableComponents.CopyTo(_rentedArrayForFixedUpdatebleComponents);
            lateUpdatableComponents.CopyTo(_rentedArrayForLateUpdatebleComponents);

            _preUpdatebleComponents = _rentedArrayForPreUpdatebleComponents.AsMemory(0, preUpdatableComponents.Count);
            _updatebleComponents = _rentedArrayForUpdatebleComponents.AsMemory(0, updatableComponents.Count);
            _fixedUpdatebleComponents = _rentedArrayForFixedUpdatebleComponents.AsMemory(0, fixedUpdatableComponents.Count);
            _lateUpdatebleComponents = _rentedArrayForLateUpdatebleComponents.AsMemory(0, lateUpdatableComponents.Count);
        }

        protected virtual void OnDestroy()
        {
            Clear();
        }

        internal virtual void Clear()
        {
            foreach (var component in _preUpdatebleComponents.Span)
            {
                component.Dispose();
            }
            foreach (var component in _updatebleComponents.Span)
            {
                component.Dispose();
            }
            foreach (var component in _fixedUpdatebleComponents.Span)
            {
                component.Dispose();
            }
            foreach (var component in _lateUpdatebleComponents.Span)
            {
                component.Dispose();
            }
            _preUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
            _updatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
            _fixedUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;
            _lateUpdatebleComponents = ReadOnlyMemory<NoteInfo>.Empty;

            Pool<NoteInfo>.ReturnArray(_rentedArrayForPreUpdatebleComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForUpdatebleComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForFixedUpdatebleComponents, true);
            Pool<NoteInfo>.ReturnArray(_rentedArrayForLateUpdatebleComponents, true);

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
            var preUpdatebleComponents = _preUpdatebleComponents.Span;
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
            _preUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var updatebleComponents = _updatebleComponents.Span;
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
            _updateElapsedMs = timeSpan.TotalMilliseconds;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnFixedUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var fixedUpdatebleComponents = _fixedUpdatebleComponents.Span;
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
            _fixedUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void OnLateUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var lateUpdatebleComponents = _lateUpdatebleComponents.Span;
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
            _lateUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
    }
}
