using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Game.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    public sealed class EachLinePool : NotePool<EachLinePoolingInfo, NoteQueueInfo>
    {
        public EachLinePool(GameObject prefab, Transform parent, EachLinePoolingInfo[] noteInfos, int capacity) : base(prefab, parent, noteInfos, capacity)
        {

        }
        public EachLinePool(GameObject prefab, Transform parent, EachLinePoolingInfo[] noteInfos) : base(prefab, parent, noteInfos)
        {

        }
        public override void Update(float currentSec)
        {
            if (_idleNotes.IsEmpty())
                return;
            foreach (var (i, tp) in _timingPoints.AsSpan().WithIndex())
            {
                if (tp is null)
                    continue;
                var timeDiff = currentSec - tp.Timing;
                if (timeDiff > -0.15f)
                {
                    if (!Dequeue(tp.Infos))
                        continue;
                    _timingPoints[i] = null;
                }
            }
        }
        bool Dequeue(EachLinePoolingInfo?[] infos)
        {
            foreach (var (i, info) in infos.AsSpan().WithIndex())
            {
                if (info is null)
                    continue;
                else if (!Dequeue(info))
                    return false;
                infos[i] = null;
            }
            return true;
        }
        bool Dequeue(EachLinePoolingInfo info)
        {
            var noteA = info.MemberA?.Instance;
            var noteB = info.MemberB?.Instance;
            if (_idleNotes.IsEmpty())
            {
                MajDebug.LogWarning($"No more EachLine can use");
                return false;
            }
            else if (noteA is null && noteB is null)
                return false;
            var idleNote = _idleNotes[0];
            var obj = idleNote.GameObject;
            info.Instance = obj;
            var eachLine = obj.GetComponent<EachLineDrop>();
            eachLine.DistanceProvider = (noteA ?? noteB)?.GetComponent<IDistanceProvider>();
            eachLine.NoteA = noteA?.GetComponent<IStatefulNote>();
            eachLine.NoteB = noteB?.GetComponent<IStatefulNote>();
            _inUseNotes.Add(idleNote);
            _idleNotes.RemoveAt(0);
            idleNote.Initialize(info);
            if (!obj.activeSelf)
                obj.SetActive(true);
            return true;
        }
    }
}
