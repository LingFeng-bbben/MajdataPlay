using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Unsafe
{
    /// <summary>
    /// Read-only managed references
    /// <para>See also <seealso cref="Ref{T}"></seealso></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe readonly struct ReadOnlyRef<T> : IDisposable
    {
        /// <summary>
        /// Reference to object instance
        /// </summary>
        public ref readonly T Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// <summary>
        /// Releases the handle of the object
        /// <para>After disposal, if you try to get a reference to the object from <see cref="ReadOnlyRef{T}"/> instance, the behavior is undefined</para>
        /// </summary>
        public void Dispose()
        {
            _ref.Dispose();
        }
    }
}
