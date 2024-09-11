using MajdataPlay.Interfaces;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class BreakShineController : MonoBehaviour
    {
        public IFlasher? Parent { get; set; } = null;

        SpriteRenderer spriteRenderer;
        GamePlayManager gpManager => GamePlayManager.Instance;
        // Start is called before the first frame update
        void Start()
        {

        }
        // Update is called once per frame
        void Update()
        {
            if (Parent is not null && Parent.CanShine)
            {
                var (brightness, contrast) = gpManager.BreakParams;
                spriteRenderer.material.SetFloat("_Brightness", brightness);
                spriteRenderer.material.SetFloat("_Contrast", contrast);
            }
        }
        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}