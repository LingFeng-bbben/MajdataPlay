using MajdataPlay.Game.Notes;
using MajdataPlay.IO;
using MajSimaiDecode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance;

    AudioSampleWrap audioSample;
    SimaiProcess Chart;
    SongDetail song;

    NoteLoader noteLoader;

    public GameObject notesParent;
    public GameObject tapPrefab;

    public float noteSpeed = 9f;
    public float touchSpeed = 7.5f;

    public float AudioTime = 0f;
    public bool isStart => audioSample.GetPlayState();
    public float CurrentSpeed = 1f;

    private int noteSortOrder = 0;
    private float AudioStartTime = -114514f;

    public List<TapDrop> TapPool = new List<TapDrop>();

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        print(GameManager.Instance.selectedIndex);
        song = GameManager.Instance.songList[GameManager.Instance.selectedIndex];
    }
    void Start()
    {

        audioSample = AudioManager.Instance.LoadMusic(song.TrackPath);

        Chart = new SimaiProcess(song.InnerMaidata[GameManager.Instance.selectedDiff]);
        

        print(Chart.notelist.Count);
        StartCoroutine(DelayPlay());
    }

    IEnumerator DelayPlay()
    {
        var settings = SettingManager.Instance.SettingFile;

        yield return new WaitForEndOfFrame();
        var BGManager = GameObject.Find("Background").GetComponent<BGManager>();
        if (song.VideoPath != null)
        {
            BGManager.SetBackgroundMovie(song.VideoPath);
        }
        else
        {
            BGManager.SetBackgroundPic(song.SongCover);
        }
        BGManager.SetBackgroundDim(settings.BackgroundDim);

        yield return new WaitForEndOfFrame();
        noteLoader = GameObject.Find("NoteLoader").GetComponent<NoteLoader>();
        noteLoader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(settings.TapSpeed + 0.9975f, -0.985558604f)));
        noteLoader.touchSpeed = settings.TouchSpeed;
        noteLoader.LoadNotes(Chart);


        yield return new WaitForEndOfFrame();

        GameObject.Find("Notes").GetComponent<NoteManager>().Refresh();
        yield return new WaitForSeconds(2);
        audioSample.Play();
        AudioStartTime = Time.unscaledTime + (float)audioSample.GetCurrentTime();
    }

    private void OnDestroy()
    {
        audioSample = null;
    }
    int i = 0;
    // Update is called once per frame
    void Update()
    {
        if (audioSample == null) return;
        //Do not use this!!!! This have connection with sample batch size
        //AudioTime = (float)audioSample.GetCurrentTime();
        if(AudioStartTime!=-114514f)
            AudioTime = Time.unscaledTime - AudioStartTime;
        var delta = Math.Abs(AudioTime - (Chart.notelist[i].time + song.First));
        if( delta < 0.01) {
            AudioManager.Instance.PlaySFX("answer.wav");
            //print(Chart.notelist[i].time);
            i++;
        }
        //TODO: Render notes
    }

    public float GetFrame()
    {
        var _audioTime = AudioTime * 1000;

        return _audioTime / 16.6667f;
    }

    public void EndGame(float acc)
    {
        GameManager.Instance.lastGameResult = acc;
        print("GameResult: "+acc);
        StartCoroutine(delayEndGame());
    }

    IEnumerator delayEndGame()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(3);
    }

}
