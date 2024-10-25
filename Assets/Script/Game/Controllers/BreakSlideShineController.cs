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
    public class BreakSlideShineController : MonoBehaviour
    {
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
        }
        void Start()
        {
            Initialize();
        }
        void Update()
        {
            if (Renderers.IsEmpty())
                return;
            if (Parent is not null && Parent.CanShine)
            {
                var param = _gpManager.BreakParam;
                foreach (var renderer in Renderers)
                {
                    if (renderer is null)
                        continue;
                    renderer.material.SetFloat("_Brightness", param.Brightness);
                    renderer.material.SetFloat("_Contrast", param.Contrast);
                }
            }
        }
        void OnDestroy()
        {
            ArrayPool<SpriteRenderer>.Shared.Return(Renderers);
        }
    }

}
