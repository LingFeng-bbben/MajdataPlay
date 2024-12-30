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
        public GameObject GameObject => _gameObject;
        public Transform Transform => _transform;
        public RectTransform RectTransform => _rectTransform;
        public bool Active
        {
            get  => _active;
            set
            {
                SetActive(value);
            }
        }
        public bool IsOnline { get; set; } = false;
        public Image Cover => _cover;
        public Image LevelBackground => _levelBackground;
        public TextMeshProUGUI LevelText => _levelText;
        
        bool _isRefreshed = false;
        bool _active = false;

        Image _icon;
        Image _cover;
        Image _levelBackground;
        SongDetail _boundSong;
        TextMeshProUGUI _levelText;
        GameObject _gameObject;
        Transform _transform;
        RectTransform _rectTransform;

        readonly CancellationTokenSource _cts = new();

        static int HIDDEN_LAYER = 3;
        static int UI_LAYER = 5;
        void Awake()
        {
            _gameObject = gameObject;
            _transform = transform;
            _rectTransform  = _transform.GetComponent<RectTransform>();
            _cover = _transform.GetComponent<Image>();
            _icon = _transform.GetChild(1).GetComponent<Image>();
            _levelBackground = _transform.GetChild(0).GetComponent<Image>();
            _levelText = _transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            _active = _gameObject.layer == UI_LAYER;
        }
        private void Start()
        {
            if(IsOnline)
                _icon.gameObject.SetActive(true);
        }
        public void SetActive(bool state)
        {
            if (state == _active)
                return;

            _active = state;
            switch (state)
            {
                case true:
                    
                    break;
                case false:
                    SetOpacity(0f);
                    break;
            }
        }
        public void SetOpacity(float alpha)
        {
            Cover.color = new Color(Cover.color.r, Cover.color.g, Cover.color.b, alpha);
            LevelBackground.color = new Color(LevelBackground.color.r, LevelBackground.color.g, LevelBackground.color.b, alpha);
            LevelText.color = new Color(LevelText.color.r, LevelText.color.g, LevelText.color.b, alpha);
            if (alpha > 0.5f)
            {
                ShowCover();
            }
        }
        public void SetLevelText(string text)
        {
            LevelText.text = text;
        }
        public void SetCover(SongDetail detail)
        {
            _boundSong = detail;
        }
        public void ShowCover()
        {
            if (_boundSong != null)
            {
                if (!_isRefreshed)
                {
                    SetCoverAsync(_boundSong).Forget();
                    _isRefreshed = true;
                }
            }
        }
        void OnDestroy()
        {
            _cts.Cancel();
        }
        async UniTaskVoid SetCoverAsync(SongDetail detail)
        {
            var spriteTask = detail.GetSpriteAsync(_cts.Token);
            //TODO:set the cover to be now loading?
            while (!spriteTask.IsCompleted)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, _cts.Token);
            _cts.Token.ThrowIfCancellationRequested();
            Cover.sprite = spriteTask.Result;
        }
    }
}