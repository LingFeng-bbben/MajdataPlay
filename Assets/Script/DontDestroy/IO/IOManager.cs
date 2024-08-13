using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityRawInput;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Types;
using UnityEngine.UIElements;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        public static IOManager Instance;
        public bool displayDebug = false;
        public bool useDummy = false;


        bool[] COMReport = Enumerable.Repeat(false,35).ToArray();
        Task? recvTask = null;
        CancellationTokenSource cancelSource = new();

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
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
            foreach (var (index, child) in transform.ToEnumerable().WithIndex())
                sensors[index] = child.GetComponent<Sensor>();
        }
        void OnApplicationQuit()
        {
            cancelSource.Cancel();
            RawInput.Stop();
            if (recvTask != null && !recvTask.IsCompleted)
                recvTask.Wait();
        }

        void Update()
        {
            if (useDummy)
                UpdateMousePosition();
            else
                UpdateSensorState();
        }
        public void BindArea(EventHandler<InputEventArgs> checker, SensorType sType)
        {
            var sensor = sensors.Find(x => x.Type == sType);
            var button = buttons.Find(x => x.Type == sType);
            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            sensor.OnStatusChanged += checker;
            button.OnStatusChanged += checker;
        }
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
            var sensor = sensors.Find(x => x.Type == sType);
            var button = buttons.Find(x => x.Type == sType);

            if (sensor == null || button is null)
                throw new Exception($"{sType} Sensor or Button not found.");

            return sensor.Status == targetStatus || button.Status == targetStatus;
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
                throw new InvalidOperationException("Button index cannot greater than A8");
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
    }
}