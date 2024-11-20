#nullable  enable
using System;

namespace MajdataPlay.Buffers
{
    public abstract class ObjectPool<TElement,TElementContainer>: IObjectPool<TElement> where TElement : IEquatable<TElement>
    {
        public int Capacity { get; set; }
        public abstract bool IsStatic { get; }
        
        protected int _capacity;
        
        protected TElementContainer _idleElements;
        protected TElementContainer _inUseElements;

        public abstract TElement? Dequeue();
        public abstract void Collect(in TElement element);
    }
}