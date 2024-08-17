using MajdataPlay.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultScreenManager : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI acc;
    public Image coverImg;


    void Start()
    {
        var gameManager = GameManager.Instance;
        var song = gameManager.songList[gameManager.selectedIndex];
        title.text = song.Title;
        acc.text = gameManager.lastGameResult.ToString();
        coverImg.sprite = song.SongCover;
        IOManager.Instance.BindAnyArea(OnAreaDown);
        AudioManager.Instance.PlaySFX("Sugoi.wav");
    }

    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (e.IsClick && e.IsButton && e.Type == MajdataPlay.Types.SensorType.A4)
        {
            SceneManager.LoadScene(1);
        }
    }

    private void OnDestroy()
    {
        IOManager.Instance.UnbindAnyArea(OnAreaDown);
    }
}
