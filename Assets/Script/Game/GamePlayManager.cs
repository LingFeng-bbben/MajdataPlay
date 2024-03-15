using SimaiParserWithAntlr;
using SimaiParserWithAntlr.DataModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance;

    public BackgroundDisplayer BackgroundDisplayer;
    AudioSampleWrap audioSample;
    ChartParser Chart;

    SongDetail song;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        print(GameManager.Instance.selectedIndex);
        song = GameManager.Instance.songList[GameManager.Instance.selectedIndex];
    }
    void Start()
    {
        BackgroundDisplayer.SetBackground(song.SongCover);

        audioSample = AudioManager.Instance.LoadMusic(song.TrackPath);
        audioSample.Play();
        Chart = ChartParser.GenerateFromText(song.InnerMaidata[4]);
        print(Chart.NoteList.Count);
    }

    private void OnDestroy()
    {
        audioSample = null;
    }
    int i = 0;
    // Update is called once per frame
    void Update()
    {
        var delta = Math.Abs(audioSample.GetCurrentTime() - (Chart.NoteList[i].Timing.Time + song.First + 1.25));
        if( delta < 0.01) {
            AudioManager.Instance.PlaySFX("answer.wav");
            print(Chart.NoteList[i].Timing);
            i++;
        }
    }

/*    double convertTime(NoteTiming timing, double bpm)
    {
        //return 60/bpm * 4 * timing.Bar + 60 / bpm * 4 /
    }*/

}
