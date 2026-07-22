using MajdataPlay.IO;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine.UI;

namespace MajdataPlay.UI
{
    public class ButtonAnimation : MonoBehaviour
    {
        public SensorArea[] BindSensor;
        public ButtonZone[] BindButton;

        public float HoldTriggerTime = 0f;   // 按住多久算完成
        public float ReleasedScale = 1f;
        public float PressedScale = 0.9f;
        public float AnimationDuration = 0.1f;
        public Ease EaseType = Ease.OutQuad;

        public Image HoldCircle;            // 圆环 Image

        // 颜色渐变
        public Color StartColor = Color.white;
        public Color EndColor = Color.green;

        private float _holdTimer = 0f;       // 按住计时器
        private MotionHandle _holdMotion;    // 圆环动画句柄

        void Start()
        {
            if (HoldCircle != null)
            {
                HoldCircle.type = Image.Type.Filled;
                HoldCircle.fillMethod = Image.FillMethod.Radial360;
                HoldCircle.fillOrigin = (int)Image.Origin360.Top;
                HoldCircle.fillClockwise = true;

                HoldCircle.fillAmount = 0f;
                HoldCircle.color = StartColor;
            }
        }

        void Update()
        {
            var isDown = false;
            var isUp = false;
            var isPressed = false;

            foreach (var button in BindButton)
            {
                isDown |= InputManager.CheckButtonStatusInPreviousFrame(button, SwitchStatus.Off) &&
                          InputManager.CheckButtonStatusInThisFrame(button, SwitchStatus.On);

                isUp |= InputManager.CheckButtonStatusInPreviousFrame(button, SwitchStatus.On) &&
                        InputManager.CheckButtonStatusInThisFrame(button, SwitchStatus.Off);

                isPressed |= InputManager.CheckButtonStatusInThisFrame(button, SwitchStatus.On);
            }

            foreach (var sensor in BindSensor)
            {
                isDown |= InputManager.CheckSensorStatusInPreviousFrame(sensor, SwitchStatus.Off) &&
                          InputManager.CheckSensorStatusInThisFrame(sensor, SwitchStatus.On);

                isUp |= InputManager.CheckSensorStatusInPreviousFrame(sensor, SwitchStatus.On) &&
                        InputManager.CheckSensorStatusInThisFrame(sensor, SwitchStatus.Off);

                isPressed |= InputManager.CheckSensorStatusInThisFrame(sensor, SwitchStatus.On);
            }

            // -------------------------
            // 按下缩放动画
            // -------------------------
            if (isDown)
            {
                var pressedScale = new Vector3(PressedScale, PressedScale, PressedScale);
                LMotion.Create(transform.localScale, pressedScale, AnimationDuration)
                    .WithEase(EaseType)
                    .BindToLocalScale(transform);
                if (HoldTriggerTime > 0f)
                    _holdTimer = 0f; // 开始计时
            }

            // -------------------------
            // 松开缩放动画 + 圆环清零
            // -------------------------
            if (isUp && !isPressed)
            {
                var releasedScale = new Vector3(ReleasedScale, ReleasedScale, ReleasedScale);
                LMotion.Create(transform.localScale, releasedScale, AnimationDuration)
                    .WithEase(EaseType)
                    .BindToLocalScale(transform);

                if (HoldTriggerTime > 0f)
                {
                    _holdTimer = 0f;

                    if (HoldCircle != null)
                    {
                        if (_holdMotion.IsPlaying() && _holdMotion.IsValid())
                            _holdMotion.Cancel();

                        _holdMotion = LMotion
                            .Create(HoldCircle.fillAmount, 0f, 0.15f)
                            .WithEase(Ease.OutQuad)
                            .Bind(value =>
                            {
                                HoldCircle.fillAmount = value;
                                HoldCircle.color = Color.Lerp(StartColor, EndColor, value);
                            });
                    }
                }
            }

            // -------------------------
            // 按住计时 + 圆环填充（带 LitMotion 缓动 + 颜色渐变）
            // -------------------------
            if (isPressed && HoldTriggerTime > 0f)
            {
                _holdTimer += Time.deltaTime;

                float targetFill = Mathf.Clamp01(_holdTimer / HoldTriggerTime);

                if (HoldCircle != null)
                {
                    if (_holdMotion.IsPlaying() && _holdMotion.IsValid())
                        _holdMotion.Cancel();

                    _holdMotion = LMotion
                        .Create(HoldCircle.fillAmount, targetFill, 0.15f)
                        .WithEase(Ease.OutQuad)
                        .Bind(value =>
                        {
                            HoldCircle.fillAmount = value;
                            HoldCircle.color = Color.Lerp(StartColor, EndColor, value);
                        });
                }
            }
        }
    }
}
