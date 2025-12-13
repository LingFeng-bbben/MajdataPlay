using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Numerics
{
    public readonly struct Range<T> where T : IComparable<T>
    {
        public T Start => _left;
        public T End => _right;
        public ContainsType Type { get; init; }

        readonly T _left;
        readonly T _right;

        public Range(T left, T right, ContainsType type)
        {
            if (left.CompareTo(right) > 0)
                throw new ArgumentException("The lower bound of the interval must be greater than or equal to the upper bound");
            _left = left;
            _right = right;
            Type = type;
        }
        public Range(T left, T right) : this(left, right, ContainsType.Closed) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InRange(T value)
        {
            var inRange = value.InRange(_left, _right);
            switch (Type)
            {
                case ContainsType.Closed:
                    return inRange;
                case ContainsType.LeftOpen:
                    if (inRange)
                        return _left.CompareTo(value) < 0;
                    return false;
                case ContainsType.RightOpen:
                    if (inRange)
                        return _right.CompareTo(value) > 0;
                    return false;
                case ContainsType.Open:
                    return _left.CompareTo(value) < 0 && _right.CompareTo(value) > 0;
                default:
                    return false;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Range<int> FromRange(Range range)
        {
            return new Range<int>(range.Start.Value, range.End.Value, ContainsType.RightOpen);
        }
    }
}
