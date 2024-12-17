using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Game
{
    public class TimeDisplayer : MonoBehaviour
    {
        Text _timeText;

        GamePlayManager _gpManager;


        void Start()
        {
            _timeText = GetComponent<Text>();
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
        }

        void Update()
        {
            // Lock AudioTime variable for real
            var ctime = _gpManager.AudioTime;
            var timenowInt = (int)ctime;
            var minute = timenowInt / 60;
            var second = timenowInt - 60 * minute;
            double mili = (ctime - timenowInt) * 10000;

            // Make timing display "cleaner" on negative timing.
            if (ctime < 0)
            {
                minute = Math.Abs(minute);
                second = Math.Abs(second);
                mili = Math.Abs(mili);
                
                //_timeText.text = string.Format("-{0}:{1:00}.{2:000}", minute, second, mili / 10);
                _timeText.text = $"-{minute}:{second:00}.{mili / 10:000}";
            }
            else
            {
                _timeText.text = $"{minute}:{second:00}.{mili / 10:000}";
            }
        }
    }
}