using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.References
{
    public unsafe readonly ref struct ReadOnlyValueRef<T> where T : unmanaged
    {
        public ref readonly T Target
        {
            get => ref _ref.Target;
        }
        readonly ValueRef<T> _ref;

        public ReadOnlyValueRef(ref T obj)
        {
            _ref = new ValueRef<T>(ref obj);
        }
        public ReadOnlyValueRef(T* pointer)
        {
            _ref = new ValueRef<T>(pointer);
        }
        public ReadOnlyValueRef(ValueRef<T> valueRef)
        {
            _ref = valueRef;
        }
    }
}
