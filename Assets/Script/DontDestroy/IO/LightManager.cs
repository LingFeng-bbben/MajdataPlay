using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public class LightManager : MonoBehaviour
    {
        bool _useDummy = true;
        SpriteRenderer[] _dummyLights;
        SerialPort _serial;
        List<byte> _templateAll = new List<byte>() { 0xE0, 0x11, 0x01, 0x08, 0x32, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
        List<byte> _templateSingle = new List<byte>() { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00 };
        List<byte> _templateUpdate = new List<byte>() { 0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F };
        private void Awake()
        {
            MajInstances.LightManager = this;
            DontDestroyOnLoad(gameObject);
            _dummyLights = gameObject.GetComponentsInChildren<SpriteRenderer>();
            try
            {
                _serial = new SerialPort("COM21", 115200);
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
                Debug.Log("Cannot open COM21, using dummy lights");
                _useDummy = true;
            }
        }
        private void OnDestroy()
        {
            MajInstanceHelper<LightManager>.Free();
            _serial!.Close();
        }

        void Start()
        {
            //StartCoroutine(DebugLights());
            //print(string.Format("{0:X}", CalculateCheckSum(templateSingle)));
            //print(string.Format("{0:X}", CalculateCheckSum(templateAll)));
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

        void SetAllLightSerial(Color color)
        {
            var bytes = _templateAll.Clone();
            bytes[8] = (byte)(color.r * 255);
            bytes[9] = (byte)(color.g * 255);
            bytes[10] = (byte)(color.b * 255);
            bytes.Add(CalculateCheckSum(bytes));
            Task.Run(() => { _serial.Write(bytes.ToArray(), 0, bytes.Count); });
            UpdateLightSerial();
        }
        void SetButtonLightSerial(Color color, int button)
        {
            var bytes = _templateSingle.Clone();
            bytes[5] = (byte)button;
            bytes[6] = (byte)(color.r * 255);
            bytes[7] = (byte)(color.g * 255);
            bytes[8] = (byte)(color.b * 255);
            bytes.Add(CalculateCheckSum(bytes));
            Task.Run(() => { _serial.Write(bytes.ToArray(), 0, bytes.Count); });
            UpdateLightSerial();
        }

        void UpdateLightSerial()
        {
            Task.Run(() => { _serial.Write(_templateUpdate.ToArray(), 0, _templateUpdate.Count); });
        }

        public void SetAllLight(Color lightColor)
        {
            if (_useDummy)
            {
                foreach (var light in _dummyLights)
                {
                    light.color = lightColor;
                }
            }
            else
            {
                SetAllLightSerial(lightColor);
            }
        }

        public void SetButtonLight(Color lightColor, int button)
        {
            if (_useDummy)
            {
                _dummyLights[button].color = lightColor;
            }
            else
            {
                SetButtonLightSerial(lightColor, button);
            }
        }

        Coroutine[] timers = new Coroutine[8];
        public void SetButtonLightWithTimeout(Color lightColor, int button)
        {
            SetButtonLight(lightColor, button);
            if (timers[button] != null)
                StopCoroutine(timers[button]);
            timers[button] = StartCoroutine(TurnWhiteAfter(button));
        }
        IEnumerator TurnWhiteAfter(int button)
        {
            yield return new WaitForSeconds(0.3f);
            SetButtonLight(Color.white, button);
        }

        IEnumerator DebugLights()
        {
            while (true)
            {
                SetButtonLight(Color.red, 1);
                yield return new WaitForSeconds(0.3f);
                SetButtonLight(Color.green, 1);
                yield return new WaitForSeconds(0.3f);
                //SetAllLight(Color.blue);
                //yield return new WaitForSeconds(1);
                //for (int i = 1; i < 9; i++)
                //{
                //    SetButtonLight(Color.red, i);
                //    yield return new WaitForSeconds(0.3f);
                //}
                //for (int i = 1; i < 9; i++)
                //{
                //    SetButtonLight(Color.green, i);
                //    yield return new WaitForSeconds(0.3f);
                //}
                //for (int i = 1; i < 9; i++)
                //{
                //    SetButtonLight(Color.blue, i);
                //    yield return new WaitForSeconds(0.3f);
                //}
                //for (float i = 0; i < 1; i += 0.01f)
                //{
                //    SetAllLight(new Color(1f - i, 1f - i, 1f - i));
                //    yield return new WaitForSeconds(0.01f);
                //}
                //for (float i = 0; i < 1; i+=0.01f) {
                //    SetAllLight(new Color(i,i,i));
                //    yield return new WaitForSeconds(0.01f); 
                //}
                //for (float i = 0; i < 1; i += 0.01f)
                //{
                //    SetAllLight(Color.HSVToRGB(i,1,1));
                //    yield return new WaitForSeconds(0.1f);
                //}
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}