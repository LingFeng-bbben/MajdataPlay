using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.List
{
    [RequireComponent(typeof(RectTransform))]
    public class CoverSmallDisplayer : MonoBehaviour
    {
        public bool IsOnline { get; set; } = false;
        public RectTransform RectTransform => _rectTransform;

        RectTransform _rectTransform;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}