using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    internal sealed class EachLinePool : NotePool<EachLinePoolingInfo, NoteQueueInfo>
    {
        new Queue<EachLineDrop> _storage;
        Queue<EachLineDrop> _idleEachLines;
        public EachLinePool(GameObject prefab, Transform parent, EachLinePoolingInfo[] noteInfos, int capacity) : base(prefab, parent, noteInfos, capacity)
        {
            _storage = new Queue<EachLineDrop>(capacity);
            _idleEachLines = new Queue<EachLineDrop>(capacity);

            foreach(var obj in base._storage)
            {
                _storage.Enqueue(obj.GameObject.GetComponent<EachLineDrop>());
            }
            foreach (var obj in base._idleNotes)
            {
                _idleEachLines.Enqueue(obj.GameObject.GetComponent<EachLineDrop>());
            }
        }
        public EachLinePool(GameObject prefab, Transform parent, EachLinePoolingInfo[] noteInfos) : base(prefab, parent, noteInfos)
        {
            _storage = new Queue<EachLineDrop>(64);
            _idleEachLines = new Queue<EachLineDrop>(64);

            foreach (var obj in base._storage)
            {
                _storage.Enqueue(obj.GameObject.GetComponent<EachLineDrop>());
            }
        }
        public override void OnPreUpdate(float currentSec)
        {
            if (_timingPoints.IsEmpty)
                return;
            var timingPoints = _timingPoints.Span;
            var i = 0;
            try
            {
                for (; i < timingPoints.Length; i++)
                {
                    ref var tp = ref timingPoints[i];
                    var timeDiff = currentSec - tp.Timing;
                    if (timeDiff > -0.15f)
                    {
                        if (!Dequeue(ref tp))
                            return;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                if (i != 0)
                {
                    _timingPoints = _timingPoints.Slice(i);
                }
            }
        }
        bool Dequeue(ref TimingPoint<EachLinePoolingInfo> tp)
        {
            var infos = tp.Infos;
            if (infos.IsEmpty)
                return true;
            var _infos = infos.Span;
            var i = 0;
            try
            {
                for (; i < _infos.Length; i++)
                {
                    var info = _infos[i];
                    var eachLine = Dequeue();
                    if (eachLine is null)
                        return false;
                    else if (!ActiveObject(info, eachLine))
                        return false;
                }
            }
            finally
            {
                tp.Infos = infos.Slice(i);
            }
            return true;
        }
        new EachLineDrop? Dequeue()
        {
            EachLineDrop idleEachLine;
            if (_idleEachLines.Count == 0)
            {
                if (_storage.Count != 0)
                {
                    idleEachLine = _storage.Dequeue();
                    idleEachLine.GameObject.SetActive(true);
                }
                else
                {
                    MajDebug.LogWarning($"No more EachLine can use");
                    return null;
                }
            }
            else
            {
                idleEachLine = _idleEachLines.Dequeue();
            }

            return idleEachLine;
        }
        bool ActiveObject(EachLinePoolingInfo info, EachLineDrop eachLine)
        {
            var noteA = info.MemberA?.Instance;
            var noteB = info.MemberB?.Instance;

            if (noteA is null && noteB is null)
                return false;

            eachLine.DistanceProvider = (noteA as IDistanceProvider ?? noteB as IDistanceProvider);
            eachLine.NoteA = noteA;
            eachLine.NoteB = noteB;
            eachLine.Initialize(info);

            return true;
        }
        public override void Collect(in IPoolableNote<EachLinePoolingInfo, NoteQueueInfo> endNote)
        {
            if(endNote is EachLineDrop eachLine)
            {
                _idleEachLines.Enqueue(eachLine);
            }
            else
            {
                throw new ArgumentException(nameof(eachLine));
            }
        }
    }
}
