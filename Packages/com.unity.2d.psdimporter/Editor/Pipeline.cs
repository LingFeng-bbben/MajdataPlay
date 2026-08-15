using Unity.Collections;
using UnityEditor.U2D.Common;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    //[CreateAssetMenu(fileName = "Pipeline.asset", menuName = "2D/PSDImporter Pipeline")]
    class Pipeline : ScriptableObject
    {
        void PackImage(NativeArray<Color32>[] buffers, int[] width, int[] height, int padding, uint spriteSizeExpand, out NativeArray<Color32> outPackedBuffer, out int outPackedBufferWidth, out int outPackedBufferHeight, out RectInt[] outPackedRect, out Vector2Int[] outUVTransform, bool requireSquarePOT = false)
        {
            ImagePacker.Pack(buffers, width, height, padding, spriteSizeExpand, out outPackedBuffer, out outPackedBufferWidth, out outPackedBufferHeight, out outPackedRect, out outUVTransform, requireSquarePOT);
        }
    }
}
