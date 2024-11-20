using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
#nullable enable
namespace MajdataPlay.Buffers
{
    public abstract class StaticObjectPool<TElement> : ObjectPool<TElement, TElement?[]> where TElement : IEquatable<TElement>
    {
        protected virtual void Enqueue(in TElement element)
        {
            for (int i = 0; i < _idleElements.Length; i++)
            {
                ref var idleElement = ref _idleElements[i];
                if (idleElement is null)
                    idleElement = element;
            }
        }
        public override void Collect(in TElement element)
        {
            if (element is null)
                return;

            var isMatch = false;

            for (int i = 0; i < _inUseElements.Length; i++)
            {
                ref var inUseElement = ref _inUseElements[i];

                if (inUseElement is null)
                    continue;
                else if (inUseElement.Equals(element))
                {
                    isMatch = true;
                    inUseElement = default;
                    Enqueue(element);
                    break;
                }
            }
            if (!isMatch)
                throw new ElementNotMatchException<TElement>(element);
        }
        public override TElement? Dequeue()
        {
            for (int i = 0; i < _idleElements.Length; i++)
            {
                ref var idleElement = ref _inUseElements[i];
                if (idleElement is null)
                    continue;
                return idleElement;
            }
            return default;
        }
    }
}
