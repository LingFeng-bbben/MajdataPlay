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
#nullable enable
namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        public static IOManager Instance;
        private bool[] sensorStates = new bool[35];
        private bool[] lastSensorStates = new bool[35];
        public event EventHandler<TouchAreaEventArgs> OnTouchAreaDown;
        public event EventHandler<TouchAreaEventArgs> OnTouchAreaUp;
        public event EventHandler<ButtonEventArgs> OnButtonDown;
        public event EventHandler<ButtonEventArgs> OnButtonUp;
        public GameObject[] touchDisplays;
        //public Text debugtext;
        public bool displayDebug = false;
        public bool useDummy = false;

        Task? recvTask = null;
        CancellationTokenSource cancelSource = new();




        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        void Start()
        {

            RawInput.Start();
            RawInput.OnKeyDown += RawInput_OnKeyDown;
            RawInput.OnKeyUp += RawInput_OnKeyUp;
            try
            {
                COMReceiveAsync(cancelSource.Token);
            }
            catch
            {
                Debug.LogWarning("Cannot open COM3, using Mouse as fallback.");
                useDummy = true;
            }
            //if (!displayDebug)
            //{
            //    for (int i = 0; i < 34; i++)
            //    {
            //        Destroy(touchDisplays[i]);
            //    }
            //}
        }

        

        

        private void OnApplicationQuit()
        {
            cancelSource.Cancel();
            RawInput.Stop();
        }

        void Update()
        {
            if (useDummy)
            {
                
            }
            if (displayDebug)
            {
                //debugtext.text = "";
                for (int i = 0; i < 34; i++)
                {
                    //debugtext.text += sensorStates[i] ? 1 : 0;
                    touchDisplays[i].SetActive(sensorStates[i]);
                }
            }

            for (int i = 0; i < sensorStates.Length; i++)
            {
                if (sensorStates[i] && !lastSensorStates[i])
                {
                    var args = new TouchAreaEventArgs(IndexToTouchName[i], i);
                    if (OnTouchAreaDown != null)
                        OnTouchAreaDown(this, args);
                    //print(IndexToTouchName[i]+" DOWN");
                }
                if (!sensorStates[i] && lastSensorStates[i])
                {
                    var args = new TouchAreaEventArgs(IndexToTouchName[i], i);
                    if (OnTouchAreaUp != null)
                        OnTouchAreaUp(this, args);
                    //print(IndexToTouchName[i] + " UP");
                }
                lastSensorStates[i] = sensorStates[i];
            }
        }

        

        readonly Dictionary<string, int> TouchNameToIndex = new Dictionary<string, int>
    {
        { "A1",0},
        { "A2",1},
        { "A3",2},
        { "A4",3},
        { "A5",4},
        { "A6",5},
        { "A7",6},
        { "A8",7},
        { "B1",8},
        { "B2",9},
        { "B3",10},
        { "B4",11},
        { "B5",12},
        { "B6",13},
        { "B7",14},
        { "B8",15},
        { "C2",16},
        { "C1",17},
        { "D1",18},
        { "D2",19},
        { "D3",20},
        { "D4",21},
        { "D5",22},
        { "D6",23},
        { "D7",24},
        { "D8",25},
        { "E1",26},
        { "E2",27},
        { "E3",28},
        { "E4",29},
        { "E5",30},
        { "E6",31},
        { "E7",32},
        { "E8",33},
    };
        readonly string[] IndexToTouchName = new string[]
        {
        "A1","A2","A3","A4","A5","A6","A7","A8",
        "B1","B2","B3","B4","B5","B6","B7","B8","C2","C1",
        "D1","D2","D3","D4","D5","D6","D7","D8",
        "E1","E2","E3","E4","E5","E6","E7","E8",
        };
        readonly string[] IndexToKeyButton = new string[]
        {
        "S","W","E","D","C","X","Z","A","Q","N3","N7","Multiply","N9"
        };

        public bool GetTouchAreaState(string area)
        {
            try
            {
                var index = TouchNameToIndex[area];
                return sensorStates[index];
            }
            catch (Exception)
            {
                Debug.LogError("There is no such touch area called " + area);
                return false;
            }
        }
        public bool GetButtonState(int button)
        {
            try
            {
                return RawInput.IsKeyDown((RawKey)Enum.Parse(typeof(RawKey), IndexToKeyButton[button]));
            }
            catch
            {
                Debug.LogError("No such button. 9h?");
                return false;
            }
        }
    }
}