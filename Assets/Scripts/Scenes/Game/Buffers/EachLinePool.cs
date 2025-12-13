using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Scenes.Game.Notes.Behaviours;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    internal sealed class EachLinePool : NotePool<EachLinePoolingInfo, NoteQueueInfo>
    {
        public EachLinePool(GameObject prefab, Transform parent, EachLinePoolingInfo[] noteInfos, int capacity) : base(prefab, parent, noteInfos, capacity)
        {

        }
        public EachLinePool(GameObject prefab, Transform parent, EachLinePoolingInfo[] noteInfos) : base(prefab, parent, noteInfos)
        {

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnPreUpdate(float currentSec)
        {
            ThrowIfDisposed();
            if (_timingPoints.IsEmpty)
            {
                return;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Dequeue(ref TimingPoint<EachLinePoolingInfo> tp)
        {
            var infos = tp.Infos;
            if (infos.IsEmpty)
            {
                return true;
            }
            var _infos = infos.Span;
            var i = 0;
            try
            {
                for (; i < _infos.Length; i++)
                {
                    var info = _infos[i];
                    var eachLine = Dequeue();
                    if (eachLine is null)
                    {
                        return false;
                    }
                    else if (!ActiveObject(info, eachLine))
                    {
                        return false;
                    }
                }
            }
            finally
            {
                tp.Infos = infos.Slice(i);
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        new EachLineDrop? Dequeue()
        {
            EachLineDrop? idleEachLine;
            if(!_storage.TryRent(out var poolableNote))
            {
                switch (_flag)
                {
                    case 0:
                        MajDebug.LogWarning($"No more EachLine can use");
                        _flag = 1;
                        break;
                }
                return null;
            }
            _flag = 0;
            idleEachLine = poolableNote as EachLineDrop;

            return idleEachLine;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ActiveObject(EachLinePoolingInfo info, EachLineDrop eachLine)
        {
            var noteA = info.MemberA?.Instance;
            var noteB = info.MemberB?.Instance;
            IDistanceProvider? distanceProvider;

            if (noteA is null && noteB is null)
            {
                return false;
            }
            if (noteA is TapDrop a && noteB is HoldDrop)
            {
                distanceProvider = a;
            }
            else if (noteA is HoldDrop && noteB is TapDrop aa)
            {
                distanceProvider = aa;
            }
            else
            {
                distanceProvider = (noteA as IDistanceProvider ?? noteB as IDistanceProvider);
            }

            eachLine.DistanceProvider = distanceProvider;
            eachLine.NoteA = noteA;
            eachLine.NoteB = noteB;
            eachLine.Initialize(info);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Collect(in IPoolableNote<EachLinePoolingInfo, NoteQueueInfo> endNote)
        {
            ThrowIfDisposed();
            if(endNote is EachLineDrop eachLine)
            {
                _storage.Return(eachLine);
            }
            else
            {
                throw new ArgumentException(nameof(eachLine));
            }
        }
    }
}
