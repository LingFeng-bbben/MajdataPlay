using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace PDNWrapper
{
    internal class Surface
    {
        NativeArray<Color32> m_Color;
        public Surface(int w, int h)
        {
            width = w;
            height = h;
            m_Color = new NativeArray<Color32>(width * height, Allocator.Persistent);
        }

        public void Dispose()
        {
            AtomicSafetyHandle handle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(m_Color);
            if (m_Color.IsCreated && AtomicSafetyHandle.IsHandleValid(handle))
            {
                m_Color.Dispose();
                m_Color = default;
            }
        }

        public NativeArray<Color32> color
        {
            get { return m_Color; }
        }

        public int width { get; private set; }
        public int height { get; private set; }
    }
}
