using Assets.Script.Game;
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
        var settings = SettingManager.Instance.SettingFile;
        BackgroundDisplayer.SetBackgroundDim(settings.BackgroundDim);

        noteSpeed = (float)(107.25 / (71.4184491 * Mathf.Pow(settings.TapSpeed + 0.9975f, -0.985558604f)));
        touchSpeed = settings.TouchSpeed;

        audioSample = AudioManager.Instance.LoadMusic(song.TrackPath);
        
        Chart = new SimaiProcess(song.InnerMaidata[GameManager.Instance.selectedDiff]);
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
                    NoteComp.time = (float)(timing.time + song.First);
                    NoteComp.startPosition = note.startPosition;
                    NoteComp.speed = noteSpeed * timing.HSpeed;
                    TapPool.Add(NoteComp);
                    GameScoreManager.Instance.tapSum++;
                }
            }
        }


        print(Chart.notelist.Count);
        IOManager.Instance.OnButtonDown += IO_OnButtonDown;
        IOManager.Instance.OnTouchAreaDown += IO_OnTouchAreaDown;
        StartCoroutine(DelayPlay());
    }

    IEnumerator DelayPlay()
    {
        yield return new WaitForSeconds(2);
        audioSample.Play();
    }

    private void OnDestroy()
    {
        IOManager.Instance.OnButtonDown -= IO_OnButtonDown;
        IOManager.Instance.OnTouchAreaDown -= IO_OnTouchAreaDown;
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
        var missed = TapPool.FindAll(o => nowTime - o.time > (9 * 0.0167f));
        for (int i = 0; i < missed.Count; i++)
        {
            missed[i].NotifyJudgeResult(DataTypes.TapJudgeType.Miss);
            TapPool.Remove(missed[i]);
        }
        //TODO: Render notes
    }

    //TODO: Trigger notes from event
    private void IO_OnTouchAreaDown(object sender, TouchAreaEventArgs e)
    {
        try
        {
            if (e.AreaName.Contains("A"))
            {
                JudgeTap(e.AreaIndex+1);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    private void IO_OnButtonDown(object sender, ButtonEventArgs e)
    {
        try
        {
            JudgeTap(e.ButtonIndex);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void JudgeTap(int buttonIndex)
    {
        var onButtonList = TapPool.FindAll(o => o.startPosition == buttonIndex);
        if (onButtonList == null || onButtonList.Count == 0) return;

        var nowTime = GetAudioTime();

        var onButton = onButtonList.First();
        var delta = onButton.time - nowTime;
        //pos:late neg:early
        if (delta < 0.0167f && delta > -0.0167f)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.CPerfect);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.CPerfect);
            return;
        }
        else if (delta < (3 * 0.0167f) && delta > 0)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.FastPerfect);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.FastPerfect);
            return;
        }
        else if (delta < (6 * 0.0167f) && delta > 0)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.FastGreat);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.FastGreat);
            return;
        }
        else if (delta < (9 * 0.0167f) && delta > 0)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.FastGood);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.FastGood);
            return;
        }
        else if (delta > -(3 * 0.0167f) && delta < 0)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.LatePerfect);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.LatePerfect);
            return;
        }
        else if (delta > -(6 * 0.0167f) && delta < 0)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.LateGreat);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.LateGreat);
            return;
        }
        else if (delta > -(9 * 0.0167f) && delta < 0)
        {
            AudioManager.Instance.PlaySFX("judge.wav");
            onButton.NotifyJudgeResult(DataTypes.TapJudgeType.LateGood);
            TapPool.Remove(onButton);
            GameScoreManager.Instance.UpdateTapResult(DataTypes.TapJudgeType.LateGood);
            return;
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
