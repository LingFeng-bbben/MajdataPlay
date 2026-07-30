using LitMotion;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MajdataPlay.Scenes.Result.Components
{
    public class DXScoreDisplayer : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("dxScoreSlider")]
        Slider _dxScoreSlider;

        [SerializeField]
        [FormerlySerializedAs("dxScoreDisplayer")]
        TextMeshProUGUI _dxScoreDisplayer;

        MotionHandle _slideBarAnim;
        MotionHandle _scoreTextAnim;
#if UNITY_EDITOR
        bool _hasTargetPercent;
        float _targetPercent;
        float _valueBeforePreview;
        long _targetDXScore;
        long _totalDXScore;
        int _dxScoreRank;
        string _textBeforePreview = string.Empty;
#endif

        public void SetScore(GameResult result, float animationDuration)
        {
            var score = new DXScoreRank(result.DXScore, result.TotalDXScore);
            var percent = result.DXScore / (float)result.TotalDXScore;
#if UNITY_EDITOR
            _hasTargetPercent = true;
            _targetPercent = percent;
            _targetDXScore = result.DXScore;
            _totalDXScore = result.TotalDXScore;
            _dxScoreRank = score.Rank;
#endif
            PlaySliderAnimation(percent, animationDuration);
            PlayScoreTextAnimation(result.DXScore, result.TotalDXScore, score.Rank, animationDuration);
        }

#if UNITY_EDITOR
        public void PlayAnimationPreview(float animationDuration)
        {
            _valueBeforePreview = _dxScoreSlider.value;
            _textBeforePreview = _dxScoreDisplayer.text;
            var previewTarget = _hasTargetPercent ? _targetPercent : _dxScoreSlider.maxValue;
            PlaySliderAnimation(previewTarget, animationDuration);
            if (_hasTargetPercent)
            {
                PlayScoreTextAnimation(_targetDXScore, _totalDXScore, _dxScoreRank, animationDuration);
            }
            else if (TryParseDisplayedScore(out var targetScore, out var totalScore, out var rank))
            {
                PlayScoreTextAnimation(targetScore, totalScore, rank, animationDuration);
            }
        }

        public void StopAnimationPreview()
        {
            _slideBarAnim.TryCancel();
            _scoreTextAnim.TryCancel();
            _dxScoreSlider.value = _hasTargetPercent ? _targetPercent : _valueBeforePreview;
            _dxScoreDisplayer.text = _hasTargetPercent
                ? FormatScore(_targetDXScore, _totalDXScore, _dxScoreRank)
                : _textBeforePreview;
        }
#endif

        void PlaySliderAnimation(float targetPercent, float animationDuration)
        {
            _slideBarAnim.TryCancel();
            _slideBarAnim = LMotion.Create(0f, targetPercent, animationDuration)
                                   .WithEase(Ease.OutQuart)
                                   .Bind(value => _dxScoreSlider.value = value);
        }

        void PlayScoreTextAnimation(long targetScore, long totalScore, int rank, float animationDuration)
        {
            _scoreTextAnim.TryCancel();
            _dxScoreDisplayer.text = FormatScore(0, totalScore, rank);
            _scoreTextAnim = LMotion.Create(0d, targetScore, animationDuration)
                                    .WithEase(Ease.OutQuart)
                                    .Bind(value =>
                                    {
                                        var displayedScore = (long)Math.Floor(value);
                                        _dxScoreDisplayer.text = FormatScore(displayedScore, totalScore, rank);
                                    });
        }

#if UNITY_EDITOR
        bool TryParseDisplayedScore(out long score, out long totalScore, out int rank)
        {
            score = 0;
            totalScore = 0;
            rank = 0;
            var text = _dxScoreDisplayer.GetParsedText();
            var slashIndex = text.LastIndexOf('/');
            if (slashIndex <= 0
             || !TryParseNumberBefore(text, slashIndex, out score)
             || !TryParseNumberAfter(text, slashIndex + 1, out totalScore))
            {
                return false;
            }

            var rankSymbolIndex = text.IndexOf('✧');
            if (rankSymbolIndex >= 0)
            {
                TryParseNumberAfter(text, rankSymbolIndex + 1, out rank);
            }
            return true;
        }

        static bool TryParseNumberBefore(string text, int endIndex, out long value)
        {
            var startIndex = endIndex;
            while (startIndex > 0 && char.IsDigit(text[startIndex - 1]))
            {
                startIndex--;
            }
            return long.TryParse(text.Substring(startIndex, endIndex - startIndex), out value);
        }

        static bool TryParseNumberAfter(string text, int startIndex, out long value)
        {
            var endIndex = startIndex;
            while (endIndex < text.Length && char.IsDigit(text[endIndex]))
            {
                endIndex++;
            }
            return long.TryParse(text.Substring(startIndex, endIndex - startIndex), out value);
        }

        static bool TryParseNumberAfter(string text, int startIndex, out int value)
        {
            var endIndex = startIndex;
            while (endIndex < text.Length && char.IsDigit(text[endIndex]))
            {
                endIndex++;
            }
            return int.TryParse(text.Substring(startIndex, endIndex - startIndex), out value);
        }
#endif

        static string FormatScore(long score, long totalScore, int rank)
        {
            return rank > 0
                ? $"<Color=#A2C830>✧{rank}<Color=#4C3A37> {score}/{totalScore}"
                : $"<Color=#4C3A37> {score}/{totalScore}";
        }

        void OnDestroy()
        {
            _slideBarAnim.TryCancel();
            _scoreTextAnim.TryCancel();
        }
    }
}
