using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class BreakSlideShineController : MonoBehaviour, IUpdatableComponent<NoteStatus>
    {
        public bool Active { get; private set; }
        public NoteStatus State => ((IStatefulNote?)Parent)?.State ?? NoteStatus.Destroyed;
        public IFlasher? Parent { get; set; } = null;
        public SpriteRenderer[] Renderers { get; set; } = ArrayPool<SpriteRenderer>.Shared.Rent(0);

        NoteStatus _state = NoteStatus.Start;
        GamePlayManager _gpManager;

        public void Initialize()
        {
            if (_state >= NoteStatus.Initialized)
                return;
            else if (Parent is null)
                throw new NullReferenceException();
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            if (Renderers.IsEmpty())
            {
                var parentObject = Parent.GameObject;
                var barCount = parentObject.transform.childCount - 1;
                Renderers = ArrayPool<SpriteRenderer>.Shared.Rent(barCount);
                for (int i = 0; i < barCount; i++)
                    Renderers[i] = parentObject.transform.GetChild(i).GetComponent<SpriteRenderer>();
            }
            _state = NoteStatus.Initialized;
        }
        void Start()
        {
            Initialize();
            Active = true;
        }
        public void ComponentUpdate()
        {
            if (Renderers.IsEmpty())
                return;
            if (Parent is not null && Parent.CanShine)
            {
                try
                {
                    var param = _gpManager.BreakParam;
                    foreach (var renderer in Renderers)
                    {
                        var material = renderer?.material;
                        if (renderer is null)
                            continue;
                        if (material is null)
                            continue;
                        material.SetFloat("_Brightness", param.Brightness);
                        material.SetFloat("_Contrast", param.Contrast);
                    }
                }
                catch
                {

                }
            }
        }
        void OnDestroy()
        {
            ArrayPool<SpriteRenderer>.Shared.Return(Renderers);
            Active = false;
        }
    }

}
