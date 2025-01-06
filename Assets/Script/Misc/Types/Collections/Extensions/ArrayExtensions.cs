#nullable enable
namespace MajdataPlay.Collections
{
    public static class ArrayExtensions
    {
        //public static bool IsEmpty(this Array source) => source.Length == 0;
        public static Heap<T> AsHeap<T>(this T[] source) where T : unmanaged
        {
            return new Heap<T>(source);
        }
    }
}