using System.Reflection;
using TMPro;
using UnityEngine;

namespace MajdataPlay.Scenes
{
    public class Option : MonoBehaviour
    {
        public PropertyInfo PropertyInfo { get; set; }
        public object OptionObject { get; set; }
        [SerializeField]
        TextMeshPro nameText;
        [SerializeField]
        TextMeshPro valueText;
        void Start()
        {
            nameText.text = PropertyInfo.Name;
            valueText.text = PropertyInfo.GetValue(OptionObject).ToString();
        }
    }
}
