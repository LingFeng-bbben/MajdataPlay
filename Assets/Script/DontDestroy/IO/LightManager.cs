using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
#nullable enable
namespace MajdataPlay.IO
{
    public class LightManager : MonoBehaviour
    {
        bool _useDummy = true;
        SpriteRenderer[] _dummyLights = Array.Empty<SpriteRenderer>();
        SerialPort _serial = new SerialPort("COM21", 115200);
        LedDevice[] _ledDevices = new LedDevice[8];
        //Coroutine[] timers = new Coroutine[8];
        //List<byte> _templateAll = new List<byte>() { 0xE0, 0x11, 0x01, 0x08, 0x32, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //List<byte> _templateSingle = new List<byte>() { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00 };
        byte[] _templateSingle = new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 };
        byte[] _templateUpdate = new byte[]
        { 
            0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F 
        };
        
        private void Awake()
        {
            MajInstances.LightManager = this;
            DontDestroyOnLoad(gameObject);
            _dummyLights = gameObject.GetComponentsInChildren<SpriteRenderer>();
            for (var i = 0; i < 8; i++)
            {
                _ledDevices[i] = new()
                {
                    Index = i,
                };
            }
            try
            {
                _serial.WriteBufferSize = 16;
                _serial.Open();
                _useDummy = false;
                foreach (var light in _dummyLights)
                {
                    light.forceRenderingOff = true;
                }
            }
            catch
            {
                Debug.LogWarning("Cannot open COM21, using dummy lights");
                _useDummy = true;
            }
            UpdateLedDeviceAsync();
        }
        private void OnDestroy()
        {
            MajInstanceHelper<LightManager>.Free();
            _serial!.Close();
        }

        byte CalculateCheckSum(List<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Count; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        byte CalculateCheckSum(Span<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        //void SetAllLightSerial(Color color)
        //{
        //    //var bytes = _templateAll.Clone();
        //    //bytes[8] = (byte)(color.r * 255);
        //    //bytes[9] = (byte)(color.g * 255);
        //    //bytes[10] = (byte)(color.b * 255);
        //    //bytes.Add(CalculateCheckSum(bytes));
        //    //Task.Run(() => { _serial.Write(bytes.ToArray(), 0, bytes.Count); });
        //    //UpdateLightSerial();
        //}
        //void SetButtonLightSerial(Color color, int button)
        //{
        //    //var bytes = _templateSingle.Clone();
        //    //bytes[5] = (byte)button;
        //    //bytes[6] = (byte)(color.r * 255);
        //    //bytes[7] = (byte)(color.g * 255);
        //    //bytes[8] = (byte)(color.b * 255);
        //    //bytes.Add(CalculateCheckSum(bytes));
        //    //Task.Run(() => { _serial.Write(bytes.ToArray(), 0, bytes.Count); });
        //    //UpdateLightSerial();
        //}

        //void UpdateLightSerial()
        //{
        //    Task.Run(() => { _serial.Write(_templateUpdate, 0, _templateUpdate.Length); });
        //}

        public void SetAllLight(Color lightColor)
        {
            //if (_useDummy)
            //{
            //    foreach (var light in _dummyLights)
            //    {
            //        light.color = lightColor;
            //    }
            //}
            //else
            //{
            //    SetAllLightSerial(lightColor);
            //}
            foreach(var device in ArrayHelper.ToEnumerable(_ledDevices))
            {
                device!.SetColor(lightColor);
            }
        }

        public void SetButtonLight(Color lightColor, int button)
        {
            //if (_useDummy)
            //{
            //    _dummyLights[button].color = lightColor;
            //}
            //else
            //{
            //    SetButtonLightSerial(lightColor, button);
            //}
            _ledDevices[button].SetColor(lightColor);
        }

        public void SetButtonLightWithTimeout(Color lightColor, int button, long durationMs = 300)
        {
            _ledDevices[button].SetColor(lightColor, durationMs);
        }
        public void SetButtonLightWithTimeout(Color lightColor, int button,TimeSpan duration)
        {
            _ledDevices[button].SetColor(lightColor,duration);
            //SetButtonLight(lightColor, button);
            //if (timers[button] != null)
            //    StopCoroutine(timers[button]);
            //timers[button] = StartCoroutine(TurnWhiteAfter(button));
        }
        //IEnumerator TurnWhiteAfter(int button)
        //{
        //    yield return new WaitForSeconds(0.3f);
        //    SetButtonLight(Color.white, button);
        //}

        //IEnumerator DebugLights()
        //{
        //    while (true)
        //    {
        //        SetButtonLight(Color.red, 1);
        //        yield return new WaitForSeconds(0.3f);
        //        SetButtonLight(Color.green, 1);
        //        yield return new WaitForSeconds(0.3f);
        //        //SetAllLight(Color.blue);
        //        //yield return new WaitForSeconds(1);
        //        //for (int i = 1; i < 9; i++)
        //        //{
        //        //    SetButtonLight(Color.red, i);
        //        //    yield return new WaitForSeconds(0.3f);
        //        //}
        //        //for (int i = 1; i < 9; i++)
        //        //{
        //        //    SetButtonLight(Color.green, i);
        //        //    yield return new WaitForSeconds(0.3f);
        //        //}
        //        //for (int i = 1; i < 9; i++)
        //        //{
        //        //    SetButtonLight(Color.blue, i);
        //        //    yield return new WaitForSeconds(0.3f);
        //        //}
        //        //for (float i = 0; i < 1; i += 0.01f)
        //        //{
        //        //    SetAllLight(new Color(1f - i, 1f - i, 1f - i));
        //        //    yield return new WaitForSeconds(0.01f);
        //        //}
        //        //for (float i = 0; i < 1; i+=0.01f) {
        //        //    SetAllLight(new Color(i,i,i));
        //        //    yield return new WaitForSeconds(0.01f); 
        //        //}
        //        //for (float i = 0; i < 1; i += 0.01f)
        //        //{
        //        //    SetAllLight(Color.HSVToRGB(i,1,1));
        //        //    yield return new WaitForSeconds(0.1f);
        //        //}
        //    }
        //}

        
        async void UpdateLedDeviceAsync()
        {
            UniTask.Void(async () =>
            {
                var token = GameManager.GlobalCT;
                while(!token.IsCancellationRequested)
                {
                    foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
                    {
                        _dummyLights[device!.Index].color = device.Color;
                    }
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            });
            if (_useDummy)
                return;
            await Task.Run(async () =>
            {
                var token = GameManager.GlobalCT;
                while (!token.IsCancellationRequested)
                {
                    var startAt = MajTimeline.UnscaledTime;
                    try
                    {
                        var serialStream = _serial.BaseStream;
                        foreach (var device in ArrayHelper.ToEnumerable(_ledDevices))
                        {
                            var index = device!.Index;
                            var color = device.Color;
                            var packet = _templateSingle;
                            packet[5] = (byte)index;
                            packet[6] = (byte)(color.r * 255);
                            packet[7] = (byte)(color.g * 255);
                            packet[8] = (byte)(color.b * 255);
                            packet[9] = CalculateCheckSum(packet.AsSpan().Slice(0, 9));

                            await serialStream.WriteAsync(packet.AsMemory());
                        }
                        await serialStream.WriteAsync(_templateUpdate.AsMemory());
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    var endAt = MajTimeline.UnscaledTime;
                    var elapsedTime = endAt - startAt;
                    var waitTime = elapsedTime.TotalMilliseconds > 16.6667 ? 0 : 16.6667 - elapsedTime.TotalMilliseconds;
                    await Task.Delay(TimeSpan.FromMilliseconds(waitTime));
                }
            });
        }
    }
}