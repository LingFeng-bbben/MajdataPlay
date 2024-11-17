using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class BreakShineController : MonoBehaviour, IUpdatableComponent<NoteStatus>
    {
        public bool Active { get; private set; }
        public NoteStatus State => ((IStatefulNote?)Parent)?.State ?? NoteStatus.Destroyed;
        public IFlasher? Parent { get; set; } = null;
        public SpriteRenderer? Renderer { get; set; } = null;
        GamePlayManager _gpManager;

        void Start()
        {
            if(Renderer is null)
                Renderer = GetComponent<SpriteRenderer>();
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            Active = true;
        }
        void OnDestroy()
        {
            Active = false;
        }
        public void ComponentUpdate()
        {
            if (Renderer is null)
                return;
            if (Parent is not null && Parent.CanShine)
            {
                var param = _gpManager.BreakParam;
                var material = Renderer.material;
                if (material is null)
                    return;
                material.SetFloat("_Brightness", param.Brightness);
                material.SetFloat("_Contrast", param.Contrast);
            }
        }
    }
}