using UnityEngine;
using System.Threading;
using System;
using System.Linq;
using UnityRawInput;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        public bool displayDebug = false;
        public bool useDummy = false;

        public event EventHandler<InputEventArgs>? OnAnyAreaTrigger;

        bool[] COMReport = Enumerable.Repeat(false,35).ToArray();
        Task? recvTask = null;
        Mutex buttonCheckerMutex = new();
        CancellationTokenSource cancelSource = new();

        void Awake()
        {
            MajInstances.InputManager = this;
            DontDestroyOnLoad(this);
            foreach (var (index, child) in transform.ToEnumerable().WithIndex())
            {
                sensors[index] = child.GetComponent<Sensor>();
                sensors[index].Type = (SensorType)index;
            }
        }
        void Start()
        {
            RawInput.Start();
            RawInput.OnKeyDown += OnRawKeyDown;
            RawInput.OnKeyUp += OnRawKeyUp;
            try
            {
                COMReceiveAsync(cancelSource.Token);
            }
            catch
            {
                Debug.LogWarning("Cannot open COM3, using Mouse as fallback.");
                useDummy = true;
            }

        }
        void OnApplicationQuit()
        {
            cancelSource.Cancel();
            RawInput.Stop();
            if (recvTask != null && !recvTask.IsCompleted)
                recvTask.Wait();
        }
        void FixedUpdate()
        {
            if (useDummy)
                UpdateMousePosition();
            else
                UpdateSensorState();
            UpdateButtonState();
        }
        public void BindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger += checker;
        public void BindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = sensors.Find(x => x.Type == sType);
            var button = buttons.Find(x => x.Type == sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.OnStatusChanged += checker;
            button.OnStatusChanged += checker;
        }
        public void UnbindAnyArea(EventHandler<InputEventArgs> checker) => OnAnyAreaTrigger -= checker;
        public void UnbindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = sensors.Find(x => x.Type == sType);
            var button = buttons.Find(x => x.Type == sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.OnStatusChanged -= checker;
            button.OnStatusChanged -= checker;
        }
        public bool CheckAreaStatus(SensorType sType, SensorStatus targetStatus)
        {
            return CheckSensorStatus(sType,targetStatus) || CheckButtonStatus(sType, targetStatus);
        }
        public bool CheckSensorStatus(SensorType target, SensorStatus targetStatus)
        {
            var sensor = sensors[(int)target];
            if (sensor == null)
                throw new Exception($"{target} Sensor or Button not found.");
            return sensor.Status == targetStatus;
        }
        public bool CheckButtonStatus(SensorType target, SensorStatus targetStatus)
        {
            if (target > SensorType.A8)
                throw new ArgumentOutOfRangeException("Button index cannot greater than A8");
            var button = buttons.Find(x => x.Type == target);

            if (button is null)
                throw new Exception($"{target} Button not found.");

            return button.Status == targetStatus;
        }
        public void SetBusy(InputEventArgs args)
        {
            var type = args.Type;
            if (args.IsButton)
            {
                var button = GetButton(type);
                if (button is null)
                    throw new Exception($"{type} Button not found.");

                button.IsJudging = true;
            }
            else
            {
                var sensor = GetSensor(type);
                if (sensor is null)
                    throw new Exception($"{type} Sensor not found.");

                sensor.IsJudging = true;
            }
        }
        public void SetIdle(InputEventArgs args)
        {
            var type = args.Type;
            if (args.IsButton)
            {
                var button = GetButton(type);
                if (button is null)
                    throw new Exception($"{type} Button not found.");

                button.IsJudging = false;
            }
            else
            {
                var sensor = GetSensor(type);
                if (sensor is null)
                    throw new Exception($"{type} Sensor not found.");

                sensor.IsJudging = false;
            }
        }
        public bool IsIdle(InputEventArgs args)
        {
            bool isIdle = false;
            var type = args.Type;
            if (args.IsButton)
            {
                var button = GetButton(type);
                if (button is null)
                    throw new Exception($"{type} Button not found.");

                isIdle = !button.IsJudging;
            }
            else
            {
                var sensor = GetSensor(type);
                if (sensor is null)
                    throw new Exception($"{type} Sensor not found.");

                isIdle = !sensor.IsJudging;
            }
            return isIdle;
        }
        public Button? GetButton(SensorType type) => buttons.Find(x => x.Type == type);
        public Sensor GetSensor(SensorType target) => sensors[(int)target];
        public Sensor[] GetSensors() => sensors.ToArray();
        public Sensor[] GetSensors(SensorGroup group) => sensors.Where(x => x.Group == group).ToArray();
        public void ClearAllSubscriber()
        {
            foreach(var sensor in sensors)
                sensor.ClearSubscriber();
            foreach(var button in buttons)
                button.ClearSubscriber();
            OnAnyAreaTrigger = null;
        }
        void PushEvent(InputEventArgs args)
        {
            if (OnAnyAreaTrigger is not null)
                OnAnyAreaTrigger(this, args);
        }
    }
}