using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    internal class RecorderStatusDisplayer : MajComponent
    {
        [SerializeField]
        Sprite _notReady;
        [SerializeField]
        Sprite _ready;
        [SerializeField]
        Sprite _recording;

        SpriteRenderer _renderer;

        protected override void Awake()
        {
            base.Awake();
            Majdata<RecorderStatusDisplayer>.Instance = this;
            _renderer = GetComponent<SpriteRenderer>();
            if(!RecordHelper.IsEnabled)
            {
                SetActive(false);
            }
        }
        internal void OnLateUpdate()
        {
            if (!RecordHelper.IsEnabled)
            {
                return;
            }
            if (RecordHelper.IsRecording)
            {
                _renderer.sprite = _recording;
            }
            else if(RecordHelper.IsConnected)
            {
                _renderer.sprite = _ready;
            }
            else
            {
                _renderer.sprite = _notReady;
            }
        }
        void OnDestroy()
        {
            Majdata<RecorderStatusDisplayer>.Free();
        }
    }
}
