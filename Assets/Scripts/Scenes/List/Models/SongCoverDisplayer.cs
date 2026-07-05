using Cysharp.Threading.Tasks;
using MajdataPlay.Drawing;
using MajdataPlay.Settings.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.List.Models
{
    internal class SongCoverDisplayer : CoverSmallDisplayer
    {
        [SerializeField]
        [FormerlySerializedAs("coverDisplayer")]
        Image _coverDisplayer;

        [SerializeField]
        [FormerlySerializedAs("levelDisplayerListRoot")]
        GameObject _levelDisplayerListRoot;

        [SerializeField]
        [FormerlySerializedAs("loadingComponent")]
        GameObject _loadingComponent;

        CancellationTokenSource _cts = new();

        LevelDisplayer[] _levelDisplayers = Array.Empty<LevelDisplayer>();
        LevelDPOriginPosition[] _levelDPOriginPositions = Array.Empty<LevelDPOriginPosition>();

        protected override void Awake()
        {
            base.Awake();
            var levelDisplayerListRoot = _levelDisplayerListRoot.transform;
            var displayerCount = levelDisplayerListRoot.childCount;
            _levelDisplayers = new LevelDisplayer[displayerCount];
            _levelDPOriginPositions = new LevelDPOriginPosition[displayerCount];
            for (var i = 0; i < displayerCount; i++)
            {
                var child = levelDisplayerListRoot.GetChild(i);
                var textTransform = child.Find("Text");
                if (textTransform == null)
                {
                    throw new Exception($"Level displayer {i} does not have a child named 'Text'.");
                }
                var textDisplayer = textTransform.GetComponent<TextMeshProUGUI>();
                if (textDisplayer == null)
                {
                    throw new Exception($"Level displayer {i}'s 'Text' child does not have a TextMeshProUGUI component.");
                }
                _levelDisplayers[i] = new LevelDisplayer
                {
                    Transform = child,
                    TextTransform = textTransform,
                    TextDisplayer = textDisplayer
                };
                _levelDPOriginPositions[i] = new LevelDPOriginPosition
                {
                    Position = child.localPosition,
                    Rotation = child.localRotation,
                    TextRotation = textTransform.localRotation
                };
            }
        }
        public void SetSongDetail(ISongDetail detail)
        {
            SetLevelText(detail);
            _coverDisplayer.sprite = SpriteLoader.EmptySprite;
            if(!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts = new();
            }
            ListManager.AllBackgroundTasks.Add(SetCoverAsync(detail, _cts.Token));
        }
        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }
        void SetLevelText(ISongDetail songDetail)
        {
            var posIndex = _levelDPOriginPositions.Length - 1;
            for (var i = songDetail.Levels.Length - 1; i >= 0; i--)
            {
                var level = songDetail.Levels[i];
                if(string.IsNullOrEmpty(level))
                {
                    continue;
                }
                var displayer = _levelDisplayers[i];
                var originPos = _levelDPOriginPositions[posIndex--];
                displayer.Transform.localPosition = originPos.Position;
                displayer.Transform.localRotation = originPos.Rotation;
                displayer.TextTransform.localRotation = originPos.TextRotation;
                displayer.TextDisplayer.text = level;
            }
        }
        async Task SetCoverAsync(ISongDetail songDetail, CancellationToken token = default)
        {
            _loadingComponent.SetActive(true);
            _coverDisplayer.sprite = SpriteLoader.EmptySprite;
            var cover = await songDetail.GetCoverAsync(true, token: token);
            if(token.IsCancellationRequested)
            {
                return;
            }
            await UniTask.SwitchToMainThread();
            _coverDisplayer.sprite = cover;
            _loadingComponent.SetActive(false);
        }

        readonly struct LevelDisplayer
        {
            public required Transform Transform { get; init; }
            public required Transform TextTransform { get; init; }
            public required TextMeshProUGUI TextDisplayer { get; init; }
        }
        readonly struct LevelDPOriginPosition
        {
            public Vector3 Position { get; init; }
            public Quaternion Rotation { get; init; }
            public Quaternion TextRotation { get; init; }
        }
    }
}
