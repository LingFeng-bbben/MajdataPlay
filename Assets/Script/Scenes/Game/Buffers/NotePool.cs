using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    public class NotePool<TInfo, TMember> : INotePool<TInfo, TMember>
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public int Capacity { get; set; } = 64;
        public bool IsStatic { get; } = true;

        protected TimingPoint<TInfo>?[] _timingPoints;
        protected List<IPoolableNote<TInfo, TMember>> _idleNotes;
        protected List<IPoolableNote<TInfo, TMember>> _inUseNotes;

        public NotePool(GameObject prefab, Transform parent, TInfo[] noteInfos, int capacity)
        {
            Capacity = capacity;
            _idleNotes = new(capacity);
            _inUseNotes = new(capacity);
            for (var i = 0; i < capacity; i++)
            {
                var obj = UnityEngine.Object.Instantiate(prefab, parent);
                obj.SetActive(false);
                var noteObj = obj.GetComponent<IPoolableNote<TInfo, TMember>>();
                if (noteObj is null)
                    throw new NotSupportedException();
                _idleNotes.Add(noteObj);
            }
            var timingPoints = noteInfos.GroupBy(x => x.AppearTiming)
                                        .OrderBy(x => x.Key);
            _timingPoints = new TimingPoint<TInfo>[timingPoints.Count()];
            foreach (var (i, timingPoint) in timingPoints.WithIndex())
            {
                _timingPoints[i] = new TimingPoint<TInfo>()
                {
                    Timing = timingPoint.Key,
                    Infos = timingPoint.ToArray()
                };
            }
        }
        public NotePool(GameObject prefab, Transform parent, TInfo[] noteInfos) : this(prefab, parent, noteInfos, 64)
        {

        }
        public virtual void Update(float currentSec)
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
                        return;
                    _timingPoints[i] = null;
                }
            }
        }
        bool Dequeue(TInfo?[] infos)
        {
            foreach (var (i, info) in infos.AsSpan().WithIndex())
            {
                if (info is null)
                    continue;
                var idleNote = Dequeue();
                if (idleNote is null)
                    return false;
                ActiveObject(idleNote, info);
                infos[i] = null;
            }
            return true;
        }
        public IPoolableNote<TInfo, TMember>? Dequeue()
        {

            if (_idleNotes.IsEmpty())
            {
                Debug.LogWarning($"No more Note can use");
                return null;
            }
            var idleNote = _idleNotes[0];
            _idleNotes.RemoveAt(0);
            return idleNote;
        }
        void ActiveObject(IPoolableNote<TInfo, TMember> element, TInfo info)
        {
            var obj = element.GameObject;
            info.Instance = obj;
            _inUseNotes.Add(element);
            element.Initialize(info);
            if (!obj.activeSelf)
                obj.SetActive(true);
        }
        public virtual void Collect(in IPoolableNote<TInfo, TMember> endNote)
        {
            _inUseNotes.Remove(endNote);
            _idleNotes.Add(endNote);
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
                    Debug.LogWarning($"Cannot destroy note:\n{e}");
                }
            }
            foreach (var note in _inUseNotes)
            {
                try
                {
                    note.End(true);
                    UnityEngine.Object.Destroy(note.GameObject);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Cannot destroy note:\n{e}");
                }
            }
        }
        protected class TimingPoint<T> where T : NotePoolingInfo
        {
            public float Timing { get; init; }
            public T?[] Infos { get; init; } = Array.Empty<T>();
        }
    }
}
