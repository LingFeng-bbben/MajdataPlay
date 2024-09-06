using MajdataPlay.Game.Notes;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajSimaiDecode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#nullable enable
public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance;
    public (float,float) BreakParams => (0.95f + Math.Max(Mathf.Sin(GetFrame() * 0.20f) * 0.8f, 0), 1f + Math.Min(Mathf.Sin(GetFrame() * 0.2f) * -0.15f, 0));

    AudioSampleWrap audioSample;
    SimaiProcess Chart;
    SongDetail song;
    GameManager settingManager => GameManager.Instance;

    NoteLoader noteLoader;

    Text ErrorText;

    public GameObject notesParent;
    public GameObject tapPrefab;

    public float noteSpeed = 9f;
    public float touchSpeed = 7.5f;

    public float AudioTime = -114514f;
    public bool isStart => audioSample.GetPlayState();
    bool isLoading = false;
    public float CurrentSpeed = 1f;

    private float AudioStartTime = -114514f;
    List<AnwserSoundPoint> AnwserSoundList = new List<AnwserSoundPoint>();
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        print(GameManager.Instance.SelectedIndex);
        song = GameManager.Instance.SongList[GameManager.Instance.SelectedIndex];
    }

    private void OnPauseButton(object sender,InputEventArgs e)
    {
        if (e.IsButton && e.IsClick && e.Type == SensorType.P1) {
            print("Pause!!");
            BackToList();
        }
    }
    
    void Start()
    {
        InputManager.Instance.BindAnyArea(OnPauseButton);
        audioSample = AudioManager.Instance.LoadMusic(song.TrackPath);
        audioSample.SetVolume(settingManager.Setting.Audio.Volume.BGM);
        ErrorText = GameObject.Find("ErrText").GetComponent<Text>();
        LightManager.Instance.SetAllLight(Color.white);
        try
        {
            //TODO: should load from disk instead of memory
            var maidata = song.InnerMaidata[(int)GameManager.Instance.SelectedDiff];
            if (maidata == "" || maidata == null) {
                BackToList();
                return;
            }
                
            Chart = new SimaiProcess(maidata);
            if (Chart.notelist.Count == 0)
            {
                BackToList();
                return;
            }
            else
            {
                StartCoroutine(DelayPlay());
            }

            //Generate ClockSounds
            var countnum = (song.ClockCount == null ? 4 : song.ClockCount);
            var firstBpm = Chart.notelist.FirstOrDefault().currentBpm;
            var interval = 60 / firstBpm;
            if(Chart.notelist.Any(o=>o.time<countnum*interval)) {
                //if there is something in first measure, we add clock before the bgm
                for (int i = 0; i < countnum; i++)
                {
                    AnwserSoundList.Add(new AnwserSoundPoint()
                    {
                        time = -(i + 1) * interval,
                        isClock = true,
                        isPlayed = false
                    });
                }
            }
            else
            {
                //if nothing there, we can add it with bgm
                for (int i = 0; i < countnum; i++)
                {
                    AnwserSoundList.Add(new AnwserSoundPoint()
                    {
                        time = i * interval,
                        isClock = true,
                        isPlayed = false
                    });
                }
            }

            
            //Generate AnwserSounds
            foreach (var timingPoint in Chart.notelist)
            {
                if (timingPoint.noteList.All(o => o.isSlideNoHead)) continue;

                AnwserSoundList.Add(new AnwserSoundPoint()
                {
                    time = timingPoint.time,
                    isClock = false,
                    isPlayed = false
                });
                var holds = timingPoint.noteList.FindAll(o => o.noteType == SimaiNoteType.Hold || o.noteType == SimaiNoteType.TouchHold);
                if (holds.Count == 0) continue;
                foreach (var hold in holds)
                {
                    var newtime = timingPoint.time + hold.holdTime;
                    if (!Chart.notelist.Any(o => Math.Abs(o.time - newtime) < 0.001) &&
                        !AnwserSoundList.Any(o => Math.Abs(o.time - newtime) < 0.001)
                        )
                        AnwserSoundList.Add(new AnwserSoundPoint()
                        {
                            time = newtime,
                            isClock = false,
                            isPlayed = false
                        });
                }
            }
            AnwserSoundList = AnwserSoundList.OrderBy(o=>o.time).ToList();
        }
        catch (Exception ex)
        {
            ErrorText.text = "加载note时出错了哟\n" + ex.Message;
            Debug.LogError(ex);
        }
    }

    IEnumerator DelayPlay()
    {
        isLoading = true;
        var firstBpm = Chart.notelist.First().currentBpm;
        
        var settings = settingManager.Setting;

        AudioTime = -5f;
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
        BGManager.SetBackgroundDim(settings.Game.BackgroundDim);

        yield return new WaitForEndOfFrame();
        noteLoader = GameObject.Find("NoteLoader").GetComponent<NoteLoader>();
        noteLoader.noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(settings.Game.TapSpeed + 0.9975f, -0.985558604f)));
        noteLoader.touchSpeed = settings.Game.TouchSpeed;
        try
        {
            noteLoader.LoadNotes(Chart);
        }
        catch (Exception ex)
        {
            ErrorText.text = "加载note时出错了哟\n" + ex.Message;
            Debug.LogError(ex);
            StopAllCoroutines();
        }

        GameObject.Find("Notes").GetComponent<NoteManager>().Refresh();
        AudioStartTime = Time.unscaledTime + (float)audioSample.GetCurrentTime()+5f;
        isLoading = false;
        while (Time.unscaledTime - AudioStartTime < 0)
            yield return new WaitForEndOfFrame();

        
        AudioStartTime = Time.unscaledTime;
        audioSample.Play();
        //AudioStartTime = Time.unscaledTime;
        

    }

    private void OnDestroy()
    {
        print("GPManagerDestroy");
        audioSample = null;
        GC.Collect();
    }
    int i = 0;
    // Update is called once per frame
    void Update()
    {
        if (audioSample == null)
            return;
        else if (isLoading)
            return;
        //Do not use this!!!! This have connection with sample batch size
        //AudioTime = (float)audioSample.GetCurrentTime();
        if (AudioStartTime == -114514f) 
            return;

        var chartOffset = (float)song.First + settingManager.Setting.Judge.AudioOffset;
        AudioTime = Time.unscaledTime - AudioStartTime - chartOffset;

        var realTimeDifference = (float)audioSample.GetCurrentTime() - (Time.unscaledTime - AudioStartTime);
        if (Math.Abs(realTimeDifference) > 0.04f && AudioTime > 0)
        {
            ErrorText.text = "音频错位了哟\n" + realTimeDifference;
        }
        else if (Math.Abs(realTimeDifference) > 0.01f && AudioTime > 0 && GameManager.Instance.Setting.Debug.TryFixAudioSync)
        {
            ErrorText.text = "修正音频哟\n" + realTimeDifference;
            AudioStartTime -= realTimeDifference * 0.8f;
        }

        if (i >= AnwserSoundList.Count)
            return;
       



        var noteToPlay = AnwserSoundList[i].time;
        var delta = AudioTime - (noteToPlay);

        if (delta > 0)
        {
            if(AnwserSoundList[i].isClock)
                AudioManager.Instance.PlaySFX("clock.wav");
            else
                AudioManager.Instance.PlaySFX("answer.wav");
            AnwserSoundList[i].isPlayed = true;
            i++;
        }
    }

    public float GetFrame()
    {
        var _audioTime = AudioTime * 1000;

        return _audioTime / 16.6667f;
    }

    public void BackToList()
    {
        StopAllCoroutines();
        audioSample.Pause();
        audioSample = null;
        //AudioManager.Instance.UnLoadMusic();
        InputManager.Instance.UnbindAnyArea(OnPauseButton);
        StartCoroutine(delayBackToList());

    }
    IEnumerator delayBackToList()
    {
        yield return new WaitForEndOfFrame();
        GameObject.Find("Notes").GetComponent<NoteManager>().DestroyAllNotes();
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(1);
    }


    public void EndGame(float acc)
    {
        print("GameResult: "+acc);
        var objectCounter = FindFirstObjectByType<ObjectCounter?>();
        if(objectCounter != null)
            GameManager.LastGameResult = objectCounter.GetPlayRecord(song,GameManager.Instance.SelectedDiff);
        StartCoroutine(delayEndGame());
    }

    IEnumerator delayEndGame()
    {
        yield return new WaitForSeconds(2f);
        audioSample.Pause();
        audioSample = null;
        InputManager.Instance.UnbindAnyArea(OnPauseButton);
        SceneManager.LoadScene(3);
    }

    class AnwserSoundPoint
    {
        public double time;
        public bool isClock;
        public bool isPlayed;
    }
}
