using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Setting
{
    public class TapPreviewController : MonoBehaviour
    {
        public bool Active { get; set; } = false;

        [SerializeField]
        GameObject _tapObject;
        [SerializeField]
        Image _tapImage;

        float _timing = 0;
        float _speed = 0;
        float _appearTiming = 0;
        float _arriveTiming = 0;
        bool _active = false;
        NoteStatus _state = NoteStatus.Initialized;
        GameSetting _setting = MajInstances.Setting;
        SkinManager _skinManager = MajInstances.SkinManager;
        void Start()
        {
            var tapSpeed = Math.Abs(_setting.Game.TapSpeed);
            var speed = (float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f)));
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);

            _speed = speed;
            _tapImage.sprite = _skinManager.GetTapSkin().Normal;
            _appearTiming = appearDiff;
            _arriveTiming = (4.8f / _speed) - _appearTiming;
        }
        void Update()
        {
            var timing = _timing - _arriveTiming;
            var distance = timing * _speed + 4.8f;
            var scaleRate = _setting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));
            destScale *= 0.7f;

            switch (_state)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        var pos = _tapObject.transform.localPosition;
                        pos.x = -90;
                        _tapObject.transform.localPosition = pos;
                        _state = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    else
                        _tapObject.transform.localScale = new Vector3(0, 0);
                    break;
                case NoteStatus.Scaling:
                    {
                        if (distance < 1.225f)
                        {
                            _tapObject.transform.localScale = new Vector3(destScale, destScale);
                            var pos = _tapObject.transform.localPosition;
                            pos.x = -90;
                            _tapObject.transform.localPosition = pos;
                        }
                        else
                        {
                            _state = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                    }
                    break;
                case NoteStatus.Running:
                    {
                        var pos = _tapObject.transform.localPosition;
                        pos.x = -90 + GetDistance(distance);
                        _tapObject.transform.localPosition = pos;
                        _tapObject.transform.localScale = new Vector3(0.7f, 0.7f);
                        if (_timing > _arriveTiming)
                            _timing = 0;
                    }
                    break;
            }
            _timing += Time.deltaTime;
        }
        float GetDistance(float dis)
        {
            if (dis < 1.225f)
                return 0;
            var diff = 4.8f - 1.225f;
            var delta = dis - 1.225f;
            var p = (delta / diff).Clamp(0,1);
            var _diff = 152 - -90;

            return _diff * p;
        }
        public void Refresh()
        {
            var tapSpeed = Math.Abs(_setting.Game.TapSpeed);
            var speed = (float)(107.25 / (71.4184491 * Mathf.Pow(tapSpeed + 0.9975f, -0.985558604f)));
            var scaleRate = MajInstances.Setting.Debug.NoteAppearRate;
            var appearDiff = (-(1 - (scaleRate * 1.225f)) - (4.8f * scaleRate)) / (Math.Abs(speed) * scaleRate);

            _speed = speed;
            _tapImage.sprite = _skinManager.GetTapSkin().Normal;
            _appearTiming = appearDiff;
            _arriveTiming = (4.8f / _speed) - _appearTiming;
            _timing = 0;
        }
    }
}