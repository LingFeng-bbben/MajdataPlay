using System.Collections;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay
{
    public class LoadSubBg : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(WaitSkinManager());
        }
        IEnumerator WaitSkinManager()
        {
            var woe = new WaitForEndOfFrame();
            while (!MajInstances.SkinManager.IsInited)
            {
                yield return woe;
            }
            var img = GetComponent<Image>();
            img.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            img.color = Color.white;
        }
    }
}