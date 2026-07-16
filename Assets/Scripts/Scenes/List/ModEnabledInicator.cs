using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Scenes.List
{
    public class ModEnabledInicator : MonoBehaviour
    {
        void Awake()
        {
            gameObject.SetActive(MajInstances.GameManager.Settings.Mod.IsAnyModActive());
        }
    }
}