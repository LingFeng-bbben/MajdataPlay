using MajdataPlay.Interfaces;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using MajdataPlay.Editor;
using UnityEngine;
using MajdataPlay.Game.Buffers;

namespace MajdataPlay.Game.Notes.Controllers
{
    internal class NoteUpdater : MonoBehaviour
    {
        public double PreUpdateElapsedMs => _preUpdateElapsedMs;
        public double UpdateElapsedMs => _updateElapsedMs;
        public double FixedUpdateElapsedMs => _fixedUpdateElapsedMs;
        public double LateUpdateElapsedMs => _lateUpdateElapsedMs;

        NoteInfo[] _preUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _updatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _fixedUpdatebleComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _lateUpdatebleComponents = Array.Empty<NoteInfo>();

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
        public void Initialize()
        {
            var children = transform.GetChildren();

            List<NoteInfo> preUpdatableComponents = new();
            List<NoteInfo> updatableComponents = new();
            List<NoteInfo> fixedUpdatableComponents = new();
            List<NoteInfo> lateUpdatableComponents = new();
            //for (int i = 0; i < children.Length; i++)
            //    children[i] = transform.GetChild(i);
            foreach (var child in children)
            {
                var childComponents = child.GetComponents<IStateful<NoteStatus>>();
                if (childComponents.Length != 0)
                {
                    foreach (var component in childComponents)
                    {
                        var noteInfo = new NoteInfo(component);
                        if (noteInfo.IsValid)
                        {
                            if (noteInfo.IsUpdatable)
                                updatableComponents.Add(noteInfo);
                            if (noteInfo.IsFixedUpdatable)
                                fixedUpdatableComponents.Add(noteInfo);
                            if (noteInfo.IsLateUpdatable)
                                lateUpdatableComponents.Add(noteInfo);
                            if (noteInfo.IsPreUpdatable)
                                preUpdatableComponents.Add(noteInfo);
                        }
                    }
                }
            }
            _preUpdatebleComponents = preUpdatableComponents.ToArray();
            _updatebleComponents = updatableComponents.ToArray();
            _fixedUpdatebleComponents = fixedUpdatableComponents.ToArray();
            _lateUpdatebleComponents = lateUpdatableComponents.ToArray();
        }

        internal virtual void Clear()
        {
            _preUpdatebleComponents = Array.Empty<NoteInfo>();
            _updatebleComponents = Array.Empty<NoteInfo>();
            _fixedUpdatebleComponents = Array.Empty<NoteInfo>();
            _lateUpdatebleComponents = Array.Empty<NoteInfo>();
        }
        internal virtual void OnPreUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var len = _preUpdatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = _preUpdatebleComponents[i];
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
        internal virtual void OnUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var len = _updatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = _updatebleComponents[i];
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
        internal virtual void OnFixedUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var len = _fixedUpdatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = _fixedUpdatebleComponents[i];
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
        internal virtual void OnLateUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            var len = _lateUpdatebleComponents.Length;
            for (var i = 0; i < len; i++)
            {
                var component = _lateUpdatebleComponents[i];
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
