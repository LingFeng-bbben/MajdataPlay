using Cysharp.Threading.Tasks;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajdataPlay.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MajdataPlay.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CenterCoverDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("levelRingDisplayer")]
        Image _levelRingDisplayer;
        [SerializeField]
        [FormerlySerializedAs("songCoverDisplayer")]
        Image _songCoverDisplayer;

        [SerializeField]
        [FormerlySerializedAs("loadingObj")]
        GameObject _loadingObj;

        [SerializeField]
        [FormerlySerializedAs("diffColors")]
        Color[] _diffColors = new Color[6];

        [SerializeField]
        [FormerlySerializedAs("scoreDisplayer")]
        MaiScoreDisplayer _scoreDisplayer;

        [SerializeField]
        [FormerlySerializedAs("metadataDisplayer")]
        ChartMetadataDisplayer _metadataDisplayer;

        [SerializeField]
        [FormerlySerializedAs("chartAnalyzer")]
        ChartAnalyzer _chartAnalyzer;

        int _diff = 0;

        CancellationTokenSource? _cts = null;
        
        CoverListManager _listDisplayer;
        ListManager _listManager;
        private void Awake()
        {
            _loadingObj.SetActive(false);
        }
        void Start()
        {
            _listDisplayer = Majdata<CoverListManager>.Instance!;
            _listManager = Majdata<ListManager>.Instance!;
        }
        void OnDestroy()
        {
            _cts?.Cancel();
        }

        public void SetDifficulty(int i)
        {
            _levelRingDisplayer.color = _diffColors[i];
            _diff = i;
            if (i + 1 < _diffColors.Length)
            {
                CabinetLed.SetButtonLight(_diffColors[i + 1], 0);
            }
            else
            {
                CabinetLed.SetButtonLight(_diffColors.First(), 0);
            }
            if (i - 1 >= 0)
            {
                CabinetLed.SetButtonLight(_diffColors[i - 1], 7);
            }
            else
            {
                CabinetLed.SetButtonLight(_diffColors.Last(), 7);
            }

        }
        public void SetSongDetail(ISongDetail detail)
        {
            if(_cts is not null)
            {
                _cts.Cancel();
            }
            _cts = new();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_listManager.CancellationToken, _cts.Token);
            ListManager.AllBackgroundTasks.Add(SetCoverAsync(detail, linkedCts.Token));
        }
        public void SetNoCover()
        {
            _songCoverDisplayer.sprite = SpriteLoader.EmptySprite;
        }
        
        async Task SetCoverAsync(ISongDetail detail, CancellationToken ct = default)
        {
            _loadingObj.SetActive(true);
            _songCoverDisplayer.sprite = SpriteLoader.EmptySprite;
            var cover = await detail.GetCoverAsync(true, token: ct);
            await UniTask.SwitchToMainThread();
            ct.ThrowIfCancellationRequested();
            _songCoverDisplayer.sprite = cover;
            _loadingObj.SetActive(false);
        }
    }
}