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
    public sealed class CubismAfterTransparentsRenderFeature : ScriptableRendererFeature
    {
        CubismRenderPassFeature.CubismRenderPass _renderPass;

        public override void Create()
        {
            _renderPass = new CubismRenderPassFeature.CubismRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (renderingData.cameraData.cameraType != CameraType.Game
                && renderingData.cameraData.cameraType != CameraType.SceneView)
            {
                return;
            }
#endif

            renderer.EnqueuePass(_renderPass);
        }
    }
}
