using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MajSimaiDecode;
using MajdataPlay.Types;
using MajdataPlay.Game.Notes;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.Utils;
using System.Runtime.CompilerServices;

namespace MajdataPlay.Game
{
#nullable enable
    public class NoteLoader : MonoBehaviour
    {
        public NoteLoaderStatus State { get; private set; } = NoteLoaderStatus.Idle;
        public long NoteLimit { get; set; } = 1000;
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
        int _noteSortOrder = 0;
        int _touchSortOrder = 0;

        NoteManager _noteManager;
        Dictionary<int, int> _noteIndex = new();
        Dictionary<SensorType, int> _touchIndex = new();

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

        static readonly Dictionary<SensorType, SensorType[]> TOUCH_GROUPS = new()
        {
            { SensorType.A1, new SensorType[]{ SensorType.D1, SensorType.D2, SensorType.E1, SensorType.E2 } },
            { SensorType.A2, new SensorType[]{ SensorType.D2, SensorType.D3, SensorType.E2, SensorType.E3 } },
            { SensorType.A3, new SensorType[]{ SensorType.D3, SensorType.D4, SensorType.E3, SensorType.E4 } },
            { SensorType.A4, new SensorType[]{ SensorType.D4, SensorType.D5, SensorType.E4, SensorType.E5 } },
            { SensorType.A5, new SensorType[]{ SensorType.D5, SensorType.D6, SensorType.E5, SensorType.E6 } },
            { SensorType.A6, new SensorType[]{ SensorType.D6, SensorType.D7, SensorType.E6, SensorType.E7 } },
            { SensorType.A7, new SensorType[]{ SensorType.D7, SensorType.D8, SensorType.E7, SensorType.E8 } },
            { SensorType.A8, new SensorType[]{ SensorType.D8, SensorType.D1, SensorType.E8, SensorType.E1 } },

            { SensorType.D1, new SensorType[]{ SensorType.A1, SensorType.A8, SensorType.E1 } },
            { SensorType.D2, new SensorType[]{ SensorType.A2, SensorType.A1, SensorType.E2 } },
            { SensorType.D3, new SensorType[]{ SensorType.A3, SensorType.A2, SensorType.E3 } },
            { SensorType.D4, new SensorType[]{ SensorType.A4, SensorType.A3, SensorType.E4 } },
            { SensorType.D5, new SensorType[]{ SensorType.A5, SensorType.A4, SensorType.E5 } },
            { SensorType.D6, new SensorType[]{ SensorType.A6, SensorType.A5, SensorType.E6 } },
            { SensorType.D7, new SensorType[]{ SensorType.A7, SensorType.A6, SensorType.E7 } },
            { SensorType.D8, new SensorType[]{ SensorType.A8, SensorType.A7, SensorType.E8 } },

            { SensorType.E1, new SensorType[]{ SensorType.D1, SensorType.A1, SensorType.A8, SensorType.B1, SensorType.B8 } },
            { SensorType.E2, new SensorType[]{ SensorType.D2, SensorType.A2, SensorType.A1, SensorType.B2, SensorType.B1 } },
            { SensorType.E3, new SensorType[]{ SensorType.D3, SensorType.A3, SensorType.A2, SensorType.B3, SensorType.B2 } },
            { SensorType.E4, new SensorType[]{ SensorType.D4, SensorType.A4, SensorType.A3, SensorType.B4, SensorType.B3 } },
            { SensorType.E5, new SensorType[]{ SensorType.D5, SensorType.A5, SensorType.A4, SensorType.B5, SensorType.B4 } },
            { SensorType.E6, new SensorType[]{ SensorType.D6, SensorType.A6, SensorType.A5, SensorType.B6, SensorType.B5 } },
            { SensorType.E7, new SensorType[]{ SensorType.D7, SensorType.A7, SensorType.A6, SensorType.B7, SensorType.B6 } },
            { SensorType.E8, new SensorType[]{ SensorType.D8, SensorType.A8, SensorType.A7, SensorType.B8, SensorType.B7 } },

            { SensorType.B1, new SensorType[]{ SensorType.E1, SensorType.E2, SensorType.B8, SensorType.B2, SensorType.A1, SensorType.C } },
            { SensorType.B2, new SensorType[]{ SensorType.E2, SensorType.E3, SensorType.B1, SensorType.B3, SensorType.A2, SensorType.C } },
            { SensorType.B3, new SensorType[]{ SensorType.E3, SensorType.E4, SensorType.B2, SensorType.B4, SensorType.A3, SensorType.C } },
            { SensorType.B4, new SensorType[]{ SensorType.E4, SensorType.E5, SensorType.B3, SensorType.B5, SensorType.A4, SensorType.C } },
            { SensorType.B5, new SensorType[]{ SensorType.E5, SensorType.E6, SensorType.B4, SensorType.B6, SensorType.A5, SensorType.C } },
            { SensorType.B6, new SensorType[]{ SensorType.E6, SensorType.E7, SensorType.B5, SensorType.B7, SensorType.A6, SensorType.C } },
            { SensorType.B7, new SensorType[]{ SensorType.E7, SensorType.E8, SensorType.B6, SensorType.B8, SensorType.A7, SensorType.C } },
            { SensorType.B8, new SensorType[]{ SensorType.E8, SensorType.E1, SensorType.B7, SensorType.B1, SensorType.A8, SensorType.C } },

            { SensorType.C, new SensorType[]{ SensorType.B1, SensorType.B2, SensorType.B3, SensorType.B4, SensorType.B5, SensorType.B6, SensorType.B7, SensorType.B8} },
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


        private void Start()
        {
            _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
            _noteManager = MajInstanceHelper<NoteManager>.Instance!;
            _poolManager = MajInstanceHelper<NotePoolManager>.Instance!;
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
        }
        public async UniTask LoadNotesIntoPool(SimaiProcess simaiProcess)
        {
            List<Task> touchTasks = new();
            try
            {
                State = NoteLoaderStatus.ParsingNote;
                _noteManager.ResetCounter();
                _noteIndex.Clear();
                _touchIndex.Clear();

                for (int i = 1; i < 9; i++)
                    _noteIndex.Add(i, 0);
                for (int i = 0; i < 33; i++)
                    _touchIndex.Add((SensorType)i, 0);

                var loadedData = simaiProcess;


                await CountNoteSumAsync(loadedData);

                var sum = _objectCounter.tapSum +
                          _objectCounter.holdSum +
                          _objectCounter.touchSum +
                          _objectCounter.breakSum +
                          _objectCounter.slideSum;

                var lastNoteTime = loadedData.notelist.Last().time;

                foreach (var timing in loadedData.notelist)
                {
                    var eachNotes = timing.noteList.FindAll(o => o.noteType != SimaiNoteType.Touch &&
                                                                 o.noteType != SimaiNoteType.TouchHold);
                    int? num = null;
                    touchTasks.Clear();
                    //IDistanceProvider? provider = null;
                    //IStatefulNote? noteA = null;
                    //IStatefulNote? noteB = null;
                    NotePoolingInfo? noteA = null;
                    NotePoolingInfo? noteB = null;
                    List<TouchPoolingInfo> members = new();
                    foreach (var (i, note) in timing.noteList.WithIndex())
                    {
                        _noteCount++;
                        Process = (double)_noteCount / sum;
                        if (_noteCount % 30 == 0)
                            await UniTask.Yield();

                        switch (note.noteType)
                        {
                            case SimaiNoteType.Tap:
                                {
                                    var obj = CreateTap(note, timing);
                                    _poolManager.AddTap(obj);
                                    if (eachNotes.Count > 1 && i < 2)
                                    {
                                        if (num is null)
                                            num = 0;
                                        else if (num != 0)
                                            break;

                                        if (noteA is null)
                                            noteA = obj;
                                        else
                                            noteB = obj;
                                    }
                                }
                                break;
                            case SimaiNoteType.Hold:
                                {
                                    var obj = CreateHold(note, timing);
                                    _poolManager.AddHold(obj);
                                    if (eachNotes.Count > 1 && i < 2)
                                    {
                                        if (num is null)
                                            num = 1;
                                        else if (num != 1)
                                            break;

                                        if (noteA is null)
                                            noteA = obj;
                                        else
                                            noteB = obj;
                                    }
                                }
                                break;
                            case SimaiNoteType.TouchHold:
                                _poolManager.AddTouchHold(CreateTouchHold(note, timing));
                                break;
                            case SimaiNoteType.Touch:
                                _poolManager.AddTouch(CreateTouch(note, timing, members));
                                break;
                            case SimaiNoteType.Slide:
                                CreateSlideGroup(timing, note); // 星星组
                                break;
                        }
                    }
                    if (members.Count != 0)
                        touchTasks.Add(AllocTouchGroup(members));

                    if (eachNotes.Count > 1) //有多个非touchnote
                    {
                        var startPos = eachNotes[0].startPosition;
                        var endPos = eachNotes[1].startPosition;
                        startPos = Rotation(startPos);
                        endPos = Rotation(endPos);
                        endPos = endPos - startPos;
                        if (endPos == 0)
                            continue;
                        var time = (float)timing.time;
                        var speed = NoteSpeed * timing.HSpeed;
                        var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
                        var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (speed * scaleRate);
                        var appearTiming = time + appearDiff;

                        endPos = endPos < 0 ? endPos + 8 : endPos;
                        endPos = endPos > 8 ? endPos - 8 : endPos;
                        endPos++;

                        if (endPos > 4)
                        {
                            startPos = eachNotes[1].startPosition;
                            endPos = eachNotes[0].startPosition;
                            startPos = Rotation(startPos);
                            endPos = Rotation(endPos);
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
            }
            catch (Exception e)
            {
                State = NoteLoaderStatus.Error;
                throw e;
            }
            _poolManager.Initialize();
            State = NoteLoaderStatus.Finished;
        }
        TapPoolingInfo CreateTap(in SimaiNote note, in SimaiTimingPoint timing)
        {
            var startPos = note.startPosition;
            var noteTiming = (float)timing.time;
            var speed = NoteSpeed * timing.HSpeed;
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
            var appearTiming = noteTiming + appearDiff;
            var sortOrder = _noteSortOrder;
            var isEach = timing.noteList.Count > 1;
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            if(isEach)
            {
                var noteCount = timing.noteList.Count;
                var noHeadSlideCount = timing.noteList.FindAll(x => x.noteType == SimaiNoteType.Slide && x.isSlideNoHead).Count;
                if (noteCount - noHeadSlideCount == 1)
                    isEach = false;
            }

            _noteSortOrder -= NOTE_LAYER_COUNT[note.noteType];
            startPos = Rotation(startPos);
            return new()
            {
                StartPos = startPos,
                Timing = noteTiming,
                AppearTiming = appearTiming,
                NoteSortOrder = sortOrder,
                Speed = speed,
                IsEach = isEach,
                IsBreak = note.isBreak,
                IsEX = note.isEx,
                IsStar = note.isForceStar,
                IsNoHead = false,
                IsFakeStar = note.isForceStar,
                IsForceRotate = note.isFakeRotate,
                QueueInfo = new TapQueueInfo()
                {
                    Index = _noteIndex[startPos]++,
                    KeyIndex = startPos
                }
            };
        }
        HoldPoolingInfo CreateHold(in SimaiNote note, in SimaiTimingPoint timing)
        {
            var startPos = note.startPosition;
            var noteTiming = (float)timing.time;
            var speed = Math.Abs(NoteSpeed * timing.HSpeed);
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (speed * scaleRate);
            var appearTiming = noteTiming + appearDiff;
            var sortOrder = _noteSortOrder;
            var isEach = timing.noteList.Count > 1;
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            if (isEach)
            {
                var noteCount = timing.noteList.Count;
                var noHeadSlideCount = timing.noteList.FindAll(x => x.noteType == SimaiNoteType.Slide && x.isSlideNoHead).Count;
                if (noteCount - noHeadSlideCount == 1)
                    isEach = false;
            }
            _noteSortOrder -= NOTE_LAYER_COUNT[note.noteType];
            startPos = Rotation(startPos);
            return new()
            {
                StartPos = startPos,
                Timing = noteTiming,
                LastFor = (float)note.holdTime,
                AppearTiming = appearTiming,
                NoteSortOrder = sortOrder,
                Speed = speed,
                IsEach = isEach,
                IsBreak = note.isBreak,
                IsEX = note.isEx,
                QueueInfo = new TapQueueInfo()
                {
                    Index = _noteIndex[startPos]++,
                    KeyIndex = startPos
                }
            };
        }
        TapPoolingInfo CreateStar(SimaiNote note, in SimaiTimingPoint timing,GameObject slide)
        {
            var startPos = note.startPosition;
            var noteTiming = (float)timing.time;
            var speed = NoteSpeed * timing.HSpeed;
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var slideFadeInTiming = (-3.926913f / speed) + MajInstances.Setting.Game.SlideFadeInOffset + (float)timing.time;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);
            var appearTiming = noteTiming + appearDiff;
            var sortOrder = _noteSortOrder;
            var isEach = timing.noteList.Count > 1;
            bool isDouble = false;
            TapQueueInfo? queueInfo = null;

            appearTiming = Math.Min(appearTiming, slideFadeInTiming);

            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            _noteSortOrder -= NOTE_LAYER_COUNT[note.noteType];

            if(isEach)
            {
                var count = timing.noteList.FindAll(
                    o => o.noteType == SimaiNoteType.Slide &&
                         o.startPosition == startPos).Count;
                if (count > 1)
                {
                    isDouble = true;
                    if (count == timing.noteList.Count)
                        isEach = false;
                    else
                    {
                        var noteCount = timing.noteList.Count;
                        var noHeadSlideCount = timing.noteList.FindAll(x => x.noteType == SimaiNoteType.Slide && x.isSlideNoHead).Count;
                        if (noteCount - noHeadSlideCount == 1)
                            isEach = false;
                    }
                }
            }
            startPos = Rotation(startPos);
            if(!note.isSlideNoHead)
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
                IsBreak = note.isBreak,
                IsEX = note.isEx,
                IsStar = true,
                IsNoHead = note.isSlideNoHead,
                IsFakeStar = false,
                IsForceRotate = false,
                IsDouble = isDouble,
                RotateSpeed = (float)note.slideTime,
                Slide = slide,
                QueueInfo = queueInfo ?? TapQueueInfo.Default
            };
        }
        TouchPoolingInfo CreateTouch(in SimaiNote note,
                                     in SimaiTimingPoint timing,
                                     in List<TouchPoolingInfo> members)
        {
            note.startPosition = Rotation(note.startPosition);
            var sensorPos = TouchBase.GetSensor(note.touchArea, note.startPosition);
            var queueInfo = new TouchQueueInfo()
            {
                SensorPos = sensorPos,
                Index = _touchIndex[sensorPos]++
            };
            var noteTiming = (float)timing.time;
            var areaPosition = note.touchArea;
            var startPosition = note.startPosition;
            var isEach = timing.noteList.Count > 1;
            var isBreak = note.isBreak;
            var speed = TouchSpeed * Math.Abs(timing.HSpeed);
            var isFirework = note.isHanabi;
            var noteSortOrder = _touchSortOrder;
            var moveDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
            var appearTiming = noteTiming - moveDuration;
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);
            _touchSortOrder -= NOTE_LAYER_COUNT[note.noteType];
            if (isEach)
            {
                var noteCount = timing.noteList.Count;
                var noHeadSlideCount = timing.noteList.FindAll(x => x.noteType == SimaiNoteType.Slide && x.isSlideNoHead).Count;
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
        TouchHoldPoolingInfo CreateTouchHold(in SimaiNote note, in SimaiTimingPoint timing)
        {
            note.startPosition = Rotation(note.startPosition);
            var sensorPos = TouchBase.GetSensor(note.touchArea, note.startPosition);
            var queueInfo = new TouchQueueInfo()
            {
                SensorPos = sensorPos,
                Index = _touchIndex[sensorPos]++
            };
            var startPosition = note.startPosition;
            var areaPosition = note.touchArea;
            var noteTiming = (float)timing.time;
            var lastFor = (float)note.holdTime;
            var speed = TouchSpeed * Math.Abs(timing.HSpeed);
            var isFirework = note.isHanabi;
            var isBreak = note.isBreak;
            var moveDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
            var appearTiming = noteTiming - moveDuration;
            var noteSortOrder = _touchSortOrder;
            if (appearTiming < -5f)
                _gpManager.FirstNoteAppearTiming = Mathf.Min(_gpManager.FirstNoteAppearTiming, appearTiming);

            _touchSortOrder -= NOTE_LAYER_COUNT[note.noteType];
            sensorPos = Rotation(sensorPos);
            return new TouchHoldPoolingInfo()
            {
                SensorPos = sensorPos,
                Timing = noteTiming,
                AppearTiming = appearTiming,
                AreaPos = areaPosition,
                StartPos = startPosition,
                Speed = speed,
                IsFirework = isFirework,
                IsEach = false,
                IsBreak = isBreak,
                IsEX = false,
                LastFor = lastFor,
                NoteSortOrder = noteSortOrder,
                QueueInfo = queueInfo,
            };
        }
        async Task AllocTouchGroup(List<TouchPoolingInfo> members)
        {
            await Task.Run(() =>
            {
                var sensorTypes = members.GroupBy(x => x.SensorPos)
                                         .Select(x => x.Key)
                                         .ToList();
                List<List<SensorType>> sensorGroups = new();

                while (sensorTypes.Count > 0)
                {
                    var sensorType = sensorTypes[0];
                    var existsGroup = sensorGroups.FindAll(x => x.Contains(sensorType));
                    var groupMap = TOUCH_GROUPS[sensorType];
                    existsGroup.AddRange(sensorGroups.FindAll(x => x.Any(y => groupMap.Contains(y))));

                    var groupMembers = existsGroup.SelectMany(x => x)
                                                  .ToList();
                    var newMembers = sensorTypes.FindAll(x => groupMap.Contains(x));

                    groupMembers.AddRange(newMembers);
                    groupMembers.Add(sensorType);
                    var newGroup = groupMembers.GroupBy(x => x)
                                               .Select(x => x.Key)
                                               .ToList();

                    foreach (var newMember in newGroup)
                        sensorTypes.Remove(newMember);
                    foreach (var oldGroup in existsGroup)
                        sensorGroups.Remove(oldGroup);

                    sensorGroups.Add(newGroup);
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
        private async ValueTask CountNoteSumAsync(SimaiProcess json)
        {
            await Task.Run(() =>
            {
                foreach (var timing in json.notelist)
                    foreach (var note in timing.noteList)
                        if (!note.isBreak)
                        {
                            if (note.noteType == SimaiNoteType.Tap) _objectCounter.tapSum++;
                            if (note.noteType == SimaiNoteType.Hold) _objectCounter.holdSum++;
                            if (note.noteType == SimaiNoteType.TouchHold) _objectCounter.holdSum++;
                            if (note.noteType == SimaiNoteType.Touch) _objectCounter.touchSum++;
                            if (note.noteType == SimaiNoteType.Slide)
                            {
                                if (!note.isSlideNoHead) _objectCounter.tapSum++;
                                if (note.isSlideBreak)
                                    _objectCounter.breakSum++;
                                else
                                    _objectCounter.slideSum++;
                            }
                        }
                        else
                        {
                            if (note.noteType == SimaiNoteType.Slide)
                            {
                                if (!note.isSlideNoHead) _objectCounter.breakSum++;
                                if (note.isSlideBreak)
                                    _objectCounter.breakSum++;
                                else
                                    _objectCounter.slideSum++;
                            }
                            else
                            {
                                _objectCounter.breakSum++;
                            }
                        }
                _objectCounter.totalDXScore = (_objectCounter.tapSum + _objectCounter.holdSum + _objectCounter.touchSum + _objectCounter.slideSum + _objectCounter.breakSum) * 3;
            });
        }
        private void CreateSlideGroup(SimaiTimingPoint timing, SimaiNote note)
        {
            int charIntParse(char c)
            {
                return c - '0';
            }

            var subSlide = new List<SubSlideNote>();
            var subBarCount = new List<int>();
            var sumBarCount = 0;

            var noteContent = note.noteContent;
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
                    slidePart.noteType = SimaiNoteType.Slide;
                    slidePart.startPosition = latestStartIndex;
                    if (slideTypeChar == "V")
                    {
                        // 转折星星
                        var middlePos = noteContent[ptr++];
                        var endPos = noteContent[ptr++];

                        slidePart.noteContent = latestStartIndex + slideTypeChar + middlePos + endPos;
                        latestStartIndex = charIntParse(endPos);
                    }
                    else
                    {
                        // 其他普通星星
                        // 额外检查pp和qq
                        if (noteContent[ptr] == slideTypeChar[0]) slideTypeChar += noteContent[ptr++];
                        var endPos = noteContent[ptr++];

                        slidePart.noteContent = latestStartIndex + slideTypeChar + endPos;
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
                            slidePart.noteContent += noteContent[ptr++];
                        slidePart.noteContent += noteContent[ptr++];
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

                    string slideShape = detectShapeFromText(slidePart.noteContent);
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
                o.isBreak = note.isBreak;
                o.isEx = note.isEx;
                o.isSlideBreak = note.isSlideBreak;
                o.isSlideNoHead = true;
            });
            subSlide[0].isSlideNoHead = note.isSlideNoHead;

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
                    subSlide[i].slideStartTime = note.slideStartTime + (double)tempBarCount / sumBarCount * note.slideTime;
                    subSlide[i].slideTime = (double)subBarCount[i] / sumBarCount * note.slideTime;
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
                    subSlide[i].slideStartTime = note.slideStartTime + tempSlideTime;
                    subSlide[i].slideTime = getTimeFromBeats(subSlide[i].noteContent, timing.currentBpm);
                    tempSlideTime += subSlide[i].slideTime;
                }
            }

            IConnectableSlide? parent = null;
            List<SlideDrop> subSlides = new();
            float totalLen = (float)subSlide.Select(x => x.slideTime).Sum();
            float startTiming = (float)subSlide[0].slideStartTime;
            float totalSlideLen = 0;
            for (var i = 0; i <= subSlide.Count - 1; i++)
            {
                bool isConn = subSlide.Count != 1;
                bool isGroupHead = i == 0;
                bool isGroupEnd = i == subSlide.Count - 1;
                if (note.noteContent!.Contains('w')) //wifi
                {
                    if (isConn)
                        throw new InvalidOperationException("不允许Wifi Slide作为Connection Slide的一部分");
                    CreateWifi(timing, subSlide[i]);
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
                    parent = CreateSlide(timing, subSlide[i], info);
                    subSlides.Add(parent.GameObject.GetComponent<SlideDrop>());
                }
            }
            long judgeQueueLen = 0;
            var slideCount = subSlides.Count;
            foreach (var (i, s) in subSlides.WithIndex())
            {
                var isFirst = i == 0;
                var isEnd = i == slideCount - 1;
                var table = SlideTables.FindTableByName(s.SlideType);
                
                s.Initialize();
                totalSlideLen += s.GetSlideLength();
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
        }
        private IConnectableSlide CreateSlide(SimaiTimingPoint timing, SubSlideNote note, ConnSlideInfo info)
        {
            string slideShape = detectShapeFromText(note.noteContent);
            var isMirror = false;
            var isEach = false;
            if (slideShape.StartsWith("-"))
            {
                isMirror = true;
                slideShape = slideShape.Substring(1);
            }
            var slideIndex = SLIDE_PREFAB_MAP[slideShape];
            var slide = Instantiate(slidePrefab[slideIndex], notes.transform.GetChild(3));
            var slide_star = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            var SliCompo = slide.AddComponent<SlideDrop>();
            var isJustR = detectJustType(note.noteContent, out int endPos);
            var startPos = note.startPosition;

            slide_star.SetActive(true);
            slide.SetActive(true);
            startPos = Rotation(startPos);
            endPos = Rotation(endPos);

            _poolManager.AddTap(CreateStar(note, timing, slide));
            
            SliCompo.SlideType = slideShape;

            if (timing.noteList.Count > 1)
            {
                var slides = timing.noteList.FindAll(o => o.noteType == SimaiNoteType.Slide);
                var index = slides.FindIndex(x => x == note.Origin) + 1;
                if (slides.Count > 1)
                {
                    isEach = true;
                    if (_gpManager.IsClassicMode)
                    {
                        if (index == slides.Count && index % 2 != 0)
                            isEach = false;
                    }
                }
            }

            SliCompo.ConnectInfo = info;
            SliCompo.IsBreak = note.isSlideBreak;
            SliCompo.IsEach = isEach;
            SliCompo.IsMirror = isMirror;
            SliCompo.IsJustR = isJustR;
            SliCompo.EndPos = endPos;
            if (slideIndex - 26 > 0 && slideIndex - 26 <= 8)
            {
                // known slide sprite issue
                //    1 2 3 4 5 6 7 8
                // p  X X X X X X O O
                // q  X O O X X X X X
                var pqEndPos = slideIndex - 26;
                SliCompo.IsSpecialFlip = isMirror == (pqEndPos == 7 || pqEndPos == 8);
            }
            else
            {
                SliCompo.IsSpecialFlip = isMirror;
            }
            SliCompo.Speed = Math.Abs(NoteSpeed * timing.HSpeed);
            SliCompo.StartTiming = (float)timing.time;
            SliCompo.StartPos = startPos;
            SliCompo._stars = new GameObject[] { slide_star };
            SliCompo.Timing = (float)note.slideStartTime;
            SliCompo.Length = (float)note.slideTime;
            //SliCompo.sortIndex = -7000 + (int)((lastNoteTime - timing.time) * -100) + sort * 5;
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
            return SliCompo;
        }
        int FindSlide(List<SimaiNote> notes,in SimaiNote note)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (note == notes[i])
                    return i;
            }
            return -1;
        }
        private GameObject CreateWifi(SimaiTimingPoint timing, SubSlideNote note)
        {
            var str = note.noteContent.Substring(0, 3);
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
            var isJustR = detectJustType(note.noteContent, out endPos);

            startPos = Rotation(startPos);
            endPos = Rotation(endPos);
            slideWifi.SetActive(true);

            _poolManager.AddTap(CreateStar(note, timing, slideWifi));

            if (timing.noteList.Count > 1)
            {
                var slides = timing.noteList.FindAll(o => o.noteType == SimaiNoteType.Slide);
                var index = slides.FindIndex(x => x == note.Origin) + 1;
                if (slides.Count > 1)
                {
                    isEach = true;
                    if(_gpManager.IsClassicMode)
                    {
                        if(index == slides.Count && index % 2 != 0)
                            isEach = false;
                    }
                }
            }

            WifiCompo.IsBreak = note.isSlideBreak;
            WifiCompo.IsEach = isEach;
            WifiCompo.IsJustR = isJustR;
            WifiCompo.EndPos = endPos;
            WifiCompo.Speed = Math.Abs(NoteSpeed * timing.HSpeed);
            WifiCompo.StartTiming = (float)timing.time;
            WifiCompo.StartPos = startPos;
            WifiCompo.Timing = (float)note.slideStartTime;
            WifiCompo.Length = (float)note.slideTime;
            var centerStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            var leftStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            var rightStar = Instantiate(star_slidePrefab, notes.transform.GetChild(3));
            WifiCompo._stars = new GameObject[3]
            {
                rightStar,
                centerStar,
                leftStar
            };
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

            return slideWifi;
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
            var key = (SensorType)(keyIndex - 1);
            var newKey = key.Diff(ChartRotation);
            return newKey.GetIndex();
        }
        class SubSlideNote : SimaiNote
        {
            public SimaiNote Origin { get; set; } = new();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        SensorType Rotation(SensorType sensorIndex)
        {
            return sensorIndex.Diff(ChartRotation);
        }
    }
}