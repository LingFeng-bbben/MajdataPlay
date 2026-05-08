using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Settings;

namespace MajdataPlay.Scenes.TotalResult
{
    public class TotalResultManager : MonoBehaviour
    {
        public GameObject resultPrefab;
        public Transform resultPrefabParent;
        public TextMeshProUGUI life;
        public TextMeshProUGUI totalAchievement;
        public TextMeshProUGUI Title;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;
        bool _isExited = false;
        bool _isInited = false;
        void Awake()
        {
            InputManager.TouchButtonRingEdge = 4.8f;
        }
        void Start()
        {
            CabinetLed.SetAllLight(Color.white);
            var results = _gameInfo.Results;
            var levels = _gameInfo.Levels;
            var songInfos = _gameInfo.Charts;
            var name = _gameInfo.DanInfo.Name;
            var isClassic = MajInstances.GameManager.Settings.Judge.Mode == JudgeModeOption.Classic;
            var totalAchievementValue = results.Sum(result => isClassic ? result.Acc.Classic : result.Acc.DX);
            if (_gameInfo.IsDanLifeEnabled)
            {
                life.text = "LIFE " + _gameInfo.CurrentHP + " / " + _gameInfo.MaxHP;
            }
            else
            {
                life.text = "LIFE Disabled";
            }
            totalAchievement.text = isClassic ? $"Total {totalAchievementValue:F2}%" : $"Total {totalAchievementValue:F4}%";
            Title.text = name;
            for (var i = 0; i < songInfos.Length; i++)
            {
                var songInfo = Instantiate(resultPrefab, resultPrefabParent);
                var result = results[i];
                //if (i < results.Length)
                //{
                //    result = results[i];
                //}
                //else if (i == results.Length)
                //{
                //    result = (GameResult)GameManager.LastGameResult;
                //}
                songInfo.GetComponent<TotalResultSmallDisplayer>().DisplayResult(songInfos[i], result, (ChartLevel)levels[i]);
            }
            //SongStorage.WorkingCollection.Reset();
            //MajInstances.GameManager.isDanMode = false;
            DelayBind().Forget();
            MajInstances.AudioManager.StopSFX("bgm_result.mp3");
            MajInstances.AudioManager.PlaySFX("bgm_dan.mp3", true);
            if (!_gameInfo.IsDanLifeEnabled || _gameInfo.CurrentHP > 0)
            {
                MajInstances.AudioManager.PlaySFX("challenge_clear.wav");
            }
            else
            {
                MajInstances.AudioManager.PlaySFX("challenge_fail.wav");
            }
        }

        async UniTaskVoid DelayBind()
        {
            await UniTask.Delay(1000);
            CabinetLed.SetButtonLight(Color.green, 3);
            _isInited = true;
        }
        void Update()
        {
            if(_isExited || !_isInited)
            {
                return;
            }

            if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A4))
            {
                MajInstances.AudioManager.StopSFX("bgm_dan.mp3");
                _isExited = true;
                MajInstances.SceneSwitcher.SwitchScene("List", false);
                
            }
        }
        void OnDestroy()
        {
            InputManager.TouchButtonRingEdge = 5.4f;
        }
    }
}
