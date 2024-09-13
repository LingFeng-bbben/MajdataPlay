using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MajdataPlay.Scenes
{
    public class Option : MonoBehaviour
    {
        public int Index { get; set; } 
        public Menu Parent { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object OptionObject { get; set; }
        [SerializeField]
        TextMeshPro nameText;
        [SerializeField]
        TextMeshPro valueText;

        int lastIndex = 0;
        void Start()
        {
            nameText.text = PropertyInfo.Name;
            valueText.text = PropertyInfo.GetValue(OptionObject).ToString();
            UpdatePosition();
        }
        void Update()
        {
            if (lastIndex == Parent.SelectedIndex)
                return;
            lastIndex = Parent.SelectedIndex;
            UpdatePosition();
        }
        void UpdatePosition()
        {
            var diff = lastIndex - Index;
            var scale = GetScale(diff);
            var pos = GetPosition(diff);
            transform.localPosition = pos;
            transform.localScale = scale;
        }
        Vector3 GetScale(int diff)
        {
            switch(diff)
            {
                case 2:
                case -2:
                    return new Vector3(0.632f, 0.632f, 0.632f);
                case 1:
                case -1:
                    return new Vector3(0.738f, 0.738f, 0.738f);
                case 0:
                    return new Vector3(1, 1, 1);
                default:
                    return Vector3.zero;
            }
        }
        Vector3 GetPosition(int diff)
        {
            switch (diff)
            {
                case 2:
                    return new Vector3(0, 368, 0);
                case -2:
                    return new Vector3(0, -368, 0);
                case 1:
                    return new Vector3(0, 200, 0);
                case -1:
                    return new Vector3(0, -200, 0);
                case 0:
                    return new Vector3(0, 0, 0);
                default:
                    return new Vector3(0,1000,0);
            }
        }
    }
}
