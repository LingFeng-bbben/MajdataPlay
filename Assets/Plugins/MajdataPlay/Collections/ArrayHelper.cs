using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Collections
{
    public static class ArrayHelper
    {
        public static ValueEnumerable<T> ToEnumerable<T>(T[] source)
        {
            return new ValueEnumerable<T>(source);
        }
    }
}
