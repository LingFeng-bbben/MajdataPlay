using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MajSimai;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Scenes.Game.Notes.Slide;
using MajdataPlay.Scenes.Game.Notes.Slide.Utils;
using MajdataPlay.Scenes.Game.Notes.Touch;
using MajdataPlay.Scenes.Game.Notes.Behaviours;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Notes;
using System.Buffers;
using System.Threading;
using MajdataPlay.Settings;

namespace MajdataPlay.Scenes.Game
{
#nullable enable
    public class NoteLoader : MonoBehaviour
    {
        public double Progress { get; set; } = 0;
        public float NoteSpeed { get; set; } = 7f;
        public int ChartRotation { get; set; } = 0;
        public float TouchSpeed
        {
            get => _touchSpeed;
            set => _touchSpeed = Math.Abs(value);
        }
        public long NoteCount { get; private set; } = 0;

        public GameObject tapPrefab;
        public GameObject holdPrefab;
        public GameObject starPrefab;
        public GameObject touchHoldPrefab;
        public GameObject touchPrefab;
        public GameObject eachLine;
        public GameObject starLine;
        public GameObject notes;
        public GameObject star_slidePrefab;
        public GameObject[] slidePrefab;
        public Material breakMaterial;
        public RuntimeAnimatorController BreakShine;
        public RuntimeAnimatorController JudgeBreakShine;
        public RuntimeAnimatorController HoldShine;

        bool _isSlideNoHead = false;
        bool _isSlideNoTrack = false;

        float _touchSpeed = 7.5f;
        long _noteCount = 0;
        int _slideLayer = -1;
        int _noteSortOrder = short.MaxValue;
        int _touchSortOrder = short.MaxValue;
        int _slideIndex = 0;

        List<SlideQueueInfo> _slideQueueInfos = new();
        NoteManager _noteManager;
        Dictionary<int, int> _noteIndex = new();
        Dictionary<SensorArea, int> _touchIndex = new();

        SlideUpdater _slideUpdater;
        GamePlayManager? _gpManager;
        ObjectCounter _objectCounter;
        NotePoolManager _poolManager;

        static readonly bool USERSETTING_NOTE_FOLDING = MajEnv.UserSettings?.Debug.NoteFolding ?? true;

        public static readonly IReadOnlyDictionary<SimaiNoteType, int> NOTE_LAYER_COUNT = new Dictionary<SimaiNoteType, int>()
        {
            {SimaiNoteType.Tap, 2 },
            {SimaiNoteType.Hold, 3 },
            {SimaiNoteType.Slide, 2 },
            {SimaiNoteType.Touch, 6 },
            {SimaiNoteType.TouchHold, 6 },
        };
        public static readonly IReadOnlyDictionary<string, int> SLIDE_PREFAB_MAP = new Dictionary<string, int>()
        {
            {"line3", 0 },
            {"line4", 1 },
            {"line5", 2 },
            {"line6", 3 },
            {"line7", 4 },
            {"circle1", 5 },
            {"circle2", 6 },
            {"circle3", 7 },
            {"circle4", 8 },
            {"circle5", 9 },
            {"circle6", 10 },
            {"circle7", 11 },
            {"circle8", 12 },
            {"v1", 41 },
            {"v2", 13 },
            {"v3", 14 },
            {"v4", 15 },
            {"v6", 16 },
            {"v7", 17 },
            {"v8", 18 },
            {"ppqq1", 19 },
            {"ppqq2", 20 },
            {"ppqq3", 21 },
            {"ppqq4", 22 },
            {"ppqq5", 23 },
            {"ppqq6", 24 },
            {"ppqq7", 25 },
            {"ppqq8", 26 },
            {"pq1", 27 },
            {"pq2", 28 },
            {"pq3", 29 },
            {"pq4", 30 },
            {"pq5", 31 },
            {"pq6", 32 },
            {"pq7", 33 },
            {"pq8", 34 },
            {"s", 35 },
            {"wifi", 36 },
            {"L2", 37 },
            {"L3", 38 },
            {"L4", 39 },
            {"L5", 40 },
        };

        public static readonly IReadOnlyDictionary<SensorArea, SensorArea[]> TOUCH_GROUPS = new Dictionary<SensorArea, SensorArea[]>()
        {
            { SensorArea.A1, new SensorArea[]{ SensorArea.D1, SensorArea.D2, SensorArea.E1, SensorArea.E2, SensorArea.B1 } },
            { SensorArea.A2, new SensorArea[]{ SensorArea.D2, SensorArea.D3, SensorArea.E2, SensorArea.E3, SensorArea.B2 } },
            { SensorArea.A3, new SensorArea[]{ SensorArea.D3, SensorArea.D4, SensorArea.E3, SensorArea.E4, SensorArea.B3 } },
            { SensorArea.A4, new SensorArea[]{ SensorArea.D4, SensorArea.D5, SensorArea.E4, SensorArea.E5, SensorArea.B4 } },
            { SensorArea.A5, new SensorArea[]{ SensorArea.D5, SensorArea.D6, SensorArea.E5, SensorArea.E6, SensorArea.B5 } },
            { SensorArea.A6, new SensorArea[]{ SensorArea.D6, SensorArea.D7, SensorArea.E6, SensorArea.E7, SensorArea.B6 } },
            { SensorArea.A7, new SensorArea[]{ SensorArea.D7, SensorArea.D8, SensorArea.E7, SensorArea.E8, SensorArea.B7 } },
            { SensorArea.A8, new SensorArea[]{ SensorArea.D8, SensorArea.D1, SensorArea.E8, SensorArea.E1, SensorArea.B8 } },

            { SensorArea.D1, new SensorArea[]{ SensorArea.A1, SensorArea.A8, SensorArea.E1 } },
            { SensorArea.D2, new SensorArea[]{ SensorArea.A2, SensorArea.A1, SensorArea.E2 } },
            { SensorArea.D3, new SensorArea[]{ SensorArea.A3, SensorArea.A2, SensorArea.E3 } },
            { SensorArea.D4, new SensorArea[]{ SensorArea.A4, SensorArea.A3, SensorArea.E4 } },
            { SensorArea.D5, new SensorArea[]{ SensorArea.A5, SensorArea.A4, SensorArea.E5 } },
            { SensorArea.D6, new SensorArea[]{ SensorArea.A6, SensorArea.A5, SensorArea.E6 } },
            { SensorArea.D7, new SensorArea[]{ SensorArea.A7, SensorArea.A6, SensorArea.E7 } },
            { SensorArea.D8, new SensorArea[]{ SensorArea.A8, SensorArea.A7, SensorArea.E8 } },

            { SensorArea.E1, new SensorArea[]{ SensorArea.D1, SensorArea.A1, SensorArea.A8, SensorArea.B1, SensorArea.B8 } },
            { SensorArea.E2, new SensorArea[]{ SensorArea.D2, SensorArea.A2, SensorArea.A1, SensorArea.B2, SensorArea.B1 } },
            { SensorArea.E3, new SensorArea[]{ SensorArea.D3, SensorArea.A3, SensorArea.A2, SensorArea.B3, SensorArea.B2 } },
            { SensorArea.E4, new SensorArea[]{ SensorArea.D4, SensorArea.A4, SensorArea.A3, SensorArea.B4, SensorArea.B3 } },
            { SensorArea.E5, new SensorArea[]{ SensorArea.D5, SensorArea.A5, SensorArea.A4, SensorArea.B5, SensorArea.B4 } },
            { SensorArea.E6, new SensorArea[]{ SensorArea.D6, SensorArea.A6, SensorArea.A5, SensorArea.B6, SensorArea.B5 } },
            { SensorArea.E7, new SensorArea[]{ SensorArea.D7, SensorArea.A7, SensorArea.A6, SensorArea.B7, SensorArea.B6 } },
            { SensorArea.E8, new SensorArea[]{ SensorArea.D8, SensorArea.A8, SensorArea.A7, SensorArea.B8, SensorArea.B7 } },

            { SensorArea.B1, new SensorArea[]{ SensorArea.E1, SensorArea.E2, SensorArea.B8, SensorArea.B2, SensorArea.A1, SensorArea.C } },
            { SensorArea.B2, new SensorArea[]{ SensorArea.E2, SensorArea.E3, SensorArea.B1, SensorArea.B3, SensorArea.A2, SensorArea.C } },
            { SensorArea.B3, new SensorArea[]{ SensorArea.E3, SensorArea.E4, SensorArea.B2, SensorArea.B4, SensorArea.A3, SensorArea.C } },
            { SensorArea.B4, new SensorArea[]{ SensorArea.E4, SensorArea.E5, SensorArea.B3, SensorArea.B5, SensorArea.A4, SensorArea.C } },
            { SensorArea.B5, new SensorArea[]{ SensorArea.E5, SensorArea.E6, SensorArea.B4, SensorArea.B6, SensorArea.A5, SensorArea.C } },
            { SensorArea.B6, new SensorArea[]{ SensorArea.E6, SensorArea.E7, SensorArea.B5, SensorArea.B7, SensorArea.A6, SensorArea.C } },
            { SensorArea.B7, new SensorArea[]{ SensorArea.E7, SensorArea.E8, SensorArea.B6, SensorArea.B8, SensorArea.A7, SensorArea.C } },
            { SensorArea.B8, new SensorArea[]{ SensorArea.E8, SensorArea.E1, SensorArea.B7, SensorArea.B1, SensorArea.A8, SensorArea.C } },

            { SensorArea.C, new SensorArea[]{ SensorArea.B1, SensorArea.B2, SensorArea.B3, SensorArea.B4, SensorArea.B5, SensorArea.B6, SensorArea.B7, SensorArea.B8} },
        };

        public static readonly IReadOnlyDictionary<string, List<int>> SLIDE_AREA_STEP_MAP = new Dictionary<string, List<int>>()
        {
            {"line3", new List<int>(){ 0, 2, 8, 13 } },
            {"line4", new List<int>(){ 0, 3, 8, 12, 18 } },
            {"line5", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"line6", new List<int>(){ 0, 3, 8, 12, 18 } },
            {"line7", new List<int>(){ 0, 2, 8, 13 } },
            {"circle1", new List<int>(){ 0, 3, 11, 19, 27, 35, 43, 50, 58, 63 } },
            {"circle2", new List<int>(){ 0, 3, 7 } },
            {"circle3", new List<int>(){ 0, 3, 11, 15 } },
            {"circle4", new List<int>(){ 0, 3, 11, 19, 23 } },
            {"circle5", new List<int>(){ 0, 3, 11, 19, 27, 31 } },
            {"circle6", new List<int>(){ 0, 3, 11, 19, 27, 35, 39 } },
            {"circle7", new List<int>(){ 0, 3, 11, 19, 27, 35, 43, 47 } },
            {"circle8", new List<int>(){ 0, 3, 11, 19, 27, 35, 43, 50, 55 } },
            {"v1", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"v2", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"v3", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"v4", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"v6", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"v7", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"v8", new List<int>(){ 0, 3, 6, 11, 15, 19 } },
            {"ppqq1", new List<int>(){ 0, 3, 7, 13, 17, 26, 32, 35 } },
            {"ppqq2", new List<int>(){ 0, 3, 7, 12, 16, 25, 28 } },
            {"ppqq3", new List<int>(){ 0, 3, 6, 12, 15, 22 } },
            {"ppqq4", new List<int>(){ 0, 3, 7, 12, 16, 25, 29, 35, 40, 44, 49 } },
            {"ppqq5", new List<int>(){ 0, 3, 7, 12, 16, 25, 29, 35, 40, 44, 49 } },
            {"ppqq6", new List<int>(){ 0, 3, 7, 12, 16, 25, 28, 34, 38, 41, 48 } },
            {"ppqq7", new List<int>(){ 0, 3, 7, 13, 17, 27, 31, 37, 41, 46 } },
            {"ppqq8", new List<int>(){ 0, 3, 7, 12, 16, 25, 29, 35, 41 } },
            {"pq1", new List<int>(){ 0, 3, 8, 11, 14, 17, 21, 24, 27, 33 } },
            {"pq2", new List<int>(){ 0, 3, 8, 11, 14, 18, 21, 24, 30 } },
            {"pq3", new List<int>(){ 0, 3, 9, 12, 16, 19, 23, 27 } },
            {"pq4", new List<int>(){ 0, 3, 9, 13, 16, 20, 24 } },
            {"pq5", new List<int>(){ 0, 3, 9, 13, 17, 21 } },
            {"pq6", new List<int>(){ 0, 3, 8, 11, 15, 18, 21, 25, 28, 31, 35, 38, 42 } },
            {"pq7", new List<int>(){ 0, 3, 8, 12, 15, 18, 22, 25, 28, 32, 35, 39 } },
            {"pq8", new List<int>(){ 0, 3, 8, 11, 14, 17, 21, 24, 27, 30, 36 } },
            {"s", new List<int>(){ 0, 3, 8, 11, 17, 21, 24, 30 } },
            {"wifi", new List<int>(){ 0, 1, 4, 6, 9} },
            {"L2", new List<int>(){ 0, 2, 7, 15, 21, 26, 32 } },
            {"L3", new List<int>(){ 0, 2, 8, 17, 20, 26, 29, 34 } },
            {"L4", new List<int>(){ 0, 2, 8, 17, 22, 26, 32 } },
            {"L5", new List<int>(){ 0, 2, 8, 16, 22, 28 } },
        };
        readonly IReadOnlyDictionary<int, int> _buttonRingMappingTable;
        readonly IReadOnlyDictionary<SensorArea, SensorArea> _touchPanelMappingTable;
        NoteLoader()
        {
            (_buttonRingMappingTable, _touchPanelMappingTable) = NoteCreateHelper.GenerateMappingTable();
        }

        void Awake()
        {
            Majdata<NoteLoader>.Instance = this;
        }
        void OnDestroy()
        {
            Majdata<NoteLoader>.Free();
        }
        private void Start()
        {
            _objectCounter = Majdata<ObjectCounter>.Instance!;
            _noteManager = Majdata<NoteManager>.Instance!;
            _poolManager = Majdata<NotePoolManager>.Instance!;
            _gpManager = Majdata<GamePlayManager>.Instance;
            _slideUpdater = Majdata<SlideUpdater>.Instance!;
            _isSlideNoHead = Majdata<INoteController>.Instance?.ModInfo.SlideNoHead ?? false;
            _isSlideNoTrack = Majdata<INoteController>.Instance?.ModInfo.SlideNoTrack ?? false;
        }
        internal void Clear()
        {
            _noteManager.ResetCounter();
            _noteIndex.Clear();
            _touchIndex.Clear();
            _slideQueueInfos.Clear();
        }
        internal async UniTask LoadNotesIntoPoolAsync(SimaiChart maiChart, CancellationToken token = default)
        {
            List<Task> touchTasks = new();

            _noteManager.ResetCounter();
            _noteIndex.Clear();
            _touchIndex.Clear();

            for (int i = 1; i < 9; i++)
            {
                _noteIndex.Add(i, 0);
            }
            for (int i = 0; i < 33; i++)
            {
                _touchIndex.Add((SensorArea)i, 0);
            }


            await _objectCounter.CountNoteSumAsync(maiChart);

            NoteCount = _objectCounter.TapSum +
                      _objectCounter.HoldSum +
                      _objectCounter.TouchSum +
                      _objectCounter.BreakSum +
                      _objectCounter.SlideSum;

            if (maiChart.NoteTimings.Length != 0)
            {

                var lastNoteTime = maiChart.NoteTimings.Last().Timing;

                foreach (var timing in maiChart.NoteTimings)
                {
                    List<NotePoolingInfo?> eachNotes = new();
                    List<ITouchGroupInfoProvider> members = new();
                    var foldedNotes = NoteCreateHelper.NoteFolding(timing.Notes);
                    foreach (var (i, note) in foldedNotes.WithIndex())
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            switch (note.Type)
                            {
                                case SimaiNoteType.Tap:
                                    {
                                        var obj = CreateTap(note, timing);
                                        _poolManager.AddTap(obj);
                                        eachNotes.Add(obj);
                                    }
                                    break;
                                case SimaiNoteType.Hold:
                                    {
                                        var obj = CreateHold(note, timing);
                                        _poolManager.AddHold(obj);
                                        eachNotes.Add(obj);
                                    }
                                    break;
                                case SimaiNoteType.TouchHold:
                                    _poolManager.AddTouchHold(CreateTouchHold(note, timing, members));
                                    break;
                                case SimaiNoteType.Touch:
                                    _poolManager.AddTouch(CreateTouch(note, timing, members));
                                    break;
                                case SimaiNoteType.Slide:
                                    var foldedSlide = note as FoldedSimaiNote;
                                    foldedSlide ??= new FoldedSimaiNote()
                                    {
                                        Type = note.Type,
                                        StartPosition = note.StartPosition,
                                        HoldTime = note.HoldTime,
                                        IsBreak = note.IsBreak,
                                        IsEx = note.IsEx,
                                        IsFakeRotate = note.IsFakeRotate,
                                        IsForceStar = note.IsForceStar,
                                        IsHanabi = note.IsHanabi,
                                        IsSlideBreak = note.IsSlideBreak,
                                        IsSlideNoHead = note.IsSlideNoHead,
                                        RawContent = note.RawContent,
                                        SlideStartTime = note.SlideStartTime,
                                        SlideTime = note.SlideTime,
                                        TouchArea = note.TouchArea,
                                        Count = 1
                                    };
                                    CreateSlideGroup(timing, foldedSlide, eachNotes); // 星星组
                                    _noteCount += foldedSlide.Count - 1;
                                    break;
                            }
                            _noteCount++;
                            Progress = (double)_noteCount / NoteCount;
                            if (_noteCount % 100 == 0)
                            {
                                await UniTask.DelayFrame(3);
                            }
                        }
                        catch (InvalidSimaiSyntaxException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            MajDebug.LogException(e);
                            throw;
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    if (members.Count != 0)
                    {
                        touchTasks.Add(AllocTouchGroup(members));
                    }
                    eachNotes = eachNotes.FindAll(x => x is not null);
                    if (eachNotes.Count > 1) //有多个非touchnote
                    {
                        var eachLinePoolingInfo = CreateEachLine(timing, eachNotes[0]!, eachNotes[1]!);
                        if (eachLinePoolingInfo is not null)
                        {
                            _poolManager.AddEachLine(eachLinePoolingInfo);
                        }
                    }
                }

                var allTask = Task.WhenAll(touchTasks);
                while (!allTask.IsCompleted)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield();
                    if (allTask.IsFaulted)
                    {
                        throw allTask.Exception.GetBaseException();
                    }
                }
            }
            token.ThrowIfCancellationRequested();
            _slideUpdater.AddSlideQueueInfos(_slideQueueInfos.ToArray());
            _poolManager.Initialize();
        }
        EachLinePoolingInfo? CreateEachLine(SimaiTimingPoint timing, NotePoolingInfo noteA, NotePoolingInfo noteB)
        {
            try
            {
                var startPos = noteA.StartPos;
                var endPos = noteB.StartPos;
                endPos = endPos - startPos;
                if (endPos == 0)
                    return null;
                var time = (float)timing.Timing;
                var speed = NoteSpeed * timing.HSpeed;
                var scaleRate = MajInstances.Settings.Debug.NoteAppearRate;
                var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (speed * scaleRate);
                var appearTiming = time + appearDiff;

                endPos = endPos < 0 ? endPos + 8 : endPos;
                endPos = endPos > 8 ? endPos - 8 : endPos;
                endPos++;

                if (endPos > 4)
                {
                    startPos = noteB.StartPos;
                    endPos = noteA.StartPos;
                    endPos = endPos - startPos;
                    endPos = endPos < 0 ? endPos + 8 : endPos;
                    endPos = endPos > 8 ? endPos - 8 : endPos;
                    endPos++;
                }

                var startPosition = startPos;
                var curvLength = endPos - 1;

                return new EachLinePoolingInfo()
                {
                    StartPos = startPosition,
                    Timing = time,
                    AppearTiming = appearTiming,
                    CurvLength = curvLength,
                    MemberA = noteA,
                    MemberB = noteB,
                    Speed = speed
                };
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                return null;
            }
        }
        TapPoolingInfo CreateTap(in SimaiNote note, in SimaiTimingPoint timing)
        {
            try
            {
                var startPos = note.StartPosition;
                var noteTiming = (float)timing.Timing;
                var speed = NoteSpeed * timing.HSpeed;
                var scaleRate = MajInstances.Settings.Debug.NoteAppearRate;
                var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
                var appearTiming = Math.Min(noteTiming + appearDiff, noteTiming - 0.15f);
                var sortOrder = _noteSortOrder;
                var isEach = timing.Notes.Length > 1;
                if (appearTiming < -5f && _gpManager is not null)
                    _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
                if (isEach)
                {
                    var noteCount = timing.Notes.Length;
                    var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                    if (noteCount - noHeadSlideCount == 1)
                        isEach = false;
                }

                _noteSortOrder -= NOTE_LAYER_COUNT[note.Type];
                startPos = NoteCreateHelper.Rotation(startPos, ChartRotation);
                NoteCreateHelper.SetNewPositionIfRequested(ref startPos, _buttonRingMappingTable);
                return new()
                {
                    StartPos = startPos,
                    Timing = noteTiming,
                    AppearTiming = appearTiming,
                    NoteSortOrder = sortOrder,
                    Speed = speed,
                    IsEach = isEach,
                    IsBreak = note.IsBreak,
                    IsEX = note.IsEx,
                    IsStar = note.IsForceStar,
                    RotateSpeed = note.IsFakeRotate ? -440f : 0,
                    QueueInfo = new TapQueueInfo()
                    {
                        Index = _noteIndex[startPos]++,
                        KeyIndex = startPos
                    }
                };
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                var line = timing.RawTextPositionY;
                var column = timing.RawTextPositionX;
                throw new InvalidSimaiSyntaxException(line,
                                                      column,
                                                      note.RawContent,
                                                      BuildSyntaxErrorMessage(line, column, note.RawContent));
            }
        }
        HoldPoolingInfo CreateHold(in SimaiNote note, in SimaiTimingPoint timing)
        {
            try
            {
                var startPos = note.StartPosition;
                var noteTiming = (float)timing.Timing;
                var speed = Math.Abs(NoteSpeed * timing.HSpeed);
                var scaleRate = MajInstances.Settings.Debug.NoteAppearRate;
                var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (speed * scaleRate);
                var appearTiming = Math.Min(noteTiming + appearDiff, noteTiming - 0.15f);
                var sortOrder = _noteSortOrder;
                var isEach = timing.Notes.Length > 1;
                if (appearTiming < -5f && _gpManager is not null)
                    _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
                if (isEach)
                {
                    var noteCount = timing.Notes.Length;
                    var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                    if (noteCount - noHeadSlideCount == 1)
                        isEach = false;
                }
                _noteSortOrder -= NOTE_LAYER_COUNT[note.Type];
                startPos = NoteCreateHelper.Rotation(startPos, ChartRotation);
                NoteCreateHelper.SetNewPositionIfRequested(ref startPos, _buttonRingMappingTable);
                return new()
                {
                    StartPos = startPos,
                    Timing = noteTiming,
                    LastFor = (float)note.HoldTime,
                    AppearTiming = appearTiming,
                    NoteSortOrder = sortOrder,
                    Speed = speed,
                    IsEach = isEach,
                    IsBreak = note.IsBreak,
                    IsEX = note.IsEx,
                    QueueInfo = new TapQueueInfo()
                    {
                        Index = _noteIndex[startPos]++,
                        KeyIndex = startPos
                    }
                };
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                var line = timing.RawTextPositionY;
                var column = timing.RawTextPositionX;
                throw new InvalidSimaiSyntaxException(line,
                                                      column,
                                                      note.RawContent,
                                                      BuildSyntaxErrorMessage(line, column, note.RawContent));
            }
        }
        TapPoolingInfo CreateStar(int startPos, SimaiNote note, in SimaiTimingPoint timing)
        {
            try
            {
                var noteTiming = (float)timing.Timing;
                var speed = NoteSpeed * timing.HSpeed;
                var scaleRate = MajInstances.Settings.Debug.NoteAppearRate;
                var slideFadeInTiming = (-3.926913f / speed) + MajInstances.Settings.Game.SlideFadeInOffset + (float)timing.Timing;
                var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
                var appearTiming = Math.Min(noteTiming + appearDiff, noteTiming - 0.15f);
                var sortOrder = _noteSortOrder;
                var isEach = timing.Notes.Length > 1;
                bool isDouble = false;
                TapQueueInfo? queueInfo = null;

                appearTiming = Math.Min(appearTiming, slideFadeInTiming);

                if (appearTiming < -5f && _gpManager is not null)
                    _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
                _noteSortOrder -= NOTE_LAYER_COUNT[note.Type];

                if (isEach)
                {
                    var count = timing.Notes.FindAll(
                        o => o.Type == SimaiNoteType.Slide &&
                             o.StartPosition == note.StartPosition).Length;
                    if (count > 1)
                    {
                        isDouble = true;
                        if (count == timing.Notes.Length)
                        {
                            isEach = false;
                        }
                        else
                        {
                            var noteCount = timing.Notes.Length;
                            var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                            if (noteCount - noHeadSlideCount == 1)
                            {
                                isEach = false;
                            }
                        }
                    }
                }
                if (!note.IsSlideNoHead)
                {
                    queueInfo = new TapQueueInfo()
                    {
                        Index = _noteIndex[startPos]++,
                        KeyIndex = startPos
                    };
                }

                return new()
                {
                    StartPos = startPos,
                    Timing = noteTiming,
                    AppearTiming = appearTiming,
                    NoteSortOrder = sortOrder,
                    Speed = speed,
                    IsEach = isEach,
                    IsBreak = note.IsBreak,
                    IsEX = note.IsEx,
                    IsStar = true,
                    IsDouble = isDouble,
                    RotateSpeed = -180 / (float)note.SlideTime,
                    QueueInfo = queueInfo ?? TapQueueInfo.Default
                };
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                var line = timing.RawTextPositionY;
                var column = timing.RawTextPositionX;
                throw new InvalidSimaiSyntaxException(line,
                                                      column,
                                                      note.RawContent,
                                                      BuildSyntaxErrorMessage(line, column, note.RawContent));
            }
        }
        TouchPoolingInfo CreateTouch(in SimaiNote note,
                                     in SimaiTimingPoint timing,
                                     in List<ITouchGroupInfoProvider> members)
        {
            try
            {
                note.StartPosition = NoteCreateHelper.Rotation(note.StartPosition, ChartRotation);
                var sensorPos = NoteHelper.GetSensor(note.TouchArea, note.StartPosition);
                NoteCreateHelper.SetNewPositionIfRequested(ref sensorPos, _touchPanelMappingTable);
                var queueInfo = new TouchQueueInfo()
                {
                    SensorPos = sensorPos,
                    Index = _touchIndex[sensorPos]++
                };
                var noteTiming = (float)timing.Timing;
                var areaPosition = note.TouchArea;
                var startPosition = note.StartPosition;
                var isEach = timing.Notes.Length > 1;
                var isBreak = note.IsBreak;
                var speed = TouchSpeed * Math.Abs(timing.HSpeed);
                var isFirework = note.IsHanabi;
                var noteSortOrder = _touchSortOrder;
                var moveDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
                var appearTiming = Math.Min(noteTiming - moveDuration, noteTiming - 0.15f);
                if (appearTiming < -5f && _gpManager is not null)
                    _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
                _touchSortOrder -= NOTE_LAYER_COUNT[note.Type];
                if (isEach)
                {
                    var noteCount = timing.Notes.Length;
                    var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                    if (noteCount - noHeadSlideCount == 1)
                        isEach = false;
                }
                var poolingInfo = new TouchPoolingInfo()
                {
                    SensorPos = sensorPos,
                    Timing = noteTiming,
                    AppearTiming = appearTiming,
                    AreaPos = areaPosition,
                    StartPos = startPosition,
                    Speed = speed,
                    IsFirework = isFirework,
                    IsEach = isEach,
                    IsBreak = isBreak,
                    IsEX = false,
                    NoteSortOrder = noteSortOrder,
                    QueueInfo = queueInfo,
                };
                if (isEach)
                    members.Add(poolingInfo);
                return poolingInfo;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                var line = timing.RawTextPositionY;
                var column = timing.RawTextPositionX;
                throw new InvalidSimaiSyntaxException(line,
                                                      column,
                                                      note.RawContent,
                                                      BuildSyntaxErrorMessage(line, column, note.RawContent));
            }
        }
        TouchHoldPoolingInfo CreateTouchHold(in SimaiNote note,
                                             in SimaiTimingPoint timing,
                                             in List<ITouchGroupInfoProvider> members)
        {
            try
            {
                note.StartPosition = NoteCreateHelper.Rotation(note.StartPosition, ChartRotation);
                var sensorPos = NoteHelper.GetSensor(note.TouchArea, note.StartPosition);
                NoteCreateHelper.SetNewPositionIfRequested(ref sensorPos, _touchPanelMappingTable);
                var queueInfo = new TouchQueueInfo()
                {
                    SensorPos = sensorPos,
                    Index = _touchIndex[sensorPos]++
                };
                var startPosition = note.StartPosition;
                var areaPosition = note.TouchArea;
                var noteTiming = (float)timing.Timing;
                var lastFor = (float)note.HoldTime;
                var speed = TouchSpeed * Math.Abs(timing.HSpeed);
                var isFirework = note.IsHanabi;
                var isBreak = note.IsBreak;
                var isEach = timing.Notes.Length > 1;
                var moveDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
                var appearTiming = Math.Min(noteTiming - moveDuration, noteTiming - 0.15f);
                var noteSortOrder = _touchSortOrder;
                if (appearTiming < -5f && _gpManager is not null)
                    _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);

                _touchSortOrder -= NOTE_LAYER_COUNT[note.Type];
                sensorPos = NoteCreateHelper.Rotation(sensorPos, ChartRotation);
                var poolingInfo = new TouchHoldPoolingInfo()
                {
                    SensorPos = sensorPos,
                    Timing = noteTiming,
                    AppearTiming = appearTiming,
                    AreaPos = areaPosition,
                    StartPos = startPosition,
                    Speed = speed,
                    IsFirework = isFirework,
                    IsEach = isEach,
                    IsBreak = isBreak,
                    IsEX = false,
                    LastFor = lastFor,
                    NoteSortOrder = noteSortOrder,
                    QueueInfo = queueInfo,
                };
                if (isEach)
                    members.Add(poolingInfo);
                return poolingInfo;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                var line = timing.RawTextPositionY;
                var column = timing.RawTextPositionX;
                throw new InvalidSimaiSyntaxException(line,
                                                      column,
                                                      note.RawContent,
                                                      BuildSyntaxErrorMessage(line, column, note.RawContent));
            }
        }
        async Task AllocTouchGroup(List<ITouchGroupInfoProvider> members, CancellationToken token = default)
        {
            await Task.Run(() =>
            {
                var sensorTypes = members.GroupBy(x => x.SensorPos)
                                         .Select(x => x.Key)
                                         .ToList();
                List<List<SensorArea>> sensorGroups = new();

                while (sensorTypes.Count > 0)
                {
                    var sensorType = sensorTypes[0];
                    List<SensorArea> groupMembers = new();
                    groupMembers.Add(sensorType);

                    for (var i = 0; i < groupMembers.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        var currentArea = groupMembers[i];
                        var nearbyArea = TOUCH_GROUPS[currentArea];
                        for (var j = 0; j < sensorTypes.Count; j++)
                        {
                            token.ThrowIfCancellationRequested();
                            var area = sensorTypes[j];
                            if (groupMembers.Contains(area))
                            {
                                continue;
                            }
                            else if (nearbyArea.Contains(area))
                            {
                                groupMembers.Add(area);
                            }
                        }
                    }

                    foreach (var area in groupMembers)
                    {
                        token.ThrowIfCancellationRequested();
                        sensorTypes.Remove(area);
                    }
                    token.ThrowIfCancellationRequested();
                    sensorGroups.Add(groupMembers);
                }
                List<TouchGroup> touchGroups = new();
                var memberMapping = members.GroupBy(x => x.SensorPos).ToDictionary(x => x.Key);
                token.ThrowIfCancellationRequested();
                foreach (var group in sensorGroups)
                {
                    token.ThrowIfCancellationRequested();
                    touchGroups.Add(new TouchGroup()
                    {
                        Members = group.SelectMany(x => memberMapping[x]).ToArray()
                    });
                }
                foreach (var member in members)
                {
                    token.ThrowIfCancellationRequested();
                    member.GroupInfo = touchGroups.Find(x => x.Members.Any(y => y == member));
                }
            });
        }
        private void CreateSlideGroup(SimaiTimingPoint timing, FoldedSimaiNote note, in List<NotePoolingInfo?> eachNotes)
        {
            try
            {
                int charIntParse(char c)
                {
                    return c - '0';
                }

                var subSlide = new List<SubSlideNote>();
                var subBarCount = new List<int>();
                var sumBarCount = 0;

                var noteContent = note.RawContent;
                var latestStartIndex = charIntParse(noteContent[0]); // 存储上一个Slide的结尾 也就是下一个Slide的起点
                var ptr = 1; // 指向目前处理的字符

                var specTimeFlag = 0; // 表示此组合slide是指定总时长 还是指定每一段的时长
                                      // 0-目前还没有读取 1-读取到了一个未指定时长的段落 2-读取到了一个指定时长的段落 3-（期望）读取到了最后一个时长指定

                while (ptr < noteContent.Length)
                {
                    if (!char.IsNumber(noteContent[ptr]))
                    {
                        // 读取到字符
                        var slideTypeChar = noteContent[ptr++].ToString();

                        var slidePart = new SubSlideNote();
                        slidePart.Type = SimaiNoteType.Slide;
                        slidePart.StartPosition = latestStartIndex;
                        if (slideTypeChar == "V")
                        {
                            // 转折星星
                            var middlePos = noteContent[ptr++];
                            var endPos = noteContent[ptr++];

                            slidePart.RawContent = latestStartIndex + slideTypeChar + middlePos + endPos;
                            latestStartIndex = charIntParse(endPos);
                        }
                        else
                        {
                            // 其他普通星星
                            // 额外检查pp和qq
                            if (noteContent[ptr] == slideTypeChar[0]) slideTypeChar += noteContent[ptr++];
                            var endPos = noteContent[ptr++];

                            slidePart.RawContent = latestStartIndex + slideTypeChar + endPos;
                            latestStartIndex = charIntParse(endPos);
                        }

                        if (noteContent[ptr] == '[')
                        {
                            // 如果指定了速度
                            if (specTimeFlag == 0)
                                // 之前未读取过
                                specTimeFlag = 2;
                            else if (specTimeFlag == 1)
                                // 之前读取到的都是未指定时长的段落 那么将flag设为3 如果之后又读取到时长 则报错
                                specTimeFlag = 3;
                            else if (specTimeFlag == 3)
                                // 之前读取到了指定时长 并期待那个时长就是最终时长 但是又读取到一个新的时长 则报错
                                throw new Exception("组合星星有错误\nSLIDE CHAIN ERROR");

                            while (ptr < noteContent.Length && noteContent[ptr] != ']')
                                slidePart.RawContent += noteContent[ptr++];
                            slidePart.RawContent += noteContent[ptr++];
                        }
                        else
                        {
                            // 没有指定速度
                            if (specTimeFlag == 0)
                                // 之前未读取过
                                specTimeFlag = 1;
                            else if (specTimeFlag == 2 || specTimeFlag == 3)
                                // 之前读取到指定时长的段落了 说明这一条组合星星有的指定时长 有的没指定 则需要报错
                                throw new Exception("组合星星有错误\nSLIDE CHAIN ERROR");
                        }

                        string slideShape = NoteCreateHelper.DetectShapeFromText(slidePart.RawContent);
                        if (slideShape.StartsWith("-"))
                        {
                            slideShape = slideShape.Substring(1);
                        }
                        int slideIndex = SLIDE_PREFAB_MAP[slideShape];
                        if (slideIndex < 0) slideIndex = -slideIndex;

                        var barCount = slidePrefab[slideIndex].transform.childCount;
                        subBarCount.Add(barCount);
                        sumBarCount += barCount;

                        slidePart.Origin = note;
                        subSlide.Add(slidePart);
                    }
                    else
                    {
                        // 理论上来说 不应该读取到数字 因此如果读取到了 说明有语法错误
                        throw new Exception("组合星星有错误\nwSLIDE CHAIN ERROR");
                    }
                }

                subSlide.ForEach(o =>
                {
                    o.IsBreak = note.IsBreak;
                    o.IsEx = note.IsEx;
                    o.IsSlideBreak = note.IsSlideBreak;
                    o.IsSlideNoHead = true;
                });
                subSlide[0].IsSlideNoHead = note.IsSlideNoHead;

                if (specTimeFlag == 1 || specTimeFlag == 0)
                    // 如果到结束还是1 那说明没有一个指定了时长 报错
                    throw new Exception("组合星星有错误\nwSLIDE CHAIN ERROR");
                // 此时 flag为2表示每条指定语法 为3表示整体指定语法

                if (specTimeFlag == 3)
                {
                    // 整体指定语法 使用slideTime来计算
                    var tempBarCount = 0;
                    for (var i = 0; i < subSlide.Count; i++)
                    {
                        subSlide[i].SlideStartTime = note.SlideStartTime + (double)tempBarCount / sumBarCount * note.SlideTime;
                        subSlide[i].SlideTime = (double)subBarCount[i] / sumBarCount * note.SlideTime;
                        tempBarCount += subBarCount[i];
                    }
                }
                else
                {
                    // 每条指定语法

                    // 获取时长的子函数
                    double getTimeFromBeats(string noteText, float currentBpm)
                    {
                        var startIndex = noteText.IndexOf('[');
                        var overIndex = noteText.IndexOf(']');
                        var innerString = noteText.Substring(startIndex + 1, overIndex - startIndex - 1);
                        var timeOneBeat = 1d / (currentBpm / 60d);
                        if (innerString.Count(o => o == '#') == 1)
                        {
                            var times = innerString.Split('#');
                            if (times[1].Contains(':'))
                            {
                                innerString = times[1];
                                timeOneBeat = 1d / (double.Parse(times[0]) / 60d);
                            }
                            else
                            {
                                return double.Parse(times[1]);
                            }
                        }

                        if (innerString.Count(o => o == '#') == 2)
                        {
                            var times = innerString.Split('#');
                            return double.Parse(times[2]);
                        }

                        var numbers = innerString.Split(':');
                        var divide = int.Parse(numbers[0]);
                        var count = int.Parse(numbers[1]);


                        return timeOneBeat * 4d / divide * count;
                    }

                    double tempSlideTime = 0;
                    for (var i = 0; i < subSlide.Count; i++)
                    {
                        subSlide[i].SlideStartTime = note.SlideStartTime + tempSlideTime;
                        subSlide[i].SlideTime = getTimeFromBeats(subSlide[i].RawContent, timing.Bpm);
                        tempSlideTime += subSlide[i].SlideTime;
                    }
                }

                IConnectableSlide? parent = null;
                List<SlideDrop> subSlides = new();
                float totalLen = (float)subSlide.Select(x => x.SlideTime).Sum();
                float startTiming = (float)subSlide[0].SlideStartTime;
                float totalSlideLen = 0;
                int? extraRotation = null;
                CreateSlideResult<SlideDrop>? slideResult = null;
                for (var i = 0; i <= subSlide.Count - 1; i++)
                {
                    bool isConn = subSlide.Count != 1;
                    bool isGroupHead = i == 0;
                    bool isGroupEnd = i == subSlide.Count - 1;
                    SlideBase sliObj;

                    if (note.RawContent!.Contains('w')) //wifi
                    {
                        if (isConn)
                        {
                            throw new InvalidOperationException("不允许Wifi Slide作为Connection Slide的一部分");
                        }
                        var result = CreateWifi(timing, subSlide[i], note.Count);
                        sliObj = result.SlideInstance;
                        foreach(var starInfo in result.StarInfos)
                        {
                            if(starInfo is null)
                            {
                                continue;
                            }
                            eachNotes.Add(starInfo);
                        }
                        //AddSlideToQueue(timing, result.SlideInstance);
                        UpdateStarRotateSpeed(result, (float)subSlide[i].SlideTime, 20);
                        sliObj.Initialize();
                    }
                    else
                    {
                        var info = new ConnSlideInfo()
                        {
                            TotalLength = totalLen,
                            IsGroupPart = isConn,
                            IsGroupPartHead = isGroupHead,
                            IsGroupPartEnd = isGroupEnd,
                            Parent = parent,
                            StartTiming = startTiming
                        };
                        var result = CreateSlide(timing, subSlide[i], info, note.Count, ref extraRotation);
                        parent = result.SlideInstance;
                        sliObj = result.SlideInstance;
                        foreach (var starInfo in result.StarInfos)
                        {
                            if (starInfo is null)
                            {
                                continue;
                            }
                            eachNotes.Add(starInfo);
                        }
                        subSlides.Add(result.SlideInstance);
                        if (i == 0)
                        {
                            slideResult = result;
                        }
                    }
                    AddSlideToQueue(timing, sliObj);
                }
                long judgeQueueLen = 0;
                var slideCount = subSlides.Count;
                foreach (var (i, s) in subSlides.WithIndex())
                {
                    var isFirst = i == 0;
                    var isEnd = i == slideCount - 1;
                    var table = SlideTables.FindTableByName(s.SlideType);

                    totalSlideLen += s.SlideLength;
                    if (isEnd)
                    {
                        judgeQueueLen += table!.JudgeQueue.Length;
                    }
                    else
                    {
                        judgeQueueLen += table!.JudgeQueue.Length - 1;
                    }
                }
                subSlides.ForEach(s =>
                {
                    s.ConnectInfo.TotalSlideLen = totalSlideLen;
                    s.ConnectInfo.TotalJudgeQueueLen = judgeQueueLen;
                });
                subSlides.ForEach(s => s.Initialize());
                if (slideResult is not null)
                {
                    UpdateStarRotateSpeed((CreateSlideResult<SlideDrop>)slideResult, totalLen, totalSlideLen);
                }
            }
            catch (UnityException)
            {
                throw;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                var line = timing.RawTextPositionY;
                var column = timing.RawTextPositionX;
                throw new InvalidSimaiSyntaxException(line,
                                                      column,
                                                      note.RawContent,
                                                      BuildSyntaxErrorMessage(line, column, note.RawContent));
            }
        }
        void UpdateStarRotateSpeed<T>(CreateSlideResult<T> result, float totalLen, float totalSlideLen) where T : SlideBase
        {
            var speed = (totalSlideLen * 0.47f) / (totalLen * 1000);
            var ratio = speed / 0.0034803742562305f;

            foreach(var starInfo in result.StarInfos)
            {
                if (starInfo is not null)
                {
                    starInfo.RotateSpeed = Math.Max(-(68.54838709677419f) * ratio, -1080);
                }
            }
        }
        void AddSlideToQueue<T>(SimaiTimingPoint timing, T SliCompo) where T : SlideBase
        {
            var speed = NoteSpeed * timing.HSpeed;
            var scaleRate = MajInstances.Settings.Debug.NoteAppearRate;
            var slideFadeInTiming = Math.Max((-3.926913f / speed) + MajInstances.Settings.Game.SlideFadeInOffset + (float)timing.Timing, -5f);
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
            var appearTiming = (float)timing.Timing + appearDiff;
            _slideQueueInfos.Add(new()
            {
                Index = _slideIndex++,
                SlideObject = SliCompo,
                AppearTiming = Math.Min(appearTiming, slideFadeInTiming)
            });
        }
        private CreateSlideResult<SlideDrop> CreateSlide(SimaiTimingPoint timing,
                                                         SubSlideNote note,
                                                         ConnSlideInfo info,
                                                         in int multiple,
                                                         ref int? extraRotation)
        {
            string slideShape = NoteCreateHelper.DetectShapeFromText(note.RawContent);
            var isMirror = false;
            var isEach = false;
            if (slideShape.StartsWith("-"))
            {
                isMirror = true;
                slideShape = slideShape.Substring(1);
            }
            var slideIndex = SLIDE_PREFAB_MAP[slideShape];
            var slide = Instantiate(slidePrefab[slideIndex], notes.transform.GetChild(3));
            //var slide_star = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            var SliCompo = slide.GetComponent<SlideDrop>();
            var isJustR = NoteCreateHelper.DetectJustType(note.RawContent, out int endPos);
            var startPos = note.StartPosition;

            //slide_star.SetActive(true);
            slide.SetActive(true);
            if(extraRotation is null)
            {
                startPos = NoteCreateHelper.Rotation(startPos, ChartRotation);
                endPos = NoteCreateHelper.Rotation(endPos, ChartRotation);
                var oldStartPos = startPos;
                NoteCreateHelper.SetSlideNewPositionIfRequested(ref startPos, ref endPos, _buttonRingMappingTable);
                var diff = oldStartPos - startPos;
                if (diff > 0)
                {
                    diff = 8 - diff;
                }
                else if (diff < 0)
                {
                    diff = Math.Abs(diff);
                }
                extraRotation = diff;
            }
            else if(extraRotation is int eR)
            {
                startPos = NoteCreateHelper.Rotation(startPos, ChartRotation + eR);
                endPos = NoteCreateHelper.Rotation(endPos, ChartRotation + eR);
            }

            TapPoolingInfo?[] starInfos = new TapPoolingInfo?[multiple];
            if (!note.IsSlideNoHead)
            {

                for (var i = 0; i < multiple; i++)
                {
                    var _info = CreateStar(startPos, note, timing);
                    _poolManager.AddTap(_info);
                    starInfos[i] = _info;
                }
            }

            //SliCompo.SlideType = slideShape;

            if (timing.Notes.Length > 1)
            {
                var slides = timing.Notes.FindAll(o => o.Type == SimaiNoteType.Slide);
                var index = slides.FindIndex(x => x == note.Origin) + 1;
                if (slides.Length > 1)
                {
                    isEach = true;
                    if (_gpManager is not null && _gpManager.IsClassicMode)
                    {
                        if (index == slides.Length && index % 2 != 0)
                        {
                            isEach = false;
                        }
                    }
                }
            }

            SliCompo.ConnectInfo = info;
            SliCompo.IsBreak = note.IsSlideBreak;
            SliCompo.IsEach = isEach || multiple > 1;
            SliCompo.IsMirror = isMirror;
            SliCompo.IsJustR = isJustR;
            SliCompo.EndPos = endPos;
            SliCompo.Speed = Math.Abs(NoteSpeed * timing.HSpeed);
            SliCompo.StartTiming = (float)note.SlideStartTime;
            SliCompo.StartPos = startPos;
            //SliCompo._stars = new GameObject[] { slide_star };
            SliCompo.Timing = (float)timing.Timing;
            SliCompo.Length = (float)note.SlideTime;
            SliCompo.IsSlideNoHead = _isSlideNoHead;
            SliCompo.IsSlideNoTrack = _isSlideNoTrack;
            SliCompo.Multiple = multiple;
            //SliCompo.sortIndex = -7000 + (int)((lastNoteTime - timing.Timing) * -100) + sort * 5;
            if (MajInstances.Settings.Display.SlideSortOrder == JudgeModeOption.Classic)
            {
                _slideLayer += SLIDE_AREA_STEP_MAP[slideShape].Last();
                SliCompo.SortOrder = _slideLayer;
            }
            else
            {
                SliCompo.SortOrder = _slideLayer;
                _slideLayer -= SLIDE_AREA_STEP_MAP[slideShape].Last();
            }
            //slideLayer += 5;

            return new()
            {
                SlideInstance = SliCompo,
                StarInfos = starInfos
            };
        }
        private CreateSlideResult<WifiDrop> CreateWifi(SimaiTimingPoint timing, SubSlideNote note, in int multiple)
        {
            var str = note.RawContent.Substring(0, 3);
            var digits = str.Split('w');
            var startPos = int.Parse(digits[0]);
            var endPos = int.Parse(digits[1]);
            var isEach = false;
            endPos = endPos - startPos;
            endPos = endPos < 0 ? endPos + 8 : endPos;
            endPos = endPos > 8 ? endPos - 8 : endPos;
            endPos++;

            var slideWifi = Instantiate(slidePrefab[SLIDE_PREFAB_MAP["wifi"]], notes.transform.GetChild(3));
            var WifiCompo = slideWifi.GetComponent<WifiDrop>();
            var isJustR = NoteCreateHelper.DetectJustType(note.RawContent, out endPos);

            startPos = NoteCreateHelper.Rotation(startPos, ChartRotation);
            endPos = NoteCreateHelper.Rotation(endPos, ChartRotation);
            NoteCreateHelper.SetSlideNewPositionIfRequested(ref startPos, ref endPos, _buttonRingMappingTable);
            slideWifi.SetActive(true);

            TapPoolingInfo?[] starInfos = new TapPoolingInfo?[multiple];
            if (!note.IsSlideNoHead)
            {

                for (var i = 0; i < multiple; i++)
                {
                    var _info = CreateStar(startPos, note, timing);
                    _poolManager.AddTap(_info);
                    starInfos[i] = _info;
                }
            }

            if (timing.Notes.Length > 1)
            {
                var slides = timing.Notes.FindAll(o => o.Type == SimaiNoteType.Slide);
                var index = slides.FindIndex(x => x == note.Origin) + 1;
                if (slides.Length > 1)
                {
                    isEach = true;
                    if (_gpManager is not null && _gpManager.IsClassicMode)
                    {
                        if (index == slides.Length && index % 2 != 0)
                        {
                            isEach = false;
                        }
                    }
                }
            }

            WifiCompo.IsBreak = note.IsSlideBreak;
            WifiCompo.IsEach = isEach || multiple > 1;
            WifiCompo.IsJustR = isJustR;
            WifiCompo.EndPos = endPos;
            WifiCompo.Speed = Math.Abs(NoteSpeed * timing.HSpeed);
            WifiCompo.StartTiming = (float)note.SlideStartTime;
            WifiCompo.StartPos = startPos;
            WifiCompo.Timing = (float)timing.Timing;
            WifiCompo.Length = (float)note.SlideTime;
            WifiCompo.IsSlideNoHead = _isSlideNoHead;
            WifiCompo.IsSlideNoTrack = _isSlideNoTrack;
            WifiCompo.Multiple = multiple;
            //var centerStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            //var leftStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            //var rightStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            //WifiCompo._stars = new GameObject[3]
            //{
            //    rightStar,
            //    centerStar,
            //    leftStar
            //};
            if (MajInstances.Settings.Display.SlideSortOrder == JudgeModeOption.Classic)
            {
                _slideLayer += SLIDE_AREA_STEP_MAP["wifi"].Last();
                WifiCompo.SortOrder = _slideLayer;
            }
            else
            {
                WifiCompo.SortOrder = _slideLayer;
                _slideLayer -= SLIDE_AREA_STEP_MAP["wifi"].Last();
            }
            //slideLayer += 5;

            return new()
            {
                SlideInstance = WifiCompo,
                StarInfos = starInfos
            };
        }
        


        string BuildSyntaxErrorMessage(int line, int column, string noteContent)
        {
            return $"(at L{line}:C{column}) \"{noteContent}\" is not a valid note syntax";
        }
        string BuildSyntaxErrorMessage(int line, int column)
        {
            return $"(at L{line}:C{column})";
        }
        static class NoteCreateHelper
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SensorArea Rotation(SensorArea sensorIndex, int diff)
            {
                return sensorIndex.Diff(diff);
            }
            public static int Rotation(int keyIndex, int diff)
            {
                if (!keyIndex.InRange(1, 8))
                    throw new ArgumentOutOfRangeException();
                var key = (SensorArea)(keyIndex - 1);
                var newKey = key.Diff(diff);
                return newKey.GetIndex();
            }
            public static int MirrorKeys(int key)
            {
                switch(key)
                {
                    case 1:
                        return 1;
                    case 2:
                        return 8;
                    case 3:
                        return 7;
                    case 4:
                        return 6;
                    case 5:
                        return 5;
                    case 6:
                        return 4;
                    case 7:
                        return 3;
                    case 8:
                        return 2;
                    default:
                        throw new Exception("Keys out of range: " + key);
                }
            }
            public static bool IsRightHalf(int key)
            {
                switch(key)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        return true;
                    default:
                        return false;

                }
            }
            public static bool IsUpperHalf(int key)
            {
                switch (key)
                {
                    case 7:
                    case 8:
                    case 1:
                    case 2:
                        return true;
                    default:
                        return false;

                }
            }
            public static string DetectShapeFromText(string content)
            {
                int getRelativeEndPos(int startPos, int endPos)
                {
                    endPos = endPos - startPos;
                    endPos = endPos < 0 ? endPos + 8 : endPos;
                    endPos = endPos > 8 ? endPos - 8 : endPos;
                    return endPos + 1;
                }

                //print(content);
                if (content.Contains('-'))
                {
                    // line
                    var str = content.Substring(0, 3); //something like "8-6"
                    var digits = str.Split('-');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (endPos < 3 || endPos > 7) throw new Exception("-星星至少隔开一键\n-スライドエラー");
                    return "line" + endPos;
                }

                if (content.Contains('>'))
                {
                    // circle 默认顺时针
                    var str = content.Substring(0, 3);
                    var digits = str.Split('>');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (NoteCreateHelper.IsUpperHalf(startPos))
                    {
                        return "circle" + endPos;
                    }

                    endPos = NoteCreateHelper.MirrorKeys(endPos);
                    return "-circle" + endPos; //Mirror
                }

                if (content.Contains('<'))
                {
                    // circle 默认顺时针
                    var str = content.Substring(0, 3);
                    var digits = str.Split('<');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (!NoteCreateHelper.IsUpperHalf(startPos))
                    {
                        return "circle" + endPos;
                    }

                    endPos = NoteCreateHelper.MirrorKeys(endPos);
                    return "-circle" + endPos; //Mirror
                }

                if (content.Contains('^'))
                {
                    var str = content.Substring(0, 3);
                    var digits = str.Split('^');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);

                    if (endPos == 1 || endPos == 5)
                    {
                        throw new Exception("^星星不合法\n^スライドエラー");
                    }

                    if (endPos < 5)
                    {
                        return "circle" + endPos;
                    }
                    if (endPos > 5)
                    {
                        return "-circle" + NoteCreateHelper.MirrorKeys(endPos);
                    }
                }

                if (content.Contains('v'))
                {
                    // v
                    var str = content.Substring(0, 3);
                    var digits = str.Split('v');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (endPos == 5) throw new Exception("v星星不合法\nvスライドエラー");
                    return "v" + endPos;
                }

                if (content.Contains("pp"))
                {
                    // ppqq 默认为pp
                    var str = content.Substring(0, 4);
                    var digits = str.Split('p');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[2]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    return "ppqq" + endPos;
                }

                if (content.Contains("qq"))
                {
                    // ppqq 默认为pp
                    var str = content.Substring(0, 4);
                    var digits = str.Split('q');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[2]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    endPos = NoteCreateHelper.MirrorKeys(endPos);
                    return "-ppqq" + endPos;
                }

                if (content.Contains('p'))
                {
                    // pq 默认为p
                    var str = content.Substring(0, 3);
                    var digits = str.Split('p');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    return "pq" + endPos;
                }

                if (content.Contains('q'))
                {
                    // pq 默认为p
                    var str = content.Substring(0, 3);
                    var digits = str.Split('q');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    endPos = NoteCreateHelper.MirrorKeys(endPos);
                    return "-pq" + endPos;
                }

                if (content.Contains('s'))
                {
                    // s
                    var str = content.Substring(0, 3);
                    var digits = str.Split('s');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (endPos != 5) throw new Exception("s星星尾部错误\nsスライドエラー");
                    return "s";
                }

                if (content.Contains('z'))
                {
                    // s镜像
                    var str = content.Substring(0, 3);
                    var digits = str.Split('z');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (endPos != 5) throw new Exception("z星星尾部错误\nzスライドエラー");
                    return "-s";
                }

                if (content.Contains('V'))
                {
                    // L
                    var str = content.Substring(0, 4);
                    var digits = str.Split('V');
                    var startPos = int.Parse(digits[0]);
                    var turnPos = int.Parse(digits[1][0].ToString());
                    var endPos = int.Parse(digits[1][1].ToString());

                    turnPos = getRelativeEndPos(startPos, turnPos);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (turnPos == 7)
                    {
                        if (endPos < 2 || endPos > 5) throw new Exception("V星星终点不合法\nVスライドエラー");
                        return "L" + endPos;
                    }

                    if (turnPos == 3)
                    {
                        if (endPos < 5) throw new Exception("V星星终点不合法\nVスライドエラー");
                        return "-L" + NoteCreateHelper.MirrorKeys(endPos);
                    }

                    throw new Exception("V星星拐点只能隔开一键\nVスライドエラー");
                }

                if (content.Contains('w'))
                {
                    // wifi
                    var str = content.Substring(0, 3);
                    var digits = str.Split('w');
                    var startPos = int.Parse(digits[0]);
                    var endPos = int.Parse(digits[1]);
                    endPos = getRelativeEndPos(startPos, endPos);
                    if (endPos != 5) throw new Exception("w星星尾部错误\nwスライドエラー");
                    return "wifi";
                }

                return "";
            }
            /// <summary>
            /// 判断Slide SlideOK是否需要镜像翻转
            /// </summary>
            /// <param name="content"></param>
            /// <param name="endPos"></param>
            /// <returns></returns>
            public static bool DetectJustType(string content, out int endPos)
            {
                // > < ^ V w
                if (content.Contains('>'))
                {
                    var str = content.Substring(0, 3);
                    var digits = str.Split('>');
                    var startPos = int.Parse(digits[0]);
                    endPos = int.Parse(digits[1]);

                    if (NoteCreateHelper.IsUpperHalf(startPos))
                        return true;
                    return false;
                }

                if (content.Contains('<'))
                {
                    var str = content.Substring(0, 3);
                    var digits = str.Split('<');
                    var startPos = int.Parse(digits[0]);
                    endPos = int.Parse(digits[1]);

                    if (!NoteCreateHelper.IsUpperHalf(startPos))
                        return true;
                    return false;
                }

                if (content.Contains('^'))
                {
                    var str = content.Substring(0, 3);
                    var digits = str.Split('^');
                    var startPos = int.Parse(digits[0]);
                    endPos = int.Parse(digits[1]);
                    endPos = endPos - startPos;
                    endPos = endPos < 0 ? endPos + 8 : endPos;
                    endPos = endPos > 8 ? endPos - 8 : endPos;

                    if (endPos < 4)
                    {
                        endPos = int.Parse(digits[1]);
                        return true;
                    }
                    if (endPos > 4)
                    {
                        endPos = int.Parse(digits[1]);
                        return false;
                    }
                }
                else if (content.Contains('V'))
                {
                    var str = content.Substring(0, 4);
                    var digits = str.Split('V');
                    endPos = int.Parse(digits[1][1].ToString());

                    if (NoteCreateHelper.IsRightHalf(endPos))
                        return true;
                    return false;
                }
                else if (content.Contains('w'))
                {
                    var str = content.Substring(0, 3);
                    endPos = int.Parse(str.Substring(2, 1));
                    if (NoteCreateHelper.IsUpperHalf(endPos))
                        return true;
                    return false;
                }
                else
                {
                    //int endPos;
                    if (content.Contains("qq") || content.Contains("pp"))
                        endPos = int.Parse(content.Substring(3, 1));
                    else
                        endPos = int.Parse(content.Substring(2, 1));
                    if (NoteCreateHelper.IsRightHalf(endPos))
                        return true;
                    return false;
                }
                return true;
            }
            public static (IReadOnlyDictionary<int, int> ,IReadOnlyDictionary<SensorArea, SensorArea>) GenerateMappingTable()
            {
                var touchPannelMappingTable = GenerateTouchPanelMappingTable();
                var buttonRingMappingTable = GenerateButtonRingMappingTable();
                foreach(var (k,v) in buttonRingMappingTable)
                {
                    touchPannelMappingTable[(SensorArea)(k - 1)] = (SensorArea)(v - 1);
                }
                return (buttonRingMappingTable, touchPannelMappingTable);
            }
            static Dictionary<SensorArea,SensorArea> GenerateTouchPanelMappingTable()
            {
                var areas = ((SensorArea[])Enum.GetValues(typeof(SensorArea))).ToArray();
                var newAreas = new SensorArea?[33];
                var rd = new System.Random();
                var dict = new Dictionary<SensorArea, SensorArea>();

                for (var i = 0; i < 33; i++)
                {
                    var originArea = (SensorArea)i;
                    SensorArea value;
                    if(i < 8)
                    {
                        newAreas[i] = originArea;
                        continue;
                    }
                    while(true)
                    {
                        value = (SensorArea)rd.Next(0, 33);
                        if (value > SensorArea.E8 || value < SensorArea.A1)
                            continue;
                        else if (value.GetGroup() != originArea.GetGroup())
                            continue;
                        else if (!newAreas.Contains(value))
                            break;
                    }
                    newAreas[i] = value;
                }

                for (var i = 0; i < 33; i++)
                {
                    dict.Add(areas[i], (SensorArea)newAreas[i]!);
                }
                return dict;
            }
            static Dictionary<int, int> GenerateButtonRingMappingTable()
            {
                var areas = new int[8]
                {
                    1,2,3,4,5,6,7,8
                };
                var newAreas = new int?[8];
                var rd = new System.Random();
                var dict = new Dictionary<int, int>();

                for (var i = 0; i < 8; i++)
                {
                    int value;
                    do
                    {
                        value = rd.Next(1, 9);
                        if (value > 8 || value < 1)
                            continue;
                    }
                    while (newAreas.Contains(value));
                    newAreas[i] = value;
                }

                for (var i = 0; i < 8; i++)
                {
                    dict.Add(areas[i], (int)newAreas[i]!);
                }
                return dict;
            }
            static int RandomTap(int originKeyIndex, IReadOnlyDictionary<int, int> mappingTable)
            {
                return mappingTable[originKeyIndex];
            }
            static SensorArea RandomTouch(SensorArea originArea, IReadOnlyDictionary<SensorArea,SensorArea> mappingTable)
            {
                return mappingTable[originArea];
            }
            static (int,int) RandomSlide(int startPos,int endPos, IReadOnlyDictionary<int, int> mappingTable)
            {
                var diff = startPos - endPos;
                if (diff > 0)
                {
                    diff = 8 - diff;
                }
                else if (diff < 0)
                {
                    diff = Math.Abs(diff);
                }
                var newStartPos = mappingTable[startPos];
                var newEndPos = ((SensorArea)(newStartPos - 1)).Diff(diff).GetIndex();

                return (newStartPos, newEndPos);
            }
            static int RandomTap()
            {
                var rd = new System.Random();
                return rd.Next(1, 9);
            }
            static SensorArea RandomTouch()
            {
                var rd = new System.Random();
                return (SensorArea)rd.Next(0, 33);
            }
            static (int, int) RandomSlide(int startPos, int endPos)
            {
                var diff = startPos - endPos;
                if (diff > 0)
                {
                    diff = 8 - diff;
                }
                else if (diff < 0)
                {
                    diff = Math.Abs(diff);
                }
                var rd = new System.Random();
                var newStartPos = rd.Next(1, 9);
                var newEndPos = ((SensorArea)(newStartPos - 1)).Diff(diff).GetIndex();

                return (newStartPos, newEndPos);
            }
            public static void SetNewPositionIfRequested(ref int originPos, 
                                                         IReadOnlyDictionary<int, int> mappingTable)
            {
                switch(MajEnv.UserSettings.Game.Random)
                {
                    case RandomModeOption.Disabled:
                        return;
                    case RandomModeOption.RANDOM:
                        originPos = RandomTap(originPos, mappingTable);
                        break;
                    case RandomModeOption.S_RANDOM:
                        originPos = RandomTap();
                        break;
                }
            }
            public static void SetNewPositionIfRequested(ref SensorArea originPos, 
                                                         IReadOnlyDictionary<SensorArea, SensorArea> mappingTable)
            {
                switch (MajEnv.UserSettings.Game.Random)
                {
                    case RandomModeOption.Disabled:
                        return;
                    case RandomModeOption.RANDOM:
                        originPos = RandomTouch(originPos, mappingTable);
                        break;
                    case RandomModeOption.S_RANDOM:
                        originPos = RandomTouch();
                        break;
                }
            }
            public static void SetSlideNewPositionIfRequested(ref int originStartPos, 
                                                              ref int originEndPos,
                                                              IReadOnlyDictionary<int, int> mappingTable)
            {
                switch (MajEnv.UserSettings.Game.Random)
                {
                    case RandomModeOption.Disabled:
                        return;
                    case RandomModeOption.RANDOM:
                        (originStartPos, originEndPos) = RandomSlide(originStartPos, originEndPos, mappingTable);
                        break;
                    case RandomModeOption.S_RANDOM:
                        (originStartPos, originEndPos) = RandomSlide(originStartPos, originEndPos);
                        break;
                }
            }
            public static SimaiNote[] NoteFolding(SimaiNote[] simaiNotes)
            {
                if(!USERSETTING_NOTE_FOLDING)
                {
                    return simaiNotes;
                }
                var buffer = ArrayPool<FoldingSimaiNote>.Shared.Rent(4);
                var buffer2 = ArrayPool<FoldingSimaiNote>.Shared.Rent(4);
                var buffer3 = ArrayPool<FoldedSimaiNote>.Shared.Rent(4);
                try
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    Array.Clear(buffer2, 0, buffer2.Length);
                    Array.Clear(buffer3, 0, buffer3.Length);
                    var bufferIndex = 0;
                    var buffer2Index = 0;
                    var buffer3Index = 0;

                    foreach (var note in simaiNotes)
                    {
                        var foldingNote = new FoldingSimaiNote(note);
                        if (foldingNote.Type == SimaiNoteType.Slide)
                        {
                            BufferHelper.EnsureBufferLength(bufferIndex + 1, ref buffer);
                            buffer[bufferIndex++] = foldingNote;
                            continue;
                        }
                        else
                        {
                            BufferHelper.EnsureBufferLength(buffer2Index + 1, ref buffer2);
                            buffer2[buffer2Index++] = foldingNote;
                            continue;
                        }
                    }
                    var groupedSlides = buffer.GroupBy(x => x);
                    foreach (var slides in groupedSlides)
                    {
                        var key = slides.Key;
                        if (key.Origin == null)
                        {
                            continue;
                        }
                        BufferHelper.EnsureBufferLength(buffer3Index + 1, ref buffer3);

                        buffer3[buffer3Index++] = new()
                        {
                            Type = key.Type,
                            StartPosition = key.StartPosition,
                            HoldTime = key.HoldTime,
                            IsBreak = key.IsBreak,
                            IsEx = key.IsEx,
                            IsFakeRotate = key.IsFakeRotate,
                            IsForceStar = key.IsForceStar,
                            IsHanabi = key.IsHanabi,
                            IsSlideBreak = key.IsSlideBreak,
                            IsSlideNoHead = key.IsSlideNoHead,
                            RawContent = key.RawContent,
                            SlideStartTime = key.SlideStartTime,
                            SlideTime = key.SlideTime,
                            TouchArea = key.TouchArea,
                            Count = slides.Count()
                        };
                    }
                    var result = new SimaiNote[buffer2Index + buffer3Index];
                    var resultIndex = 0;
                    foreach (var note in buffer2.AsSpan(0, buffer2Index))
                    {
                        result[resultIndex++] = note.Origin!;
                    }
                    foreach (var note in buffer3.AsSpan(0, buffer3Index))
                    {
                        result[resultIndex++] = note;
                    }

                    return result;
                }
                finally
                {
                    ArrayPool<FoldingSimaiNote>.Shared.Return(buffer);
                    ArrayPool<FoldingSimaiNote>.Shared.Return(buffer2);
                    ArrayPool<FoldedSimaiNote>.Shared.Return(buffer3);
                }
            }
        }

        readonly struct FoldingSimaiNote
        {
            public SimaiNoteType Type
            {
                get => _origin.Type;
            }
            public int StartPosition
            {
                get => _origin.StartPosition;
            }
            public double HoldTime
            {
                get => _origin.HoldTime;
            }
            public bool IsBreak
            {
                get => _origin.IsBreak;
            }
            public bool IsEx
            {
                get => _origin.IsEx;
            }
            public bool IsFakeRotate
            {
                get => _origin.IsFakeRotate;
            }
            public bool IsForceStar
            {
                get => _origin.IsForceStar;
            }
            public bool IsHanabi
            {
                get => _origin.IsHanabi;
            }
            public bool IsSlideBreak
            {
                get => _origin.IsSlideBreak;
            }
            public bool IsSlideNoHead
            {
                get => _origin.IsSlideNoHead;
            }
            public string RawContent
            {
                get => _origin.RawContent;
            }
            public double SlideStartTime
            {
                get => _origin.SlideStartTime;
            }
            public double SlideTime
            {
                get => _origin.SlideTime;
            }
            public char TouchArea
            {
                get => _origin.TouchArea;
            }
            public SimaiNote? Origin
            {
                get => _origin;
            }

            readonly SimaiNote? _origin;
            readonly int _hashCode;
            public FoldingSimaiNote(SimaiNote origin,bool? isSlideNoHead = null)
            {
                _origin = origin;
                var hash1 = HashCode.Combine(
                    _origin.Type,
                    _origin.StartPosition,
                    _origin.RawContent,
                    _origin.HoldTime,
                    _origin.SlideStartTime,
                    _origin.SlideTime,
                    _origin.IsBreak,
                    _origin.IsEx
                );
                var hash2 = HashCode.Combine(
                    hash1,
                    _origin.IsHanabi,
                    _origin.IsSlideBreak,
                    isSlideNoHead ?? _origin.IsSlideNoHead,
                    _origin.IsFakeRotate,
                    _origin.IsForceStar,
                    _origin.TouchArea
                );
                _hashCode = hash2;
            }
            public static implicit operator FoldingSimaiNote(SimaiNote origin)
            {
                return new(origin);
            }
            public static bool operator ==(FoldingSimaiNote left, FoldingSimaiNote right)
            {
                //return left.Type == right.Type &&
                //       left.StartPosition == right.StartPosition &&
                //       left.RawContent == right.RawContent &&
                //       left.HoldTime == right.HoldTime &&
                //       left.SlideStartTime == right.SlideStartTime &&
                //       left.SlideTime == left.SlideTime &&
                //       left.IsBreak == left.IsBreak &&
                //       left.IsEx == right.IsEx &&
                //       left.IsHanabi == right.IsHanabi &&
                //       left.IsSlideBreak == left.IsSlideBreak &&
                //       left.IsSlideNoHead == right.IsSlideNoHead &&
                //       left.IsFakeRotate == right.IsFakeRotate &&
                //       left.IsForceStar == right.IsForceStar &&
                //       left.TouchArea == right.TouchArea;
                return left._hashCode == right._hashCode;
            }
            public static bool operator !=(FoldingSimaiNote left, FoldingSimaiNote right)
            {
                return !(left == right);
            }
            public override bool Equals(object obj)
            {
                if(obj is not FoldingSimaiNote obj2)
                {
                    return false;
                }
                return this == obj2;
            }
            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
        class FoldedSimaiNote : SimaiNote
        {
            public int Count { get; init; }
        }
        class SubSlideNote : SimaiNote
        {
            public SimaiNote Origin { get; set; } = new();
        }
        readonly struct CreateSlideResult<T> where T : SlideBase
        {
            public T SlideInstance { get; init; }
            public TapPoolingInfo?[] StarInfos { get; init; }
        }
    }
}