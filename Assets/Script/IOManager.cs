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

public class IOManager : MonoBehaviour
{
    public static IOManager Instance;
    private SerialPort serial;
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
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        DontDestroyOnLoad(this);
        

       try 
       {
            sensorStates = new bool[35];
            serial = new SerialPort("COM3", 9600);
            serial.Open();
            serial.Write("{STAT}");
            Thread recvThread = new Thread(new ThreadStart(recvLoop));
            recvThread.Start();
       }
       catch
       {
            Debug.LogWarning("Cannot open COM3, using Mouse as fallback.");
            useDummy = true;
       }
    }


    void recvLoop()
    {
        while (true)
        {
            if(serial.IsOpen)
            {
                int count = serial.BytesToRead;
                var buf = new byte[count];
                serial.Read(buf, 0, count);
                if (buf.Length > 0)
                {
                    
                    if (buf[0] == '(')
                    {
                        int k = 0;
                        for (int i = 1; i < 8; i++)
                        {
                            print(buf[i].ToString("X2"));
                            for (int j = 0; j < 5; j++)
                            {
                                sensorStates[k] = (buf[i] & (0x01 << j)) > 0;
                                k++;
                            }
                        }
                    }
                    
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (serial!= null & serial.IsOpen) { serial.Close(); }
    }
    
    void Update()
    {
        if (useDummy)
        {
            if (Input.GetMouseButton(0))
            {
                var x = Input.mousePosition.x / Screen.width * 2 - 1;
                var y = Input.mousePosition.y / Screen.width * 2 - 1;
                var distance = Math.Sqrt(x * x + y * y);
                var angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
                for (int i = 0; i < sensorStates.Length; i++)
                {
                    sensorStates[i] = false;
                }
                if (distance > 0.75)
                {
                    if (isInRange(angle, 0))
                    {
                        sensorStates[TouchNameToIndex["D3"]] = true;
                    }
                    else if (isInRange(angle, 45f))
                    {
                        sensorStates[TouchNameToIndex["D2"]] = true;
                    }
                    else if (isInRange(angle, 90f))
                    {
                        sensorStates[TouchNameToIndex["D1"]] = true;
                    }
                    else if (isInRange(angle, 135f))
                    {
                        sensorStates[TouchNameToIndex["D8"]] = true;
                    }
                    else if (isInRange(angle, 180f))
                    {
                        sensorStates[TouchNameToIndex["D7"]] = true;
                    }
                    else if (isInRange(angle, -135f))
                    {
                        sensorStates[TouchNameToIndex["D6"]] = true;
                    }
                    else if (isInRange(angle, -90f))
                    {
                        sensorStates[TouchNameToIndex["D5"]] = true;
                    }
                    else if (isInRange(angle, -45f))
                    {
                        sensorStates[TouchNameToIndex["D4"]] = true;
                    }
                    
                    
                }
                if (distance > 0.6)
                {
                    if (isInRange(angle, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["A2"]] = true;
                    }
                    else if (isInRange(angle, 67.5f))
                    {
                        sensorStates[TouchNameToIndex["A1"]] = true;
                    }
                    else if (isInRange(angle, 112.5f))
                    {
                        sensorStates[TouchNameToIndex["A8"]] = true;
                    }
                    else if (isInRange(angle, 157.5f))
                    {
                        sensorStates[TouchNameToIndex["A7"]] = true;
                    }
                    else if (isInRange(angle, -157.5f))
                    {
                        sensorStates[TouchNameToIndex["A6"]] = true;
                    }
                    else if (isInRange(angle, -112.5f))
                    {
                        sensorStates[TouchNameToIndex["A5"]] = true;
                    }
                    else if (isInRange(angle, -67.5f))
                    {
                        sensorStates[TouchNameToIndex["A4"]] = true;
                    }
                    else if (isInRange(angle, -22.5f))
                    {
                        sensorStates[TouchNameToIndex["A3"]] = true;
                    }
                }
                if (distance > 0.42 && distance<=0.71)
                {
                    if (isInRange(angle, 0))
                    {
                        sensorStates[TouchNameToIndex["E3"]] = true;
                    }
                    else if (isInRange(angle, 45f))
                    {
                        sensorStates[TouchNameToIndex["E2"]] = true;
                    }
                    else if (isInRange(angle, 90f))
                    {
                        sensorStates[TouchNameToIndex["E1"]] = true;
                    }
                    else if (isInRange(angle, 135f))
                    {
                        sensorStates[TouchNameToIndex["E8"]] = true;
                    }
                    else if (isInRange(angle, 180f))
                    {
                        sensorStates[TouchNameToIndex["E7"]] = true;
                    }
                    else if (isInRange(angle, -135f))
                    {
                        sensorStates[TouchNameToIndex["E6"]] = true;
                    }
                    else if (isInRange(angle, -90f))
                    {
                        sensorStates[TouchNameToIndex["E5"]] = true;
                    }
                    else if (isInRange(angle, -45f))
                    {
                        sensorStates[TouchNameToIndex["E4"]] = true;
                    }
                }
                if (distance > 0.267 && distance <= 0.53)
                {
                    if (isInRange(angle, 22.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B2"]] = true;
                    }
                    else if (isInRange(angle, 67.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B1"]] = true;
                    }
                    else if (isInRange(angle, 112.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B8"]] = true;
                    }
                    else if (isInRange(angle, 157.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B7"]] = true;
                    }
                    else if (isInRange(angle, -157.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B6"]] = true;
                    }
                    else if (isInRange(angle, -112.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B5"]] = true;
                    }
                    else if (isInRange(angle, -67.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B4"]] = true;
                    }
                    else if (isInRange(angle, -22.5f, 22.5f))
                    {
                        sensorStates[TouchNameToIndex["B3"]] = true;
                    }
                }
                if (distance <= 0.267)
                {
                    if (isInRange(angle, 0, 90))
                    {
                        sensorStates[TouchNameToIndex["C2"]] = true;
                    }
                    else if (isInRange(angle, 180, 90))
                    {
                        sensorStates[TouchNameToIndex["C1"]] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < sensorStates.Length; i++)
                {
                    sensorStates[i] = false;
                }
            }
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
        
        for (int i = 0; i<sensorStates.Length; i++)
        {
            if (sensorStates[i] && !lastSensorStates[i])
            {
                var args = new TouchAreaEventArgs(IndexToTouchName[i],i);
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
        for(int i = 0; i < IndexToKeyButton.Length; i++)
        {
            if (Input.GetKeyDown(IndexToKeyButton[i]))
            {
                if(OnButtonDown != null)
                {
                    OnButtonDown(this,new ButtonEventArgs(i));
                }
            }
            if (Input.GetKeyUp(IndexToKeyButton[i]))
            {
                if (OnButtonUp != null)
                {
                    OnButtonUp(this, new ButtonEventArgs(i));
                }
            }
        }
        
    }

    bool isInRange(float input, float angle,float range = 11.25f)
    {
        return Mathf.Abs(Mathf.DeltaAngle(input, angle)) < range;
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
        "s","w","e","d","c","x","z","a","q","1","2","3","4"
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
            return Input.GetKey(IndexToKeyButton[button]);
        }
        catch
        {
            Debug.LogError("No such button. 9h?");
            return false;
        }
    }
}

public class TouchAreaEventArgs : EventArgs
{
    public TouchAreaEventArgs(string area, int index)
    {
        AreaName = area;
        AreaIndex = index;
    }

    public string AreaName { get; set; }
    public int AreaIndex { get; set; }
}

public class ButtonEventArgs : EventArgs
{
    public ButtonEventArgs(int index)
    {
        ButtonIndex = index;
    }

    public int ButtonIndex { get; set; }
}
