using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.References
{
    public unsafe readonly struct ReadOnlyRef<T>: IDisposable
    {
        public ref readonly T Target
        {
            get => ref _ref.Target;
        }
        readonly Ref<T> _ref;
        public ReadOnlyRef(Ref<T> @ref)
        {
            _ref = @ref;
        }
        public ReadOnlyRef(ref T obj)
        {
            _ref = new(ref obj);
        }
        public void Dispose()
        {
            _ref.Dispose();
        }
    }
}
