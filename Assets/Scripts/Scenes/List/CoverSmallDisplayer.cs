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
    public class CoverSmallDisplayer : MonoBehaviour
    {
        public bool IsOnline { get; set; } = false;
        public RectTransform RectTransform => _rectTransform;

        public Image Cover;
        public Image LevelBackground;
        public Image Background;
        public TextMeshProUGUI LevelText;
        public GameObject Loading;
        public GameObject Icon;
        public GameObject? mask = null;

        RectTransform _rectTransform;
        ISongDetail _boundSong;

        bool _isRefreshed = false;
        ListManager _listManager;
        CancellationToken _cancellationToken = default;

        void Awake()
        {
            _cancellationToken = _listManager.CancellationToken;
            _rectTransform = GetComponent<RectTransform>();

            if(!Loading.IsUnityNull())
            {
                Loading.SetActive(false);
            }
        }
        private void Start()
        {
            _listManager = MajInstanceHelper<ListManager>.Instance!;
            if (IsOnline)
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
                ShowCoverAsync();
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
        public void ShowCoverAsync()
        {
            if (_boundSong != null)
            {
                if (!_isRefreshed)
                {
                    ListManager.AllBackguardTasks.Add(SetCoverAsync());
                    _isRefreshed = true;
                }
            }
        }
        async UniTask SetCoverAsync()
        {
            Loading.SetActive(true);
            var cover = await _boundSong.GetCoverAsync(true, _cancellationToken);
            _cancellationToken.ThrowIfCancellationRequested();
            Cover.sprite = cover;
            Loading.SetActive(false);
        }
    }
}