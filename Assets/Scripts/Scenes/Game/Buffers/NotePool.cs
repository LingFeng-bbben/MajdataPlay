using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    internal class NotePool<TInfo, TMember> : INotePool<TInfo, TMember>
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public int Capacity { get; set; } = 64;
        public bool IsStatic { get; } = true;

        protected Memory<TimingPoint<TInfo>> _timingPoints = Memory<TimingPoint<TInfo>>.Empty;
        protected Queue<IPoolableNote<TInfo, TMember>> _storage;
        protected Queue<IPoolableNote<TInfo, TMember>> _idleNotes;

        public NotePool(GameObject prefab, Transform parent, TInfo[] noteInfos, int capacity)
        {
            Capacity = capacity;
            _storage = new(capacity);
            _idleNotes = new(capacity);
            for (var i = 0; i < capacity; i++)
            {
                var obj = UnityEngine.Object.Instantiate(prefab, parent);
                obj.SetActive(false);
                var noteObj = obj.GetComponent<IPoolableNote<TInfo, TMember>>();
                if (noteObj is null)
                    throw new NotSupportedException();
                _storage.Enqueue(noteObj);
            }
            var orderedTimingPoints = noteInfos.GroupBy(x => x.AppearTiming)
                                        .OrderBy(x => x.Key);
            _timingPoints = new TimingPoint<TInfo>[orderedTimingPoints.Count()];
            var timingPoints = _timingPoints.Span;
            foreach (var (i, timingPoint) in orderedTimingPoints.WithIndex())
            {
                timingPoints[i] = new TimingPoint<TInfo>()
                {
                    Timing = timingPoint.Key,
                    Infos = timingPoint.ToArray()
                };
            }
        }
        public NotePool(GameObject prefab, Transform parent, TInfo[] noteInfos) : this(prefab, parent, noteInfos, 64)
        {

        }
        public virtual void OnUpdate(float currentSec)
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
        bool Dequeue(ref TimingPoint<TInfo> tp)
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
                    var idleNote = Dequeue();
                    if (idleNote is null)
                        return false;
                    ActiveObject(idleNote, info);
                }
            }
            finally
            {
                tp.Infos = infos.Slice(i);
            }
            return true;
        }
        public IPoolableNote<TInfo, TMember>? Dequeue()
        {
            IPoolableNote<TInfo, TMember> idleNote;
            if (_idleNotes.Count == 0)
            {
                if(_storage.Count != 0)
                {
                    idleNote = _storage.Dequeue();
                    idleNote.GameObject.SetActive(true);
                }
                else
                {
                    MajDebug.LogWarning($"No more Note can use");
                    return null;
                }
            }
            else
            {
                idleNote = _idleNotes.Dequeue();
            }
            
            return idleNote;
        }
        void ActiveObject(IPoolableNote<TInfo, TMember> element, TInfo info)
        {
            info.Instance = element as NoteDrop;
            element.Initialize(info);
        }
        public virtual void Collect(in IPoolableNote<TInfo, TMember> endNote)
        {
            _idleNotes.Enqueue(endNote);
        }
        public virtual void Destroy()
        {
            foreach (var note in _idleNotes)
            {
                try
                {
                    note.End(true);
                    UnityEngine.Object.Destroy(note.GameObject);
                }
                catch (Exception e)
                {
                    MajDebug.LogWarning($"Cannot destroy note:\n{e}");
                }
            }
        }
        protected struct TimingPoint<T> where T : NotePoolingInfo
        {
            public float Timing { get; init; }
            public Memory<T> Infos { get; set; }
        }
    }
}
