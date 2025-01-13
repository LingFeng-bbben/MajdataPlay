using MajdataPlay.Extensions;
using MajdataPlay.Game.Buffers;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Game.Types;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TouchHoldDrop : NoteLongDrop, INoteQueueMember<TouchQueueInfo>, IRendererContainer,IPoolableNote<TouchHoldPoolingInfo, TouchQueueInfo>
    {
        public TouchGroup? GroupInfo { get; set; } = null;
        public TouchQueueInfo QueueInfo { get; set; } = TouchQueueInfo.Default;
        public RendererStatus RendererState
        {
            get => _rendererState;
            set
            {
                if (State < NoteStatus.Initialized)
                    return;

                switch (value)
                {
                    case RendererStatus.Off:
                        foreach (var renderer in _fanRenderers)
                            renderer.forceRenderingOff = true;
                        _borderRenderer.forceRenderingOff = true;
                        _borderMask.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        foreach (var renderer in _fanRenderers)
                            renderer.forceRenderingOff = false;
                        _borderRenderer.forceRenderingOff = false;
                        _borderMask.forceRenderingOff = false;
                        break;
                    default:
                        return;
                }
                _rendererState = value;
            }
        }
        public char areaPosition;
        public bool isFirework;

        Sprite board_On;
        Sprite board_Off;

        readonly GameObject[] _fans = new GameObject[4];
        readonly Transform[] _fanTransforms = new Transform[4];
        readonly SpriteRenderer[] _fanRenderers = new SpriteRenderer[4];

        float displayDuration;
        float moveDuration;
        float wholeDuration;

        GameObject _pointObject;
        GameObject _borderObject;
        SpriteMask _borderMask;
        SpriteRenderer _pointRenderer;
        SpriteRenderer _borderRenderer;
        NotePoolManager _notePoolManager;

        const int _fanSpriteSortOrder = 2;
        const int _borderSortOrder = 6;
        const int _pointBorderSortOrder = 1;

        protected override void Awake()
        {
            base.Awake();
            _notePoolManager = FindObjectOfType<NotePoolManager>();

            _fanTransforms[0] = Transform.GetChild(5);
            _fanTransforms[1] = Transform.GetChild(4);
            _fanTransforms[2] = Transform.GetChild(3);
            _fanTransforms[3] = Transform.GetChild(2);

            _fans[0] = _fanTransforms[0].gameObject;
            _fans[1] = _fanTransforms[1].gameObject;
            _fans[2] = _fanTransforms[2].gameObject;
            _fans[3] = _fanTransforms[3].gameObject;

            for (var i = 0; i < 4; i++)
            {
                _fanRenderers[i] = _fans[i].GetComponent<SpriteRenderer>();
            }

            _pointObject = transform.GetChild(6).gameObject;
            _borderObject = transform.GetChild(1).gameObject;
            _pointRenderer = _pointObject.GetComponent<SpriteRenderer>();
            _borderRenderer = _borderObject.GetComponent<SpriteRenderer>();
            _borderMask = Transform.GetChild(0).GetComponent<SpriteMask>();

            _pointObject.SetActive(true);
            _borderObject.SetActive(true);

            Transform.position = new Vector3(0, 0, 0);
            SetFansColor(new Color(1f, 1f, 1f, 0f));
            SetFansPosition(0.4f);

            base.SetActive(false);
            SetFanActive(false);
            SetBorderActive(false);
            SetPointActive(false);
            Active = false;
            RendererState = RendererStatus.Off;
        }
        protected override async void Autoplay()
        {
            while (!_isJudged)
            {
                if (_gpManager is null)
                    return;
                else if (GetTimeSpanToJudgeTiming() >= 0)
                {
                    var autoplayParam = _gpManager.AutoplayParam;
                    if (autoplayParam.InRange(0, 14))
                        _judgeResult = (JudgeGrade)autoplayParam;
                    else
                        _judgeResult = (JudgeGrade)_randomizer.Next(0, 15);
                    ConvertJudgeGrade(ref _judgeResult);
                    _isJudged = true;
                    _judgeDiff = _judgeResult switch
                    {
                        < JudgeGrade.Perfect => 1,
                        > JudgeGrade.Perfect => -1,
                        _ => 0
                    };
                    PlayJudgeSFX(new JudgeResult()
                    {
                        Grade = _judgeResult,
                        IsBreak = IsBreak,
                        IsEX = IsEX,
                        Diff = _judgeDiff
                    });
                    PlayHoldEffect();
                }
                await Task.Delay(1);
            }
        }
        public void Initialize(TouchHoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.Destroyed)
                return;

            StartPos = poolingInfo.StartPos;
            areaPosition = poolingInfo.AreaPos;
            Timing = poolingInfo.Timing;
            _judgeTiming = Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            GroupInfo = poolingInfo.GroupInfo;
            _isJudged = false;
            Length = poolingInfo.LastFor;
            isFirework = poolingInfo.IsFirework;
            _sensorPos = poolingInfo.SensorPos;
            _playerIdleTime = 0;
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.316667f, ContainsType.Closed);

            wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            LoadSkin();

            SetFansColor(new Color(1f, 1f, 1f, 0f));
            _borderMask.enabled = false;
            _borderMask.alphaCutoff = 0;
            SetActive(true);
            SetFanActive(false);
            SetBorderActive(false);
            SetPointActive(false);

            _sensorPos = TouchBase.GetSensor(areaPosition, StartPos);
            var pos = TouchBase.GetAreaPos(_sensorPos);
            transform.position = pos;
            SetFansPosition(0.4f);
            RendererState = RendererStatus.Off;

            for (var i = 0; i < 4; i++)
                _fanRenderers[i].sortingOrder = SortOrder - (_fanSpriteSortOrder + i);
            _pointRenderer.sortingOrder = SortOrder - _pointBorderSortOrder;
            _borderRenderer.sortingOrder = SortOrder - _borderSortOrder;
            _borderMask.frontSortingOrder = SortOrder - _borderSortOrder;
            _borderMask.backSortingOrder = SortOrder - _borderSortOrder - 1;

            if (_gpManager.IsAutoplay)
                Autoplay();
            else
                SubscribeEvent();

            State = NoteStatus.Initialized;
        }
        public void End(bool forceEnd = false)
        {
            State = NoteStatus.Destroyed;
            UnsubscribeEvent();
            if (forceEnd)
                return;
            _judgeResult = EndJudge(_judgeResult);
            ConvertJudgeGrade(ref _judgeResult);
            var result = new JudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff,
            };
            //_pointObject.SetActive(false);
            SetActive(false);
            RendererState = RendererStatus.Off;

            _objectCounter.ReportResult(this, result);
            if (!_isJudged)
                _noteManager.NextTouch(QueueInfo);
            if (isFirework && !result.IsMissOrTooFast)
                _effectManager.PlayFireworkEffect(transform.position);

            PlayJudgeSFX(new JudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = false,
                IsEX = false,
                Diff = _judgeDiff
            });
            _audioEffMana.StopTouchHoldSound();
            _effectManager.PlayTouchHoldEffect(_sensorPos, result);
            _effectManager.ResetHoldEffect(_sensorPos);
            _notePoolManager.Collect(this);
        }

        void Check()
        {
            if (_isJudged)
                return;
            else if (!_noteManager.CanJudge(QueueInfo))
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.316667f;
            
            if (_judgableRange.InRange(_gpManager.ThisFrameSec))
            {
                var sensorState = _noteManager.GetSensorStateInThisFrame(_sensorPos);

                Check(sensorState, ref _noteManager.IsSensorUsedInThisFrame(_sensorPos));
                if (!_isJudged && GroupInfo is not null)
                {
                    if (GroupInfo.Percent > 0.5f && GroupInfo.JudgeResult != null)
                    {
                        _isJudged = true;
                        _judgeResult = (JudgeGrade)GroupInfo.JudgeResult;
                        _judgeDiff = GroupInfo.JudgeDiff;
                    }
                }
            }
            else if (isTooLate)
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                _judgeDiff = 316.667f;
            }

            if (_isJudged)
            {
                _noteManager.NextTouch(QueueInfo);
                if (GroupInfo is not null && !_judgeResult.IsMissOrTooFast())
                {
                    GroupInfo.JudgeResult = _judgeResult;
                    GroupInfo.JudgeDiff = _judgeDiff;
                    GroupInfo.RegisterResult(_judgeResult);
                }
            }
        }
        void Check(in InputEventArgs args, ref bool isUsedInThisFrame)
        {
            if (_isJudged)
                return;
            else if (!args.IsClick)
                return;
            else if (isUsedInThisFrame)
                return;

            var thisFrameSec = _gpManager.ThisFrameSec;
            isUsedInThisFrame = true;

            Judge(thisFrameSec);
        }
        void BodyCheck()
        {
            if (!_isJudged)
                return;

            var remainingTime = GetRemainingTime();
            var timing = GetTimeSpanToJudgeTiming();

            if (remainingTime == 0)
            {
                End();
                return;
            }

            if (timing <= 0.25f) // 忽略头部15帧
                return;
            else if (remainingTime <= 0.2f) // 忽略尾部12帧
                return;
            else if (!_gpManager.IsStart) // 忽略暂停
                return;

            var on = _noteManager.CheckSensorStateInThisFrame(_sensorPos, SensorStatus.On);
            if (on || _gpManager.IsAutoplay)
            {
                PlayHoldEffect();
            }
            else
            {
                _playerIdleTime += Time.deltaTime;
                StopHoldEffect();
            }
        }
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTouchHoldSkin();

            SetFansMaterial(DefaultMaterial);
            if(IsBreak)
            {
                for (var i = 0; i < 4; i++)
                    _fanRenderers[i].sprite = skin.Fans_Break[i];
                _borderRenderer.sprite = skin.Boader_Break; // TouchHold Border
                _pointRenderer.sprite = skin.Point_Break;
                board_On = skin.Boader_Break;
                SetFansMaterial(BreakMaterial);
            }
            else
            {
                for (var i = 0; i < 4; i++)
                    _fanRenderers[i].sprite = skin.Fans[i];
                _borderRenderer.sprite = skin.Boader; // TouchHold Border
                _pointRenderer.sprite = skin.Point;
                board_On = skin.Boader;
            }
            board_Off = skin.Off;
        }
        protected override void Judge(float currentSec)
        {
            const float JUDGE_GOOD_AREA = 316.667f;
            const int JUDGE_GREAT_AREA = 250;
            const int JUDGE_PERFECT_AREA = 200;

            const float JUDGE_SEG_PERFECT1 = 150f;
            const float JUDGE_SEG_PERFECT2 = 175f;
            const float JUDGE_SEG_GREAT1 = 216.6667f;
            const float JUDGE_SEG_GREAT2 = 233.3334f;

            if (_isJudged)
                return;

            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            _judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);

            if (diff > JUDGE_SEG_PERFECT1 && isFast)
                return;

            JudgeGrade result = diff switch
            {
                < JUDGE_SEG_PERFECT1 => JudgeGrade.Perfect,
                < JUDGE_SEG_PERFECT2 => JudgeGrade.LatePerfect1,
                < JUDGE_PERFECT_AREA => JudgeGrade.LatePerfect2,
                < JUDGE_SEG_GREAT1 => JudgeGrade.LateGreat,
                < JUDGE_SEG_GREAT2 => JudgeGrade.LateGreat1,
                < JUDGE_GREAT_AREA => JudgeGrade.LateGreat2,
                < JUDGE_GOOD_AREA => JudgeGrade.LateGood,
                _ => JudgeGrade.Miss
            };

            ConvertJudgeGrade(ref result);
            _judgeResult = result;
            _isJudged = true;
            PlayHoldEffect();
        }
        public override void ComponentFixedUpdate()
        {

        }
        public override void ComponentUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();

            Check();
            BodyCheck();

            switch(State)
            {
                case NoteStatus.Initialized:
                    if (-timing < wholeDuration)
                    {
                        SetPointActive(true);
                        SetFanActive(true);
                        RendererState = RendererStatus.On;
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    return;
                case NoteStatus.Scaling:
                    {
                        var newColor = Color.white;
                        if (-timing < moveDuration)
                        {
                            SetFansColor(Color.white);
                            State = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                        var alpha = ((wholeDuration + timing) / displayDuration).Clamp(0, 1);
                        newColor.a = alpha;
                        SetFansColor(newColor);
                    }
                    return;
                case NoteStatus.Running:
                    {
                        var pow = -Mathf.Exp(8 * (timing * 0.43f / moveDuration) - 0.85f) + 0.42f;
                        var distance = Mathf.Clamp(pow, 0f, 0.4f);
                        if (float.IsNaN(distance))
                            distance = 0f;
                        if (timing >= 0)
                        {
                            var _pow = -Mathf.Exp(-0.85f) + 0.42f;
                            var _distance = Mathf.Clamp(_pow, 0f, 0.4f);
                            SetFansPosition(_distance);
                            SetBorderActive(true);
                            _borderMask.enabled = true;
                            State = NoteStatus.End;
                            goto case NoteStatus.End;
                        }
                        else
                            SetFansPosition(distance);
                    }
                    return;
                case NoteStatus.End:
                    {
                        var value = 0.91f * (1 - (Length - timing) / Length);
                        var alpha = value.Clamp(0, 1f);
                        _borderMask.alphaCutoff = alpha;
                    }
                    return;
            }   
        }

        public override void SetActive(bool state)
        {
            if (Active == state)
                return;
            base.SetActive(state);
            SetFanActive(state);
            SetBorderActive(state);
            SetPointActive(state);
            Active = state;
        }
        void SetFanActive(bool state)
        {
            switch (state)
            {
                case true:
                    foreach (var fanObj in _fans.AsSpan())
                    {
                        fanObj.layer = MajEnv.DEFAULT_LAYER;
                    }
                    break;
                case false:
                    foreach (var fanObj in _fans.AsSpan())
                    {
                        fanObj.layer = MajEnv.HIDDEN_LAYER;
                    }
                    break;
            }
        }
        void SetPointActive(bool state)
        {
            switch (state)
            {
                case true:
                    _pointObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _pointObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
        void SetBorderActive(bool state)
        {
            switch (state)
            {
                case true:
                    _borderObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _borderObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }

        void SetFansPosition(in float distance)
        {
            for (var i = 0; i < 4; i++)
            {
                var pos = (0.226f + distance) * GetAngle(i);
                _fanTransforms[i].localPosition = pos;
            }
        }
        JudgeGrade EndJudge(in JudgeGrade result)
        {
            if (!_isJudged) 
                return result;
            var offset = (int)result > 7 ? 0 : _judgeDiff;
            var realityHT = (Length - 0.45f - offset / 1000f).Clamp(0, Length - 0.45f);
            var percent = ((realityHT - _playerIdleTime) / realityHT).Clamp(0, 1);

            if (realityHT > 0)
            {
                if (percent >= 1f)
                {
                    if (result.IsMissOrTooFast())
                        return JudgeGrade.LateGood;
                    else if (MathF.Abs((int)result - 7) == 6)
                        return (int)result < 7 ? JudgeGrade.LateGreat : JudgeGrade.FastGreat;
                    else
                        return result;
                }
                else if (percent >= 0.67f)
                {
                    if (result.IsMissOrTooFast())
                        return JudgeGrade.LateGood;
                    else if (MathF.Abs((int)result - 7) == 6)
                        return (int)result < 7 ? JudgeGrade.LateGreat : JudgeGrade.FastGreat;
                    else if (result == JudgeGrade.Perfect)
                        return (int)result < 7 ? JudgeGrade.LatePerfect1 : JudgeGrade.FastPerfect1;
                }
                else if (percent >= 0.33f)
                {
                    if (MathF.Abs((int)result - 7) >= 6)
                        return (int)result < 7 ? JudgeGrade.LateGood : JudgeGrade.FastGood;
                    else
                        return (int)result < 7 ? JudgeGrade.LateGreat : JudgeGrade.FastGreat;
                }
                else if (percent >= 0.05f)
                    return (int)result < 7 ? JudgeGrade.LateGood : JudgeGrade.FastGood;
                else if (percent >= 0)
                {
                    if (result.IsMissOrTooFast())
                        return JudgeGrade.Miss;
                    else
                        return (int)result < 7 ? JudgeGrade.LateGood : JudgeGrade.FastGood;
                }
            }
            MajDebug.Log($"TouchHold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
            return result;
        }
        void PlayHoldEffect()
        {
            //var r = MajInstances.AudioManager.GetSFX("touch_Hold_riser.wav");
            //MajDebug.Log($"IsPlaying:{r.IsPlaying}\nCurrent second: {r.CurrentSec}s");
            _effectManager.PlayHoldEffect(_sensorPos, _judgeResult);
            _audioEffMana.PlayTouchHoldSound();
            _borderRenderer.sprite = board_On;
            SetFansMaterial(DefaultMaterial);
        }
        void StopHoldEffect()
        {
            _effectManager.ResetHoldEffect(_sensorPos);
            _audioEffMana.StopTouchHoldSound();
            _borderRenderer.sprite = board_Off;
            SetFansMaterial(DefaultMaterial);
        }
        Vector3 GetAngle(int index)
        {
            var angle = Mathf.PI / 4 + index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void SetFansColor(Color color)
        {
            foreach (var fan in _fanRenderers.AsSpan()) 
                fan.color = color;
        }
        void SetFansMaterial(Material material)
        {
            for (var i = 0; i < 4; i++)
                _fanRenderers[i].sharedMaterial = material;
        }
        void SubscribeEvent()
        {
            //_ioManager.BindSensor(_noteChecker, _sensorPos);
        }
        void UnsubscribeEvent()
        {
            //_ioManager.UnbindSensor(_noteChecker, _sensorPos);
        }
        protected override void PlaySFX()
        {
            _audioEffMana.PlayTouchHoldSound();
        }
        protected override void PlayJudgeSFX(in JudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast)
                return;
            _audioEffMana.PlayTapSound(judgeResult);
            if (isFirework)
                _audioEffMana.PlayHanabiSound();
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}