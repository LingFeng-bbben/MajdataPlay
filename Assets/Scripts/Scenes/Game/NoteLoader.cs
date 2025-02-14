using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MajSimai;
using MajdataPlay.Types;
using MajdataPlay.Game.Notes;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;
using MajdataPlay.Game.Types;
using MajdataPlay.Game.Utils;
using MajdataPlay.Collections;
using MajdataPlay.Game.Buffers;

namespace MajdataPlay.Game
{
#nullable enable
    public class NoteLoader : MonoBehaviour
    {
        public double Process { get; set; } = 0;
        public float NoteSpeed { get; set; } = 7f;
        public int ChartRotation { get; set; } = 0;
        public float TouchSpeed
        {
            get => _touchSpeed;
            set => _touchSpeed = Math.Abs(value);
        }

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
        GamePlayManager _gpManager;
        ObjectCounter _objectCounter;
        NotePoolManager _poolManager;

        static readonly Dictionary<SimaiNoteType, int> NOTE_LAYER_COUNT = new Dictionary<SimaiNoteType, int>()
        {
            {SimaiNoteType.Tap, 2 },
            {SimaiNoteType.Hold, 3 },
            {SimaiNoteType.Slide, 2 },
            {SimaiNoteType.Touch, 6 },
            {SimaiNoteType.TouchHold, 6 },
        };
        static readonly Dictionary<string, int> SLIDE_PREFAB_MAP = new Dictionary<string, int>()
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

        static readonly Dictionary<SensorArea, SensorArea[]> TOUCH_GROUPS = new()
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

        public static readonly Dictionary<string, List<int>> SLIDE_AREA_STEP_MAP = new Dictionary<string, List<int>>()
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

        void Awake()
        {
            MajInstanceHelper<NoteLoader>.Instance = this;
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteLoader>.Free();
        }
        private void Start()
        {
            _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
            _noteManager = MajInstanceHelper<NoteManager>.Instance!;
            _poolManager = MajInstanceHelper<NotePoolManager>.Instance!;
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            _slideUpdater = MajInstanceHelper<SlideUpdater>.Instance!;
        }
        internal async UniTask LoadNotesIntoPool(SimaiChart maiChart)
        {
            List<Task> touchTasks = new();

            _noteManager.ResetCounter();
            _noteIndex.Clear();
            _touchIndex.Clear();

            for (int i = 1; i < 9; i++)
                _noteIndex.Add(i, 0);
            for (int i = 0; i < 33; i++)
                _touchIndex.Add((SensorArea)i, 0);


            await CountNoteSumAsync(maiChart);

            var sum = _objectCounter.tapSum +
                      _objectCounter.holdSum +
                      _objectCounter.touchSum +
                      _objectCounter.breakSum +
                      _objectCounter.slideSum;

            var lastNoteTime = maiChart.NoteTimings.Last().Timing;

            foreach (var timing in maiChart.NoteTimings)
            {
                //var eachNotes = timing.Notes.FindAll(o => o.Type != SimaiNoteType.Touch &&
                //                                             o.Type != SimaiNoteType.TouchHold);
                int? num = null;
                touchTasks.Clear();
                //IDistanceProvider? provider = null;
                //IStatefulNote? noteA = null;
                //IStatefulNote? noteB = null;
                List<NotePoolingInfo?> eachNotes = new();
                //NotePoolingInfo? noteA = null;
                //NotePoolingInfo? noteB = null;
                List<ITouchGroupInfoProvider> members = new();
                foreach (var (i, note) in timing.Notes.WithIndex())
                {
                    _noteCount++;
                    Process = (double)_noteCount / sum;
                    if (_noteCount % 100 == 0)
                        await UniTask.Yield();

                    try
                    {
                        switch (note.Type)
                        {
                            case SimaiNoteType.Tap:
                                {
                                    var obj = CreateTap(note, timing);
                                    _poolManager.AddTap(obj);
                                    eachNotes.Add(obj);
                                    //if (eachNotes.Count > 1 && i < 2)
                                    //{
                                    //    if (num is null)
                                    //        num = 0;
                                    //    else if (num != 0)
                                    //        break;

                                    //    if (noteA is null)
                                    //        noteA = obj;
                                    //    else
                                    //        noteB = obj;
                                    //}
                                }
                                break;
                            case SimaiNoteType.Hold:
                                {
                                    var obj = CreateHold(note, timing);
                                    _poolManager.AddHold(obj);
                                    eachNotes.Add(obj);
                                    //if (eachNotes.Count > 1 && i < 2)
                                    //{
                                    //    if (num is null)
                                    //        num = 1;
                                    //    else if (num != 1)
                                    //        break;

                                    //    if (noteA is null)
                                    //        noteA = obj;
                                    //    else
                                    //        noteB = obj;
                                    //}
                                }
                                break;
                            case SimaiNoteType.TouchHold:
                                _poolManager.AddTouchHold(CreateTouchHold(note, timing, members));
                                break;
                            case SimaiNoteType.Touch:
                                _poolManager.AddTouch(CreateTouch(note, timing, members));
                                break;
                            case SimaiNoteType.Slide:
                                CreateSlideGroup(timing, note, eachNotes); // 星星组
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new InvalidChartSyntaxException(timing, e);
                    }
                }
                if (members.Count != 0)
                    touchTasks.Add(AllocTouchGroup(members));
                var _eachNotes = eachNotes.FindAll(x => x is not null);
                if (_eachNotes.Count > 1) //有多个非touchnote
                {
                    var noteA = _eachNotes[0]!;
                    var noteB = _eachNotes[1]!;

                    var startPos = noteA.StartPos;
                    var endPos = noteB.StartPos;
                    endPos = endPos - startPos;
                    if (endPos == 0)
                        continue;
                    var time = (float)timing.Timing;
                    var speed = NoteSpeed * timing.HSpeed;
                    var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
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
                    _poolManager.AddEachLine(new EachLinePoolingInfo()
                    {
                        StartPos = startPosition,
                        Timing = time,
                        AppearTiming = appearTiming,
                        CurvLength = curvLength,
                        MemberA = noteA,
                        MemberB = noteB,
                        Speed = speed
                    });
                }
            }

            var allTask = Task.WhenAll(touchTasks);
            while (!allTask.IsCompleted)
            {
                await UniTask.Yield();
                if (allTask.IsFaulted)
                    throw allTask.Exception.InnerException;
            }

            _slideUpdater.AddSlideQueueInfos(_slideQueueInfos.ToArray());
            _poolManager.Initialize();
        }
        TapPoolingInfo CreateTap(in SimaiNote note, in SimaiTimingPoint timing)
        {
            var startPos = note.StartPosition;
            var noteTiming = (float)timing.Timing;
            var speed = NoteSpeed * timing.HSpeed;
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
            var appearTiming = Math.Min(noteTiming + appearDiff, noteTiming - 0.15f);
            var sortOrder = _noteSortOrder;
            var isEach = timing.Notes.Length > 1;
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            if(isEach)
            {
                var noteCount = timing.Notes.Length;
                var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                if (noteCount - noHeadSlideCount == 1)
                    isEach = false;
            }

            _noteSortOrder -= NOTE_LAYER_COUNT[note.Type];
            startPos = Rotation(startPos);
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
                RotateSpeed = note.IsFakeRotate ? -440f: 0,
                QueueInfo = new TapQueueInfo()
                {
                    Index = _noteIndex[startPos]++,
                    KeyIndex = startPos
                }
            };
        }
        HoldPoolingInfo CreateHold(in SimaiNote note, in SimaiTimingPoint timing)
        {
            var startPos = note.StartPosition;
            var noteTiming = (float)timing.Timing;
            var speed = Math.Abs(NoteSpeed * timing.HSpeed);
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (speed * scaleRate);
            var appearTiming = Math.Min(noteTiming + appearDiff, noteTiming - 0.15f);
            var sortOrder = _noteSortOrder;
            var isEach = timing.Notes.Length > 1;
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            if (isEach)
            {
                var noteCount = timing.Notes.Length;
                var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                if (noteCount - noHeadSlideCount == 1)
                    isEach = false;
            }
            _noteSortOrder -= NOTE_LAYER_COUNT[note.Type];
            startPos = Rotation(startPos);
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
        TapPoolingInfo CreateStar(SimaiNote note, in SimaiTimingPoint timing)
        {
            var startPos = note.StartPosition;
            var noteTiming = (float)timing.Timing;
            var speed = NoteSpeed * timing.HSpeed;
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var slideFadeInTiming = (-3.926913f / speed) + MajInstances.Setting.Game.SlideFadeInOffset + (float)timing.Timing;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
            var appearTiming = Math.Min(noteTiming + appearDiff, noteTiming - 0.15f);
            var sortOrder = _noteSortOrder;
            var isEach = timing.Notes.Length > 1;
            bool isDouble = false;
            TapQueueInfo? queueInfo = null;

            appearTiming = Math.Min(appearTiming, slideFadeInTiming);

            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            _noteSortOrder -= NOTE_LAYER_COUNT[note.Type];

            if(isEach)
            {
                var count = timing.Notes.FindAll(
                    o => o.Type == SimaiNoteType.Slide &&
                         o.StartPosition == startPos).Length;
                if (count > 1)
                {
                    isDouble = true;
                    if (count == timing.Notes.Length)
                        isEach = false;
                    else
                    {
                        var noteCount = timing.Notes.Length;
                        var noHeadSlideCount = timing.Notes.FindAll(x => x.Type == SimaiNoteType.Slide && x.IsSlideNoHead).Length;
                        if (noteCount - noHeadSlideCount == 1)
                            isEach = false;
                    }
                }
            }
            startPos = Rotation(startPos);
            if(!note.IsSlideNoHead)
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
        TouchPoolingInfo CreateTouch(in SimaiNote note,
                                     in SimaiTimingPoint timing,
                                     in List<ITouchGroupInfoProvider> members)
        {
            note.StartPosition = Rotation(note.StartPosition);
            var sensorPos = TouchBase.GetSensor(note.TouchArea, note.StartPosition);
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
            if (appearTiming < -5f)
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
        TouchHoldPoolingInfo CreateTouchHold(in SimaiNote note, 
                                             in SimaiTimingPoint timing,
                                             in List<ITouchGroupInfoProvider> members)
        {
            note.StartPosition = Rotation(note.StartPosition);
            var sensorPos = TouchBase.GetSensor(note.TouchArea, note.StartPosition);
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
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);

            _touchSortOrder -= NOTE_LAYER_COUNT[note.Type];
            sensorPos = Rotation(sensorPos);
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
        async Task AllocTouchGroup(List<ITouchGroupInfoProvider> members)
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
                        var currentArea = groupMembers[i];
                        var nearbyArea = TOUCH_GROUPS[currentArea];
                        for(var j = 0;j < sensorTypes.Count;j++)
                        {
                            var area = sensorTypes[j];
                            if (groupMembers.Contains(area))
                                continue;
                            else if(nearbyArea.Contains(area))
                                groupMembers.Add(area);
                        }
                    }

                    foreach(var area in groupMembers)
                        sensorTypes.Remove(area);

                    sensorGroups.Add(groupMembers);
                    //var sensorType = sensorTypes[0];
                    //var existsGroup = sensorGroups.FindAll(x => x.Contains(sensorType));
                    //var groupTable = TOUCH_GROUPS[sensorType];
                    //existsGroup.AddRange(sensorGroups.FindAll(x => x.Any(y => groupTable.Contains(y))));

                    //var groupMembers = existsGroup.SelectMany(x => x)
                    //                              .ToList();
                    //var newMembers = sensorTypes.FindAll(x => groupTable.Contains(x));

                    //groupMembers.AddRange(newMembers);
                    //groupMembers.Add(sensorType);
                    //var newGroup = groupMembers.GroupBy(x => x)
                    //                           .Select(x => x.Key)
                    //                           .ToList();

                    //foreach (var newMember in newGroup)
                    //    sensorTypes.Remove(newMember);
                    //foreach (var oldGroup in existsGroup)
                    //    sensorGroups.Remove(oldGroup);

                    //sensorGroups.Add(newGroup);
                }
                List<TouchGroup> touchGroups = new();
                var memberMapping = members.ToDictionary(x => x.SensorPos);
                foreach (var group in sensorGroups)
                {
                    touchGroups.Add(new TouchGroup()
                    {
                        Members = group.Select(x => memberMapping[x]).ToArray()
                    });
                }
                foreach (var member in members)
                    member.GroupInfo = touchGroups.Find(x => x.Members.Any(y => y == member));
            });
        }
        private async ValueTask CountNoteSumAsync(SimaiChart chart)
        {
            await Task.Run(() =>
            {
                foreach (var timing in chart.NoteTimings)
                    foreach (var note in timing.Notes)
                        if (!note.IsBreak)
                        {
                            if (note.Type == SimaiNoteType.Tap) _objectCounter.tapSum++;
                            if (note.Type == SimaiNoteType.Hold) _objectCounter.holdSum++;
                            if (note.Type == SimaiNoteType.TouchHold) _objectCounter.holdSum++;
                            if (note.Type == SimaiNoteType.Touch) _objectCounter.touchSum++;
                            if (note.Type == SimaiNoteType.Slide)
                            {
                                if (!note.IsSlideNoHead) _objectCounter.tapSum++;
                                if (note.IsSlideBreak)
                                    _objectCounter.breakSum++;
                                else
                                    _objectCounter.slideSum++;
                            }
                        }
                        else
                        {
                            if (note.Type == SimaiNoteType.Slide)
                            {
                                if (!note.IsSlideNoHead) _objectCounter.breakSum++;
                                if (note.IsSlideBreak)
                                    _objectCounter.breakSum++;
                                else
                                    _objectCounter.slideSum++;
                            }
                            else
                            {
                                _objectCounter.breakSum++;
                            }
                        }
                _objectCounter._totalDXScore = (_objectCounter.tapSum + _objectCounter.holdSum + _objectCounter.touchSum + _objectCounter.slideSum + _objectCounter.breakSum) * 3;
            });
        }
        private void CreateSlideGroup(SimaiTimingPoint timing, SimaiNote note, in List<NotePoolingInfo?> eachNotes)
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

                    string slideShape = detectShapeFromText(slidePart.RawContent);
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
                        throw new InvalidOperationException("不允许Wifi Slide作为Connection Slide的一部分");
                    var result = CreateWifi(timing, subSlide[i]);
                    sliObj = result.SlideInstance;
                    eachNotes.Add(result.StarInfo);
                    AddSlideToQueue(timing, result.SlideInstance);
                    UpdateStarRotateSpeed(result, (float)subSlide[i].SlideTime, 8.93760109f);
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
                    var result = CreateSlide(timing, subSlide[i], info);
                    parent = result.SlideInstance;
                    sliObj = result.SlideInstance;
                    eachNotes.Add(result.StarInfo);
                    subSlides.Add(result.SlideInstance);
                    if(i == 0)
                        slideResult = result;
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
                    judgeQueueLen += table!.JudgeQueue.Length;
                else
                    judgeQueueLen += table!.JudgeQueue.Length - 1;
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
        void UpdateStarRotateSpeed<T>(CreateSlideResult<T> result,float totalLen,float totalSlideLen) where T: SlideBase
        {
            var speed = totalSlideLen / (totalLen * 1000);
            var ratio = speed / 0.0034803742562305f;

            if (result.StarInfo is not null)
            {
                var starInfo = result.StarInfo;
                starInfo.RotateSpeed = Math.Max(-(68.54838709677419f) * ratio,-1080);
            }
        }
        void AddSlideToQueue<T>(SimaiTimingPoint timing,T SliCompo) where T :SlideBase
        {
            var speed = NoteSpeed * timing.HSpeed;
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var slideFadeInTiming = Math.Max((-3.926913f / speed) + MajInstances.Setting.Game.SlideFadeInOffset + (float)timing.Timing, -5f);
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
            var appearTiming = (float)timing.Timing + appearDiff;
            _slideQueueInfos.Add(new()
            {
                Index = _slideIndex++,
                SlideObject = SliCompo,
                AppearTiming = Math.Min(appearTiming, slideFadeInTiming)
            });
        }
        private CreateSlideResult<SlideDrop> CreateSlide(SimaiTimingPoint timing, SubSlideNote note, ConnSlideInfo info)
        {
            string slideShape = detectShapeFromText(note.RawContent);
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
            var isJustR = detectJustType(note.RawContent, out int endPos);
            var startPos = note.StartPosition;

            //slide_star.SetActive(true);
            slide.SetActive(true);
            startPos = Rotation(startPos);
            endPos = Rotation(endPos);

            TapPoolingInfo? starInfo = null;
            if(!note.IsSlideNoHead)
            {
                var _info = CreateStar(note, timing);
                _poolManager.AddTap(_info);
                starInfo = _info;
            }

            //SliCompo.SlideType = slideShape;

            if (timing.Notes.Length > 1)
            {
                var slides = timing.Notes.FindAll(o => o.Type == SimaiNoteType.Slide);
                var index = slides.FindIndex(x => x == note.Origin) + 1;
                if (slides.Length > 1)
                {
                    isEach = true;
                    if (_gpManager.IsClassicMode)
                    {
                        if (index == slides.Length && index % 2 != 0)
                            isEach = false;
                    }
                }
            }

            SliCompo.ConnectInfo = info;
            SliCompo.IsBreak = note.IsSlideBreak;
            SliCompo.IsEach = isEach;
            SliCompo.IsMirror = isMirror;
            SliCompo.IsJustR = isJustR;
            SliCompo.EndPos = endPos;
            SliCompo.Speed = Math.Abs(NoteSpeed * timing.HSpeed);
            SliCompo.StartTiming = (float)note.SlideStartTime;
            SliCompo.StartPos = startPos;
            //SliCompo._stars = new GameObject[] { slide_star };
            SliCompo.Timing = (float)timing.Timing;
            SliCompo.Length = (float)note.SlideTime;
            //SliCompo.sortIndex = -7000 + (int)((lastNoteTime - timing.Timing) * -100) + sort * 5;
            if(MajInstances.Setting.Display.SlideSortOrder == JudgeMode.Classic)
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
                StarInfo = starInfo
            };
        }
        private CreateSlideResult<WifiDrop> CreateWifi(SimaiTimingPoint timing, SubSlideNote note)
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
            var isJustR = detectJustType(note.RawContent, out endPos);

            startPos = Rotation(startPos);
            endPos = Rotation(endPos);
            slideWifi.SetActive(true);

            TapPoolingInfo? starInfo = null;
            if (!note.IsSlideNoHead)
            {
                var _info = CreateStar(note, timing);
                _poolManager.AddTap(_info);
                starInfo = _info;
            }

            if (timing.Notes.Length > 1)
            {
                var slides = timing.Notes.FindAll(o => o.Type == SimaiNoteType.Slide);
                var index = slides.FindIndex(x => x == note.Origin) + 1;
                if (slides.Length > 1)
                {
                    isEach = true;
                    if(_gpManager.IsClassicMode)
                    {
                        if(index == slides.Length && index % 2 != 0)
                            isEach = false;
                    }
                }
            }

            WifiCompo.IsBreak = note.IsSlideBreak;
            WifiCompo.IsEach = isEach;
            WifiCompo.IsJustR = isJustR;
            WifiCompo.EndPos = endPos;
            WifiCompo.Speed = Math.Abs(NoteSpeed * timing.HSpeed);
            WifiCompo.StartTiming = (float)note.SlideStartTime;
            WifiCompo.StartPos = startPos;
            WifiCompo.Timing = (float)timing.Timing;
            WifiCompo.Length = (float)note.SlideTime;
            //var centerStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            //var leftStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            //var rightStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            //WifiCompo._stars = new GameObject[3]
            //{
            //    rightStar,
            //    centerStar,
            //    leftStar
            //};
            if (MajInstances.Setting.Display.SlideSortOrder == JudgeMode.Classic)
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
                StarInfo = starInfo
            };
        }
        /// <summary>
        /// 判断Slide SlideOK是否需要镜像翻转
        /// </summary>
        /// <param name="content"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        private bool detectJustType(string content, out int endPos)
        {
            // > < ^ V w
            if (content.Contains('>'))
            {
                var str = content.Substring(0, 3);
                var digits = str.Split('>');
                var startPos = int.Parse(digits[0]);
                endPos = int.Parse(digits[1]);

                if (isUpperHalf(startPos))
                    return true;
                return false;
            }

            if (content.Contains('<'))
            {
                var str = content.Substring(0, 3);
                var digits = str.Split('<');
                var startPos = int.Parse(digits[0]);
                endPos = int.Parse(digits[1]);

                if (!isUpperHalf(startPos))
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

                if (isRightHalf(endPos))
                    return true;
                return false;
            }
            else if (content.Contains('w'))
            {
                var str = content.Substring(0, 3);
                endPos = int.Parse(str.Substring(2, 1));
                if (isUpperHalf(endPos))
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
                if (isRightHalf(endPos))
                    return true;
                return false;
            }
            return true;
        }

        private string detectShapeFromText(string content)
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
                if (isUpperHalf(startPos))
                {
                    return "circle" + endPos;
                }

                endPos = MirrorKeys(endPos);
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
                if (!isUpperHalf(startPos))
                {
                    return "circle" + endPos;
                }

                endPos = MirrorKeys(endPos);
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
                    return "-circle" + MirrorKeys(endPos);
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
                endPos = MirrorKeys(endPos);
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
                endPos = MirrorKeys(endPos);
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
                    return "-L" + MirrorKeys(endPos);
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

        private bool isUpperHalf(int key)
        {
            if (key == 7) return true;
            if (key == 8) return true;
            if (key == 1) return true;
            if (key == 2) return true;

            return false;
        }

        private bool isRightHalf(int key)
        {
            if (key == 1) return true;
            if (key == 2) return true;
            if (key == 3) return true;
            if (key == 4) return true;

            return false;
        }

        private int MirrorKeys(int key)
        {
            if (key == 1) return 1;
            if (key == 2) return 8;
            if (key == 3) return 7;
            if (key == 4) return 6;

            if (key == 5) return 5;
            if (key == 6) return 4;
            if (key == 7) return 3;
            if (key == 8) return 2;
            throw new Exception("Keys out of range: " + key);
        }
        int Rotation(int keyIndex)
        {
            if (!keyIndex.InRange(1, 8))
                throw new ArgumentOutOfRangeException();
            var key = (SensorArea)(keyIndex - 1);
            var newKey = key.Diff(ChartRotation);
            return newKey.GetIndex();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        SensorArea Rotation(SensorArea sensorIndex)
        {
            return sensorIndex.Diff(ChartRotation);
        }
        class SubSlideNote : SimaiNote
        {
            public SimaiNote Origin { get; set; } = new();
        }
        readonly struct CreateSlideResult<T> where T : SlideBase
        {
            public T SlideInstance { get; init; }
            public TapPoolingInfo? StarInfo { get; init; }
        }
    }
}