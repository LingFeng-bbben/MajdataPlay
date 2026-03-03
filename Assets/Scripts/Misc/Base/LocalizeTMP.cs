using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay
{
    public class LocalizeTMP : MonoBehaviour
    {
        public string local_key;
        TMP_Text text;
        Text text_legacy;
        // Start is called before the first frame update
        void Start()
        {
            try
            {
                text = GetComponent<TMP_Text>();
                text.text = local_key.i18n();
            }
            catch { }
            try
            {
                text_legacy = GetComponent<Text>();
                text_legacy.text = local_key.i18n();
            }
            catch { }
        }

    }
}