using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MajdataPlay.Game
{
    public class NoteUpdater : MonoBehaviour
    {
        NoteInfo[] _updatableComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _fixedUpdatableComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _lateUpdatableComponents = Array.Empty<NoteInfo>();
        Stopwatch _stopwatch = new Stopwatch();
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

        void Update()
        {
            _stopwatch.Restart();
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
            Debug.Log($"NoteUpdate: time consuming {_stopwatch.ElapsedMilliseconds}ms");
        }
        void FixedUpdate()
        {
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
        }
        void LateUpdate()
        {
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
        }
    }
}
