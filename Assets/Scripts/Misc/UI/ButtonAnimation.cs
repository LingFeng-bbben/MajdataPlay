using MajdataPlay.IO;
using System.Security.Policy;
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;

namespace MajdataPlay.UI
{
    public class ButtonAnimawiption : MonoBehaviour
    {
        public SensorArea[] BindSensor;
        public ButtonZone[] BindButton;
        public float ReleasedScale = 1f;
        public float PressedScale = 0.9f;
        public float AnimationDuration = 0.1f;
        public Ease EaseType = Ease.OutQuad;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            var isDown = false;
            var isUp = false;
            var isPressed = false;

            foreach (var button in BindButton) {
                isDown |= InputManager.CheckButtonStatusInPreviousFrame(button, SwitchStatus.Off) &&
                            InputManager.CheckButtonStatusInThisFrame(button, SwitchStatus.On);
            }
            foreach (var button in BindButton)
            {
                isUp |= InputManager.CheckButtonStatusInPreviousFrame(button, SwitchStatus.On) &&
                            InputManager.CheckButtonStatusInThisFrame(button, SwitchStatus.Off);
            }
            foreach (var button in BindButton)
            {
                isPressed |= InputManager.CheckButtonStatusInThisFrame(button, SwitchStatus.On);
            }

            foreach (var sensor in BindSensor)
            {
                isDown |= InputManager.CheckSensorStatusInPreviousFrame(sensor, SwitchStatus.Off) &&
                            InputManager.CheckSensorStatusInThisFrame(sensor, SwitchStatus.On);
            }
            foreach (var sensor in BindSensor)
            {
                isUp |= InputManager.CheckSensorStatusInPreviousFrame(sensor, SwitchStatus.On) &&
                            InputManager.CheckSensorStatusInThisFrame(sensor, SwitchStatus.Off);
            }
            foreach (var sensor in BindSensor)
            {
                isPressed |= InputManager.CheckSensorStatusInThisFrame(sensor, SwitchStatus.On);
            }

            if (isDown)
            {
                var pressedScale = new Vector3(PressedScale, PressedScale, PressedScale);
                LMotion.Create(transform.localScale, pressedScale, AnimationDuration)
                    .WithEase(EaseType)
                    .BindToLocalScale(transform);
            }
            else if (isUp && !isPressed)
            {
                var releasedScale = new Vector3(ReleasedScale, ReleasedScale, ReleasedScale);
                LMotion.Create(transform.localScale, releasedScale, AnimationDuration)
                    .WithEase(EaseType)
                    .BindToLocalScale(transform);
            }
        }
    }
}
