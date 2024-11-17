using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game
{
    public class NoteUpdater : MonoBehaviour
    {
        NoteInfo[] _updatableComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _fixedUpdatableComponents = Array.Empty<NoteInfo>();
        NoteInfo[] _lateUpdatableComponents = Array.Empty<NoteInfo>();
        public void Initialize()
        {
            Transform[] childs = new Transform[transform.childCount];
            List<NoteInfo> updatableComponents = new();
            List<NoteInfo> fixedUpdatableComponents = new();
            List<NoteInfo> lateUpdatableComponents = new();
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
            foreach (var component in _updatableComponents)
            {
                try
                {
                    component.Update();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        void FixedUpdate()
        {
            foreach (var component in _fixedUpdatableComponents)
            {
                try
                {
                    component.Update();
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
                    component.LateUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
