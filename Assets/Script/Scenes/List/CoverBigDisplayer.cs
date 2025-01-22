using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.List
{
    public class CoverBigDisplayer : MonoBehaviour
    {
        public Image bgCard;
        public Image Cover;
        public TMP_Text Level;
        public TMP_Text Charter;
        public TMP_Text Title;
        public TMP_Text Artist;
        public TMP_Text ArchieveRate;
        public GameObject APbg;
        public TMP_Text ClearMark;
        public TMP_Text Rank;

        public Color[] diffColors = new Color[6];

        CancellationTokenSource? _cts = null;

        private void Start()
        {
            /* Level = transform.Find("Level").GetComponent<TMP_Text>();
             Charter = transform.Find("Designer").GetComponent<TMP_Text>();
             Title = transform.Find("Title").GetComponent<TMP_Text>();
             Artist = transform.Find("Artist").GetComponent<TMP_Text>();
             ArchieveRate = transform.Find("Rate").GetComponent<TMP_Text>();*/
        }
        public void SetDifficulty(int i)
        {
            bgCard.color = diffColors[i];
            if (i + 1 < diffColors.Length)
            {
                MajInstances.LightManager.SetButtonLight(diffColors[i + 1], 0);
            }
            else
            {
                MajInstances.LightManager.SetButtonLight(diffColors.First(), 0);
            }
            if (i - 1 >= 0)
            {
                MajInstances.LightManager.SetButtonLight(diffColors[i - 1], 7);
            }
            else
            {
                MajInstances.LightManager.SetButtonLight(diffColors.Last(), 7);
            }

        }
        public void SetCover(SongDetail detail)
        {
            if(_cts is not null)
                _cts.Cancel();
            _cts = new();
            SetCoverAsync(detail, _cts.Token).Forget();
        }
        public void SetNoCover()
        {
            Cover.sprite = null;
        }
        void OnDestroy()
        {
            _cts?.Cancel();    
        }
        async UniTaskVoid SetCoverAsync(SongDetail detail, CancellationToken ct = default)
        {
            var spriteTask = detail.GetSpriteAsync(ct);
            //TODO:set the cover to be now loading?
            while (!spriteTask.IsCompleted)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);
            ct.ThrowIfCancellationRequested();
            Cover.sprite = spriteTask.Result;
        }

        public void SetMeta(string _Title, string _Artist, string _Charter, string _Level)
        {
            Title.text = _Title;
            Artist.text = _Artist;
            Charter.text = _Charter;
            Level.text = _Level;
        }
        public void SetScore(MaiScore score)
        {
            if (score.PlayCount == 0)
            {
                APbg.SetActive(false);
                ArchieveRate.enabled = false;
                Rank.text = "";
            }
            else
            {
                var isClassic = MajInstances.GameManager.Setting.Judge.Mode == JudgeMode.Classic;
                ArchieveRate.text = isClassic ? $"{score.Acc.Classic:F2}%" : $"{score.Acc.DX:F4}%";
                ArchieveRate.enabled = true;
                APbg.SetActive(false);
                if (score.ComboState == ComboState.APPlus)
                {
                    APbg.SetActive(true);
                    ClearMark.text = "AP+";
                }
                else if (score.ComboState == ComboState.AP)
                {
                    APbg.SetActive(true);
                    ClearMark.text = "AP";
                }
                else if (score.ComboState == ComboState.FCPlus)
                {
                    APbg.SetActive(true);
                    ClearMark.text = "FC+";
                }
                else if (score.ComboState == ComboState.FC)
                {
                    APbg.SetActive(true);
                    ClearMark.text = "FC";
                }
                var dxacc = score.Acc.DX;
                var rank = Rank;
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
                    Rank.text = "";
                }
            }
        }
    }
}