using MajdataPlay.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MajdataPlay.Scenes
{
    public class Menu : MonoBehaviour
    {
        /// <summary>
        /// ×Ó¼¶Option<para>Æ©ÈçGameSetting.Game</para>
        /// </summary>
        public object SubOptionObject { get; set; }
        public GameObject optionPrefab;
        Option[] options = Array.Empty<Option>();
        void Start()
        {
            var type = SubOptionObject.GetType();
            var properties = type.GetProperties();
            options = new Option[properties.Length];

            foreach(var (i,property) in properties.WithIndex())
            {
                var optionObj = Instantiate(optionPrefab, transform);
                var option = optionObj.GetComponent<Option>();
                options[i] = option;
                option.PropertyInfo = property;
                option.OptionObject = SubOptionObject;
            }
        }
        void Update()
        {

        }
    }
}
