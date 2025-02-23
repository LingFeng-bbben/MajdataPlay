using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    internal sealed class TouchFeedbackDisplayer : MajComponent
    {
        [SerializeField]
        Animator _anim;
        readonly int _id = Animator.StringToHash("Triggered");
        protected override void Awake()
        {
            base.Awake();
            SetActiveInternal(false);
        }
        public void Reset()
        {
            SetActive(false);
        }
        public void Play()
        {
            SetActive(true);
            _anim.SetTrigger(_id);
        }
        public override void SetActive(bool state)
        {
            if (Active == state)
                return;
            SetActiveInternal(state);
        }
        void SetActiveInternal(bool state)
        {
            Active = state;
            base.SetActive(state);
            switch (state)
            {
                case true:
                    GameObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    GameObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
    }
}
