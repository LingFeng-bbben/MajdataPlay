using Live2D.Cubism.Rendering;
using Live2D.Cubism.Rendering.URP;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MajdataPlay.Rendering
{
    /// <summary>
    /// Integrates Cubism with MajdataPlay's world-space UI render order without
    /// modifying the vendor SDK. The stock feature renders before transparents,
    /// which allows List's canvas to cover the model in Play Mode.
    /// </summary>
    public sealed class CubismBeforeTransparentsRenderFeature : CubismRenderFeature
    {
        public override void Create()
        {
            RenderPass = new CubismRenderPassFeature.CubismRenderPass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingTransparents
            };
        }
    }
}
