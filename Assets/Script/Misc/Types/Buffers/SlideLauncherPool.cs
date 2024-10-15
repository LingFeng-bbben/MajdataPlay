using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Buffers
{
    public class SlideLauncherPool : NotePool<TapPoolingInfo, TapQueueInfo>
    {
        public SlideLauncherPool(GameObject prefab, Transform parent, TapPoolingInfo[] noteInfos, int capacity) : base(prefab, parent, noteInfos, capacity)
        {

        }
        public SlideLauncherPool(GameObject prefab, Transform parent, TapPoolingInfo[] noteInfos) : base(prefab, parent, noteInfos)
        {

        }
        public override void Update(float currentSec)
        {
            if (_idleNotes.IsEmpty())
                return;
            foreach (var (i, tp) in _timingPoints.WithIndex())
            {
                if (tp is null)
                    continue;
                var timeDiff = currentSec - tp.Timing;
                if (timeDiff > -0.15f)
                {
                    if (!Dequeue(tp.Infos))
                        return;
                    _timingPoints[i] = null;
                }
            }
        }
        bool Dequeue(TapPoolingInfo?[] infos)
        {
            foreach (var (i, info) in infos.WithIndex())
            {
                if (info is null)
                    continue;
                else if (!Dequeue(info))
                    return false;
                infos[i] = null;
            }
            return true;
        }
        bool Dequeue(TapPoolingInfo info)
        {
            if (_idleNotes.IsEmpty())
            {
                Debug.LogWarning($"No more SlideLauncher can use");
                return false;
            }
            var idleNote = _idleNotes[0];
            var obj = idleNote.GameObject;
            info.Instance = obj;
            var launcher = obj.GetComponent<ISlideLauncher>();
            if (launcher is null)
                throw new NullReferenceException("This type does not implement ISlideLauncher");
            launcher.SlideObject = info.Slide;
            _inUseNotes.Add(idleNote);
            _idleNotes.RemoveAt(0);
            idleNote.Initialize(info);
            if (!obj.activeSelf)
                obj.SetActive(true);
            return true;
        }
    }
}
