using MajdataPlay.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace MajdataPlay.IO
{
    public class LightManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static LightManager Instance;
        bool useDummy = true;
        SpriteRenderer[] DummyLights;
        SerialPort serial;
        List<byte> templateAll = new List<byte>() { 0xE0, 0x11, 0x01, 0x08, 0x32, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
        List<byte> templateSingle = new List<byte>() { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00 };
        List<byte> templateUpdate = new List<byte>() { 0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F };
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DummyLights = gameObject.GetComponentsInChildren<SpriteRenderer>();
            try
            {
                serial = new SerialPort("COM21", 115200);
                serial.WriteBufferSize = 16;
                serial.Open();
                useDummy = false;
                foreach (var light in DummyLights)
                {
                    light.forceRenderingOff = true;
                }
            }
            catch
            {
                Debug.Log("Cannot open COM21, using dummy lights");
                useDummy = true;
            }
        }
        private void OnDestroy()
        {
            serial!.Close();
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
            var bytes = templateAll.Clone();
            bytes[8] = (byte)(color.r * 255);
            bytes[9] = (byte)(color.g * 255);
            bytes[10] = (byte)(color.b * 255);
            bytes.Add(CalculateCheckSum(bytes));
            Task.Run(() => { serial.Write(bytes.ToArray(), 0, bytes.Count); });
            UpdateLightSerial();
        }
        void SetButtonLightSerial(Color color, int button)
        {
            var bytes = templateSingle.Clone();
            bytes[5] = (byte)button;
            bytes[6] = (byte)(color.r * 255);
            bytes[7] = (byte)(color.g * 255);
            bytes[8] = (byte)(color.b * 255);
            bytes.Add(CalculateCheckSum(bytes));
            Task.Run(() => { serial.Write(bytes.ToArray(), 0, bytes.Count); });
            UpdateLightSerial();
        }

        void UpdateLightSerial()
        {
            Task.Run(() => { serial.Write(templateUpdate.ToArray(), 0, templateUpdate.Count); });
        }

        public void SetAllLight(Color lightColor)
        {
            if (useDummy)
            {
                foreach (var light in DummyLights)
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
            if (useDummy)
            {
                DummyLights[button].color = lightColor;
            }
            else
            {
                SetButtonLightSerial(lightColor, button);
            }
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