using MajdataPlay;
using System.Collections;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MajdataPlay.Misc
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
            yield return new WaitForEndOfFrame();
            var img = GetComponent<Image>();
            img.sprite = MajInstances.SkinManager.SelectedSkin.SubDisplay;
            img.color = Color.white;
        }
    }
}