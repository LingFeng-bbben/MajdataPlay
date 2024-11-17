using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Types.Attribute;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MajdataPlay.Game
{
    public class NoteUpdater : MonoBehaviour
    {
        public double UpdateElapsedMs => _updateElapsedMs;
        public double FixedUpdateElapsedMs => _fixedUpdateElapsedMs;
        public double LateUpdateElapsedMs => _lateUpdateElapsedMs;

        NoteInfo[] _updatableComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _fixedUpdatableComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _lateUpdatableComponents = Array.Empty<NoteInfo>();

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
            Transform[] childs = new Transform[transform.childCount];
            List<NoteInfo> updatableComponents = new();
            List<NoteInfo> fixedUpdatableComponents = new();
            List<NoteInfo> lateUpdatableComponents = new();
            for (int i = 0; i < childs.Length; i++)
                childs[i] = transform.GetChild(i);
            foreach(var child in childs)
            {
                var childComponents = child.GetComponents<IStateful<NoteStatus>>();
                if(childComponents.Length != 0)
                {
                    foreach(var component in childComponents)
                    {
                        var noteInfo = new NoteInfo(component);
                        if(noteInfo.IsValid)
                        {
                            if(noteInfo.IsUpdatable)
                                updatableComponents.Add(noteInfo);
                            if(noteInfo.IsFixedUpdatable)
                                fixedUpdatableComponents.Add(noteInfo);
                            if (noteInfo.IsLateUpdatable)
                                lateUpdatableComponents.Add(noteInfo);
                        }
                    }
                }
            }
            _updatableComponents = updatableComponents.ToArray();
            _fixedUpdatableComponents = fixedUpdatableComponents.ToArray();
            _lateUpdatableComponents = lateUpdatableComponents.ToArray();
        }

        protected virtual void Update()
        {
            var start = MajTimeline.UnscaledTime;
            foreach (var component in _updatableComponents)
            {
                try
                {
                    if (component.CanExecute())
                        component.Update();
                    else
                        continue;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            _updateElapsedMs = timeSpan.TotalMilliseconds;
        }
        protected virtual void FixedUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            foreach (var component in _fixedUpdatableComponents)
            {
                try
                {
                    if (component.CanExecute())
                        component.FixedUpdate();
                    else
                        continue;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            _fixedUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
        protected virtual void LateUpdate()
        {
            var start = MajTimeline.UnscaledTime;
            foreach (var component in _lateUpdatableComponents)
            {
                try
                {
                    if (component.CanExecute())
                        component.LateUpdate();
                    else
                        continue;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            var end = MajTimeline.UnscaledTime;
            var timeSpan = end - start;
            _lateUpdateElapsedMs = timeSpan.TotalMilliseconds;
        }
    }
}
