using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Game
{
    public class TimeDisplayer : MonoBehaviour
    {
        public Text timeText;
        public Text rTimeText;
        public Slider progress;

        GamePlayManager _gpManager;


        void Start()
        {
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
        }

        void Update()
        {
            // Lock AudioTime variable for real
            var ctime = _gpManager.AudioTime;
            timeText.text = TimeToString(ctime);
            rTimeText.text = TimeToString(ctime-_gpManager.AudioLength);
            progress.value = ctime / _gpManager.AudioLength;
        }

        string TimeToString(float time)
        {
            var timenowInt = (int)time;
            var minute = timenowInt / 60;
            var second = timenowInt - 60 * minute;
            double mili = (time - timenowInt) * 10000;

            // Make timing display "cleaner" on negative timing.
            if (time < 0)
            {
                minute = Math.Abs(minute);
                second = Math.Abs(second);
                mili = Math.Abs(mili);

                //_timeText.text = string.Format("-{0}:{1:00}.{2:000}", minute, second, mili / 10);
                return $"-{minute}:{second:00}.{mili / 10:000}";
            }
            else
            {
                return $"{minute}:{second:00}.{mili / 10:000}";
            }
        }
    }
}