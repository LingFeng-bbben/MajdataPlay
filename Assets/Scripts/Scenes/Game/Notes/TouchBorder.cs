using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Game.Notes
{
    public class TouchBorder : MonoBehaviour
    {
        public SensorArea AreaPosition { get; set; } = SensorArea.C;

        List<NoteRegister> queue = new();

        GameObject two, three;
        SpriteRenderer twoRenderer, threeRenderer;
        Sprite[] normal, each,bReak;
        void Start()
        {
            var index = AreaPosition.GetIndex();
            char area = AreaPosition.GetGroup() switch
            {
                SensorGroup.A => 'A',
                SensorGroup.B => 'B',
                SensorGroup.D => 'D',
                SensorGroup.E => 'E',
                _ => 'C',
            };
            var pos = TouchBase.GetAreaPos(index, area);
            transform.position = pos;
            two = transform.GetChild(0).gameObject;
            three = transform.GetChild(1).gameObject;
            twoRenderer = two.GetComponent<SpriteRenderer>();
            threeRenderer = three.GetComponent<SpriteRenderer>();
            var skin = MajInstances.SkinManager.GetTouchSkin();
            normal = skin.Border_Normal;
            each = skin.Border_Each;
            bReak = skin.Border_Break;

            twoRenderer.sprite = normal[0];
            threeRenderer.sprite = normal[1];

            twoRenderer.forceRenderingOff = true;
            threeRenderer.forceRenderingOff = true;
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
        void Refresh()
        {
            if (queue.IsEmpty() || queue.Count < 2)
            {
                twoRenderer.forceRenderingOff = true;
                threeRenderer.forceRenderingOff = true;
                two.SetActive(false);
                three.SetActive(false);
                return;
            }

            NoteRegister first = queue[1];
            NoteRegister? second = null;
            if (queue.Count > 2)
                second = queue[2];

            two.SetActive(true);
            twoRenderer.forceRenderingOff = false;
            if (first.IsBreak)
                twoRenderer.sprite = bReak[0];
            else if (first.IsEach)
                twoRenderer.sprite = each[0];
            else
                twoRenderer.sprite = normal[0];

            if (second is not null)
            {
                var _second = (NoteRegister)second;
                three.SetActive(true);
                threeRenderer.forceRenderingOff = false;
                if (_second.IsBreak)
                    threeRenderer.sprite = bReak[1];
                else if (_second.IsEach)
                    threeRenderer.sprite = each[1];
                else
                    threeRenderer.sprite = normal[1];
            }
            else
            {
                threeRenderer.forceRenderingOff = true;
                three.SetActive(false);
            }

        }
        struct NoteRegister
        {
            public bool IsBreak { get; set; }
            public bool IsEach { get; set; }
        }
    }
}