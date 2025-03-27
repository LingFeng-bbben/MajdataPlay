using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Utils;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Game.Notes.Behaviours
{
    public class TouchBorder : MonoBehaviour
    {
        public SensorArea AreaPosition { get; set; } = SensorArea.C;

        List<NoteRegister> queue = new(64);

        GameObject _two, _three;
        SpriteRenderer _twoRenderer, _threeRenderer;
        Sprite[] _normal, _each, _break;
        void Start()
        {
            var index = AreaPosition.GetIndex();
            var area = AreaPosition.GetGroup() switch
            {
                SensorGroup.A => 'A',
                SensorGroup.B => 'B',
                SensorGroup.D => 'D',
                SensorGroup.E => 'E',
                _ => 'C',
            };
            var pos = NoteHelper.GetTouchAreaPosition(index, area);
            transform.position = pos;
            _two = transform.GetChild(0).gameObject;
            _three = transform.GetChild(1).gameObject;
            _twoRenderer = _two.GetComponent<SpriteRenderer>();
            _threeRenderer = _three.GetComponent<SpriteRenderer>();
            var skin = MajInstances.SkinManager.GetTouchSkin();
            _normal = skin.Border_Normal;
            _each = skin.Border_Each;
            _break = skin.Border_Break;

            _twoRenderer.sprite = _normal[0];
            _threeRenderer.sprite = _normal[1];

            _twoRenderer.forceRenderingOff = true;
            _threeRenderer.forceRenderingOff = true;

            _two.SetActive(true);
            _three.SetActive(true);
        }
        public void Add(bool isBreak, bool isEach)
        {
            var register = new NoteRegister()
            {
                IsBreak = isBreak,
                IsEach = isEach
            };
            queue.Add(register);
            Refresh();
        }
        public void Remove()
        {
            if (!queue.IsEmpty())
                queue.RemoveAt(0);
            Refresh();
        }
        internal void Clear()
        {
            queue.Clear();
            Refresh();
        }
        void Refresh()
        {
            if (queue.IsEmpty() || queue.Count < 2)
            {
                _twoRenderer.forceRenderingOff = true;
                _threeRenderer.forceRenderingOff = true;
                return;
            }

            var first = queue[1];
            NoteRegister? second = null;
            if (queue.Count > 2)
                second = queue[2];

            _twoRenderer.forceRenderingOff = false;
            if (first.IsBreak)
                _twoRenderer.sprite = _break[0];
            else if (first.IsEach)
                _twoRenderer.sprite = _each[0];
            else
                _twoRenderer.sprite = _normal[0];

            if (second is not null)
            {
                var _second = (NoteRegister)second;
                _threeRenderer.forceRenderingOff = false;
                if (_second.IsBreak)
                    _threeRenderer.sprite = _break[1];
                else if (_second.IsEach)
                    _threeRenderer.sprite = _each[1];
                else
                    _threeRenderer.sprite = _normal[1];
            }
            else
            {
                _threeRenderer.forceRenderingOff = true;
            }

        }
        struct NoteRegister
        {
            public bool IsBreak { get; set; }
            public bool IsEach { get; set; }
        }
    }
}