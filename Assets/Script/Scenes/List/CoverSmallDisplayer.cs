using Cysharp.Threading.Tasks;
using MajdataPlay.Types;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.List
{
    public class CoverSmallDisplayer : MonoBehaviour
    {
        
        public bool IsOnline { get; set; } = false;
        
        bool _isRefreshed = false;

        public RectTransform RectTransform;
        public Image Cover;
        public Image LevelBackground;
        public Image Background;
        public TextMeshProUGUI LevelText;
        public GameObject Loading;
        public GameObject Icon;
        public GameObject? mask = null;
        ISongDetail _boundSong;

        readonly CancellationTokenSource _cts = new();

        void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
        private void Start()
        {
            if(IsOnline)
                Icon.gameObject.SetActive(true);
        }
        public void SetOpacity(float alpha)
        {
            Cover.color = new Color(Cover.color.r, Cover.color.g, Cover.color.b, alpha);
            LevelBackground.color = new Color(LevelBackground.color.r, LevelBackground.color.g, LevelBackground.color.b, alpha);
            LevelText.color = new Color(LevelText.color.r, LevelText.color.g, LevelText.color.b, alpha);
            Background.color = new Color(Background.color.r, Background.color.g, Background.color.b, alpha);
            if (alpha > 0.5f)
            {
                ShowCover();
            }
        }
        public void SetLevelText(string text)
        {
            LevelText.text = text;
        }
        public void SetSongDetail(ISongDetail detail)
        {
            _boundSong = detail;
        }
        public void ShowCover()
        {
            if (_boundSong != null)
            {
                if (!_isRefreshed)
                {
                    SetCoverAsync().Forget();
                    _isRefreshed = true;
                }
            }
        }
        void OnDestroy()
        {
            _cts.Cancel();
        }
        async UniTaskVoid SetCoverAsync()
        {
            var token = _cts.Token;
            Loading.SetActive(true);
            var cover = await _boundSong.GetCoverAsync(true, token);
            token.ThrowIfCancellationRequested();
            Cover.sprite = cover;
            Loading.SetActive(false);
        }
    }
}