using MajdataPlay.Interfaces;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class BreakShineController : MonoBehaviour
    {
        public IFlasher? Parent { get; set; } = null;
        public SpriteRenderer? Renderer { get; set; } = null;
        GamePlayManager gpManager => GamePlayManager.Instance;

        void Start()
        {
            if(Renderer is null)
                Renderer = GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            if (Renderer is null)
                return;
            if (Parent is not null && Parent.CanShine)
            {
                var param = gpManager.BreakParam;
                Renderer.material.SetFloat("_Brightness", param.Brightness);
                Renderer.material.SetFloat("_Contrast", param.Contrast);
            }
        }
    }
}