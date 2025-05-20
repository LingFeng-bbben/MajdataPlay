using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.List
{
    internal class SongCoverSmallDisplayer : CoverSmallDisplayer
    {
        public int SelectedDifficulty { get; set; } = 0;
        [SerializeField]
        Image _songCover;
        [SerializeField]
        Image _levelBackground;
        [SerializeField]
        Image _background;
        [SerializeField]
        TextMeshProUGUI _levelText;
        [SerializeField]
        GameObject _loading;

        bool _isRefreshed = false;
        ISongDetail _boundSong;
        CancellationTokenSource _cts = new();

        protected override void Awake()
        {
            base.Awake();
            if (!_loading.IsUnityNull())
            {
                _loading.SetActive(false);
            }
        }
        public void SetOpacity(float alpha)
        {
            _songCover.color = new Color(_songCover.color.r, _songCover.color.g, _songCover.color.b, alpha);
            _levelBackground.color = new Color(_levelBackground.color.r, _levelBackground.color.g, _levelBackground.color.b, alpha);
            _levelText.color = new Color(_levelText.color.r, _levelText.color.g, _levelText.color.b, alpha);
            _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, alpha);
            if (alpha > 0.5f)
            {
                ShowCoverAsync();
            }
        }
        public void SetLevel(int selectedDifficulty)
        {
            SelectedDifficulty = selectedDifficulty;
            if(_boundSong is not null)
            {
                var text = _boundSong.Levels[SelectedDifficulty];
                if(string.IsNullOrEmpty(text))
                {
                    text = "-";
                }
                _levelText.text = text;
            }
        }
        public void SetSongDetail(ISongDetail detail)
        {
            _boundSong = detail;
            var text = _boundSong.Levels[SelectedDifficulty];
            if (string.IsNullOrEmpty(text))
            {
                text = "-";
            }
            _songCover.sprite = null;
            _levelText.text = text;
            _isRefreshed = false;
            _cts.Cancel();
            _cts = new();
        }
        public void ShowCoverAsync()
        {
            if (_boundSong != null)
            {
                if (!_isRefreshed)
                {
                    ListManager.AllBackgroundTasks.Add(SetCoverAsync(_cts.Token));
                    _isRefreshed = true;
                }
            }
        }
        async UniTask SetCoverAsync(CancellationToken token = default)
        {
            _loading.SetActive(true);
            var cover = await _boundSong.GetCoverAsync(true, token);
            token.ThrowIfCancellationRequested();
            _songCover.sprite = cover;
            _loading.SetActive(false);
        }
    }
}
