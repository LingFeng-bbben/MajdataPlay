using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.Types;
using MajdataPlay.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;
using MajdataPlay;
using MajdataPlay.Game;

namespace MajdataPlay.TotalResult
{
    public class TotalResultManager : MonoBehaviour
    {
        public GameObject resultPrefab;
        public Transform resultPrefabParent;
        public TextMeshProUGUI initLife;
        public TextMeshProUGUI Life;
        public TextMeshProUGUI Title;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;
        // Start is called before the first frame update
        void Start()
        {
            LedRing.SetAllLight(Color.white);
            var results = _gameInfo.Results;
            var levels = _gameInfo.Levels;
            var songInfos = _gameInfo.Charts;
            var name = _gameInfo.DanInfo.Name;
            var life = _gameInfo.CurrentHP;
            initLife.text = "Start LIFE " + _gameInfo.DanInfo.StartHP + " Restore LIFE " + _gameInfo.DanInfo.RestoreHP;
            Life.text = "LIFE\n" + life.ToString();
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
        }

        async UniTaskVoid DelayBind()
        {
            await UniTask.Delay(1000);
            InputManager.BindAnyArea(OnAreaDown);
            LedRing.SetButtonLight(Color.green, 3);
        }

        private void OnAreaDown(object sender, InputEventArgs e)
        {
            InputManager.UnbindAnyArea(OnAreaDown);
            MajInstances.AudioManager.StopSFX("bgm_result.mp3");
            MajInstances.SceneSwitcher.SwitchScene("List", false);
            return;
        }
    }
}