using System;

namespace MajdataPlay.Buffers
{
    public class ElementNotMatchException<TElement> : ArgumentException
    {
        public TElement Element { get; init; }

        public ElementNotMatchException(TElement element) : this(element,"The element do not match")
        {
        }

        public ElementNotMatchException(TElement element, string message) : base(message)
        {
            Element = element;
        }
    }
}