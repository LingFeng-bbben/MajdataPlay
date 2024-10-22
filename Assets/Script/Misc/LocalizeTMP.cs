using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            text.text = Localization.GetLocalizedText(local_key);
        }
        catch { }
        try
        {
            text_legacy = GetComponent<Text>();
            text_legacy.text = Localization.GetLocalizedText(local_key);
        }
        catch { }
    }

}
