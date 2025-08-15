using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Scenes.List
{
    public class ModEnabledInicator : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            gameObject.SetActive(MajInstances.GameManager.Setting.Mod.IsAnyModActive());
        }
    }
}