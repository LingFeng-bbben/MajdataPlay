using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    internal sealed class TouchFeedbackDisplayer : MajComponent
    {
        [SerializeField]
        Animator _anim;
        GameObject[] _children = Array.Empty<GameObject>();
        readonly int _id = Animator.StringToHash("Triggered");
        protected override void Awake()
        {
            base.Awake();
            _children = Transform.GetChildren()
                                 .Select(x => x.gameObject)
                                 .ToArray();
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
                    foreach (var child in ArrayHelper.ToEnumerable(_children))
                    {
                        if (child is null)
                            continue;
                        child.layer = MajEnv.DEFAULT_LAYER;
                    }
                    GameObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    foreach (var child in ArrayHelper.ToEnumerable(_children))
                    {
                        if (child is null)
                            continue;
                        child.layer = MajEnv.HIDDEN_LAYER;
                    }
                    GameObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
    }
}
