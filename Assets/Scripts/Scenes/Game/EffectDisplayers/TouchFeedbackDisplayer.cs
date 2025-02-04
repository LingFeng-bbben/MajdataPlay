using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class TouchFeedbackDisplayer: MonoBehaviour
    {
        [SerializeField]
        Animator _anim;
        readonly int _id = Animator.StringToHash("Triggered");
        GameObject _gameObject;
        private void Awake()
        {
            _gameObject = gameObject;
        }
        public void Reset()
        {
            _gameObject.SetActive(false);
        }
        public void Play()
        {
            _gameObject.SetActive(true);
            _anim.SetTrigger(_id);
        }
    }
}
