using LitMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void SetScore(GameResult result)
        {
            var score = new DXScoreRank(result.DXScore, result.TotalDXScore);
            if (score.Rank > 0)
            {
                _dxScoreDisplayer.text = $"{result.DXScore}/{result.TotalDXScore} ✧ {score.Rank}";
            }
            else
            {
                _dxScoreDisplayer.text = $"{result.DXScore}/{result.TotalDXScore}";
            }
            var percent = result.DXScore / (float)result.TotalDXScore;
            _slideBarAnim.TryCancel();
            _slideBarAnim = LMotion.Create(0f, percent, 0.5f)
                                   .WithEase(Ease.OutQuad)
                                   .Bind(x =>
                                   {
                                       _dxScoreSlider.value = percent;
                                   });
        }
    }
}
