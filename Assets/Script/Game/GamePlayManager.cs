using MajSimaiDecode;
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
    SimaiProcess Chart;

    SongDetail song;

    public GameObject notesParent;
    public GameObject tapPrefab;

    public float noteSpeed = 9f;
    public float touchSpeed = 7.5f;

    public double GetAudioTime() => audioSample.GetCurrentTime();

    private int noteSortOrder = 0;

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
        BackgroundDisplayer.SetBackground(song.SongCover);

        audioSample = AudioManager.Instance.LoadMusic(song.TrackPath);
        
        Chart = new SimaiProcess(song.InnerMaidata[0]);
        //TODO: Load objects and store them into a pool
        foreach (var timing in Chart.notelist)
        {
            for (var i = 0; i < timing.noteList.Count; i++)
            {
                var note = timing.noteList[i];
                if (note.noteType == SimaiNoteType.Tap)
                {
                    var GOnote = Instantiate(tapPrefab, notesParent.transform);
                    var NoteComp = GOnote.GetComponent<TapDrop>();

                    // note的图层顺序
                    NoteComp.noteSortOrder = noteSortOrder;
                    noteSortOrder -= NOTE_LAYER_COUNT[note.noteType];

                    if (note.isForceStar)
                    {
                        /*NDCompo.normalSpr = customSkin.Star;
                        NDCompo.eachSpr = customSkin.Star_Each;
                        NDCompo.breakSpr = customSkin.Star_Break;
                        NDCompo.exSpr = customSkin.Star_Ex;
                        NDCompo.tapLine = starLine;*/
                        NoteComp.isFakeStarRotate = note.isFakeRotate;
                    }
                    else
                    {
                        //自定义note样式
                        /*NDCompo.normalSpr = customSkin.Tap;
                        NDCompo.breakSpr = customSkin.Tap_Break;
                        NDCompo.eachSpr = customSkin.Tap_Each;
                        NDCompo.exSpr = customSkin.Tap_Ex;*/
                    }

                    //NoteComp.BreakShine = BreakShine;

                    if (timing.noteList.Count > 1) NoteComp.isEach = true;
                    NoteComp.isBreak = note.isBreak;
                    NoteComp.isEX = note.isEx;
                    NoteComp.time = (float)timing.time;
                    NoteComp.startPosition = note.startPosition;
                    NoteComp.speed = noteSpeed * timing.HSpeed;
                    TapPool.Add(NoteComp);
                }
            }
        }


        print(Chart.notelist.Count);
        audioSample.Play();
        IOManager.Instance.OnButtonDown += IO_OnButtonDown;
    }

    private void OnDestroy()
    {
        audioSample = null;
    }
    int i = 0;
    // Update is called once per frame
    void Update()
    {
        var delta = Math.Abs(audioSample.GetCurrentTime() - (Chart.notelist[i].time + song.First));
        if( delta < 0.01) {
            AudioManager.Instance.PlaySFX("answer.wav");
            //print(Chart.notelist[i].time);
            i++;
        }
        var nowTime = GetAudioTime();
        var missed = TapPool.FindAll(o => nowTime - o.time > 0.2f);
        for (int i = 0; i < missed.Count; i++)
        {
            missed[i].NotifyJudgeResult("miss");
            TapPool.Remove(missed[i]);
        }
        //TODO: Render notes
    }

    //TODO: Trigger notes from event

    private void IO_OnButtonDown(object sender, ButtonEventArgs e)
    {
        try
        {
            var onButtonList = TapPool.FindAll(o => o.startPosition == e.ButtonIndex);
            if (onButtonList == null) return;
            
            var nowTime = GetAudioTime();
            
            var onButton = onButtonList.First();
            var delta = Math.Abs(onButton.time - nowTime);
            if (delta < 0.08f)
            {
                AudioManager.Instance.PlaySFX("judge.wav");
                onButton.NotifyJudgeResult("perfect");
                TapPool.Remove(onButton);
                return;
            }
            else if (delta < 0.2f)
            {
                AudioManager.Instance.PlaySFX("judge.wav");
                onButton.NotifyJudgeResult("good");
                TapPool.Remove(onButton);
                return;
            }
        }catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private static readonly Dictionary<SimaiNoteType, int> NOTE_LAYER_COUNT = new Dictionary<SimaiNoteType, int>()
    {
        {SimaiNoteType.Tap, 2 },
        {SimaiNoteType.Hold, 3 },
        {SimaiNoteType.Slide, 2 },
        {SimaiNoteType.Touch, 7 },
        {SimaiNoteType.TouchHold, 6 },
    };
}
