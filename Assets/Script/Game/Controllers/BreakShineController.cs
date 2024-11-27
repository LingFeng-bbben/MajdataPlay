using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class BreakShineController : MonoBehaviour, IUpdatableComponent<NoteStatus>
    {
        public bool Active { get; set; }
        public NoteStatus State => ((IStatefulNote?)Parent)?.State ?? NoteStatus.Destroyed;
        public IFlasher? Parent { get; set; } = null;
        public SpriteRenderer? Renderer { get; set; } = null;

        static readonly int _id1 = Shader.PropertyToID("_Brightness");
        static readonly int _id2 = Shader.PropertyToID("_Contrast");

        GamePlayManager _gpManager;
        Material _material;

        void Start()
        {
            if(Renderer is null)
                Renderer = GetComponent<SpriteRenderer>();
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            _material = Renderer.material;
            //Active = true;
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
                if (_material is null)
                    return;
                _material.SetFloat(_id1, param.Brightness);
                _material.SetFloat(_id2, param.Contrast);
            }
        }
    }
}