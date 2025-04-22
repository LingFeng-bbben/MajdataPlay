using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay
{
    public class VersionDisplayer : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var tmp = GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "Version: " + MajInstances.GameVersion;
        }
    }
}
