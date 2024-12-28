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

public class TotalResultManager : MonoBehaviour
{
    public GameObject resultPrefab;
    public Transform resultPrefabParent;
    public TextMeshProUGUI Life;
    public TextMeshProUGUI Title;
    // Start is called before the first frame update
    void Start()
    {
        MajInstances.LightManager.SetAllLight(Color.white);
        var results = MajInstances.GameManager.DanResults;
        var levels = SongStorage.WorkingCollection.DanInfo.SongLevels;
        var songInfos = SongStorage.WorkingCollection.ToArray();
        var name = SongStorage.WorkingCollection.DanInfo.Name;
        var life = MajInstances.GameManager.DanHP;
        Life.text = "LIFE\n" + life.ToString();
        Title.text = name;
        for (int i = 0; i < songInfos.Length; i++)
        {
            GameObject songInfo = Instantiate(resultPrefab, resultPrefabParent);
            var result = new GameResult();
            if (i < results.Count)
            {
                result = results[i];
            }
            else if (i == results.Count) {
                result = (GameResult)GameManager.LastGameResult;
            }
            songInfo.GetComponent<TotalResultSmallDisplayer>().DisplayResult(songInfos[i], result, (ChartLevel)levels[i]);
        }
        SongStorage.WorkingCollection.Reset();
        MajInstances.GameManager.isDanMode = false;
        DelayBind().Forget();
    }

    async UniTaskVoid DelayBind()
    {
        await UniTask.Delay(1000);
        MajInstances.InputManager.BindAnyArea(OnAreaDown);
        MajInstances.LightManager.SetButtonLight(Color.green, 3);
    }

    private void OnAreaDown(object sender, InputEventArgs e)
    {

        MajInstances.InputManager.UnbindAnyArea(OnAreaDown);
        MajInstances.AudioManager.StopSFX("bgm_result.mp3");
        MajInstances.SceneSwitcher.SwitchScene("List");
        return;
    }
}
