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
    internal class NoteComponentUpdater : MonoBehaviour
    {
        public double PreUpdateElapsedMs => IntPreUpdateElapsedMs;
        public double UpdateElapsedMs => IntUpdateElapsedMs;
        public double FixedUpdateElapsedMs => IntFixedUpdateElapsedMs;
        public double LateUpdateElapsedMs => IntLateUpdateElapsedMs;

        protected ReadOnlyMemory<NoteComponentInfo> Components = ReadOnlyMemory<NoteComponentInfo>.Empty;
        protected ReadOnlyMemory<NoteComponentInfo> PreUpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;
        protected ReadOnlyMemory<NoteComponentInfo> UpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;
        protected ReadOnlyMemory<NoteComponentInfo> FixedUpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;
        protected ReadOnlyMemory<NoteComponentInfo> LateUpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;

        NoteComponentInfo[] _rentedArrayForComponents = Array.Empty<NoteComponentInfo>();
        NoteComponentInfo[] _rentedArrayForPreUpdatebleComponents = Array.Empty<NoteComponentInfo>();
        NoteComponentInfo[] _rentedArrayForUpdatebleComponents = Array.Empty<NoteComponentInfo>();
        NoteComponentInfo[] _rentedArrayForFixedUpdatebleComponents = Array.Empty<NoteComponentInfo>();
        NoteComponentInfo[] _rentedArrayForLateUpdatebleComponents = Array.Empty<NoteComponentInfo>();

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

            using RentedList<NoteComponentInfo> noteComponents = new();
            using RentedList<NoteComponentInfo> preUpdatableComponents = new();
            using RentedList<NoteComponentInfo> updatableComponents = new();
            using RentedList<NoteComponentInfo> fixedUpdatableComponents = new();
            using RentedList<NoteComponentInfo> lateUpdatableComponents = new();
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
                var noteInfo = new NoteComponentInfo(component);
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
            
            _rentedArrayForComponents = Pool<NoteComponentInfo>.RentArray(noteComponents.Count, true);
            _rentedArrayForPreUpdatebleComponents = Pool<NoteComponentInfo>.RentArray(preUpdatableComponents.Count, true);
            _rentedArrayForUpdatebleComponents = Pool<NoteComponentInfo>.RentArray(updatableComponents.Count, true);
            _rentedArrayForFixedUpdatebleComponents = Pool<NoteComponentInfo>.RentArray(fixedUpdatableComponents.Count, true);
            _rentedArrayForLateUpdatebleComponents = Pool<NoteComponentInfo>.RentArray(lateUpdatableComponents.Count, true);

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

            Components = ReadOnlyMemory<NoteComponentInfo>.Empty;
            PreUpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;
            UpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;
            FixedUpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;
            LateUpdatebleComponents = ReadOnlyMemory<NoteComponentInfo>.Empty;

            Pool<NoteComponentInfo>.ReturnArray(_rentedArrayForComponents, true);
            Pool<NoteComponentInfo>.ReturnArray(_rentedArrayForPreUpdatebleComponents, true);
            Pool<NoteComponentInfo>.ReturnArray(_rentedArrayForUpdatebleComponents, true);
            Pool<NoteComponentInfo>.ReturnArray(_rentedArrayForFixedUpdatebleComponents, true);
            Pool<NoteComponentInfo>.ReturnArray(_rentedArrayForLateUpdatebleComponents, true);

            _rentedArrayForComponents = Array.Empty<NoteComponentInfo>();
            _rentedArrayForPreUpdatebleComponents = Array.Empty<NoteComponentInfo>();
            _rentedArrayForUpdatebleComponents = Array.Empty<NoteComponentInfo>();
            _rentedArrayForFixedUpdatebleComponents = Array.Empty<NoteComponentInfo>();
            _rentedArrayForLateUpdatebleComponents = Array.Empty<NoteComponentInfo>();
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
