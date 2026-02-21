using Cysharp.Threading.Tasks;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.IO;
using MajdataPlay.Utils;
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
#nullable enable
namespace MajdataPlay.Scenes.List
{
    public class CoverBigDisplayer : MonoBehaviour
    {
        [SerializeField]
        Image _bgCard;
        [SerializeField]
        Image _cover;
        [SerializeField]
        TMP_Text _level;
        [SerializeField]
        TMP_Text _charter;
        [SerializeField]
        TMP_Text _title;
        [SerializeField]
        TMP_Text _artist;
        [SerializeField]
        TMP_Text _archieveRate;
        [SerializeField]
        GameObject _APbg;
        [SerializeField]
        TMP_Text _clearMark;
        [SerializeField]
        TMP_Text _rank;
        [SerializeField]
        GameObject _loadingObj;

        public Color[] diffColors = new Color[6];

        int diff = 0;

        CancellationTokenSource? _cts = null;
        ChartAnalyzer _chartAnalyzer;
        CoverListDisplayer _listDisplayer;
        ListManager _listManager;
        private void Awake()
        {
            /* Level = transform.Find("Level").GetComponent<TMP_Text>();
             Charter = transform.Find("Designer").GetComponent<TMP_Text>();
             Title = transform.Find("Title").GetComponent<TMP_Text>();
             Artist = transform.Find("Artist").GetComponent<TMP_Text>();
             ArchieveRate = transform.Find("Rate").GetComponent<TMP_Text>();*/
            _chartAnalyzer = GameObject.FindObjectOfType<ChartAnalyzer>();
            _loadingObj.SetActive(false);
        }
        void Start()
        {
            _listDisplayer = Majdata<CoverListDisplayer>.Instance!;
            _listManager = Majdata<ListManager>.Instance!;
        }
        public void SetDifficulty(int i)
        {
            _bgCard.color = diffColors[i];
            diff = i;
            if (i + 1 < diffColors.Length)
            {
                LedRing.SetButtonLight(diffColors[i + 1], 0);
            }
            else
            {
                LedRing.SetButtonLight(diffColors.First(), 0);
            }
            if (i - 1 >= 0)
            {
                LedRing.SetButtonLight(diffColors[i - 1], 7);
            }
            else
            {
                LedRing.SetButtonLight(diffColors.Last(), 7);
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
            _chartAnalyzer.AnalyzeAndDrawGraphAsync(detail, (ChartLevel)diff, token: linkedCts.Token).Forget();
        }
        public void SetNoCover()
        {
            _cover.sprite = null!;
        }
        void OnDestroy()
        {
            _cts?.Cancel();    
        }
        async Task SetCoverAsync(ISongDetail detail, CancellationToken ct = default)
        {
            _loadingObj.SetActive(true);
            _cover.sprite = SpriteLoader.EmptySprite;
            var cover = await detail.GetCoverAsync(true, token: ct);
            await UniTask.SwitchToMainThread();
            ct.ThrowIfCancellationRequested();
            _cover.sprite = cover;
            _loadingObj.SetActive(false);
        }

        public void SetMeta(string _Title, string _Artist, string _Charter, string _Level)
        {
            _title.text = _Title;
            _artist.text = _Artist;
            _charter.text = _Charter;
            _level.text = _Level;
        }
        public void SetScore(MaiScore score)
        {
            if (score.PlayCount == 0)
            {
                _APbg.SetActive(false);
                _archieveRate.enabled = false;
                _rank.text = "";
            }
            else
            {
                var isClassic = MajInstances.GameManager.Setting.Judge.Mode == JudgeModeOption.Classic;
                _archieveRate.text = isClassic ? $"{score.Acc.Classic:F2}%" : $"{score.Acc.DX:F4}%";
                _archieveRate.enabled = true;
                _APbg.SetActive(false);
                if (score.ComboState == ComboState.APPlus)
                {
                    _APbg.SetActive(true);
                    _clearMark.text = "AP+";
                }
                else if (score.ComboState == ComboState.AP)
                {
                    _APbg.SetActive(true);
                    _clearMark.text = "AP";
                }
                else if (score.ComboState == ComboState.FCPlus)
                {
                    _APbg.SetActive(true);
                    _clearMark.text = "FC+";
                }
                else if (score.ComboState == ComboState.FC)
                {
                    _APbg.SetActive(true);
                    _clearMark.text = "FC";
                }
                var dxacc = score.Acc.DX;
                var rank = _rank;
                if (dxacc >= 100.5f)
                {
                    rank.text = "SSS+";
                }
                else if (dxacc >= 100f)
                {
                    rank.text = "SSS";
                }
                else if (dxacc >= 99.5f)
                {
                    rank.text = "SS+";
                }
                else if (dxacc >= 99f)
                {
                    rank.text = "SS";
                }
                else if (dxacc >= 98f)
                {
                    rank.text = "S+";
                }
                else if (dxacc >= 97f)
                {
                    rank.text = "S";
                }
                else
                {
                    _rank.text = "";
                }
            }
        }
    }
}