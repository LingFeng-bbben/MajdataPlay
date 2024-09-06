using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public List<GameObject> notes = new();
    public Dictionary<GameObject, int> noteOrder = new();
    public Dictionary<int, int> noteIndex = new();

    public Dictionary<GameObject, int> touchOrder = new();
    public Dictionary<SensorType, int> touchIndex = new();

    private bool isNotesLoaded = false;
    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 30;
        
    }
    public void Refresh()
    {
        var count = transform.childCount;
        ResetCounter();
        for (int i = 0; i < count; i++)
        {
            var child = transform.GetChild(i);
            var tap = child.GetComponent<TapDrop>();
            var hold = child.GetComponent<HoldDrop>();
            var star = child.GetComponent<StarDrop>();
            var touch = child.GetComponent<TouchDrop>();
            var touchHold = child.GetComponent<TouchHoldDrop>();

            if (tap != null)
                noteOrder.Add(tap.gameObject, noteIndex[tap.startPosition]++);
            else if (hold != null)
                noteOrder.Add(hold.gameObject, noteIndex[hold.startPosition]++);
            else if (star != null && !star.isNoHead)
                noteOrder.Add(star.gameObject, noteIndex[star.startPosition]++);
            else if (touch != null)
                touchOrder.Add(touch.gameObject, touchIndex[touch.GetSensor()]++);
            else if(touchHold != null)
                touchOrder.Add(touchHold.gameObject, touchIndex[SensorType.C]++);

            notes.Add(child.gameObject);
        }
        ResetCounter();
        isNotesLoaded = true;
    }
    void ResetCounter()
    {
        noteIndex.Clear();
        touchIndex.Clear();
        //八条轨道 判定到此轨道上的第几个note了
        for (int i = 1; i < 9; i++)
            noteIndex.Add(i, 0);
        var sensorParent = GameObject.Find("IOManager");
        var count = sensorParent.transform.childCount;
        for (int i = 0; i < count; i++)
            touchIndex.Add(sensorParent.transform
                                       .GetChild(i)
                                       .GetComponent<Sensor>().Type, 0);
    }
    public int GetOrder(GameObject obj) => noteOrder[obj];
    public bool CanJudge(GameObject obj, int pos)
    {
        if (!noteOrder.ContainsKey(obj))
            return false;
        var index = noteOrder[obj];
        var nowIndex = noteIndex[pos];

        return index <= nowIndex;
    }

    public bool CanJudge(GameObject obj,SensorType t)
    {
        if (!touchOrder.ContainsKey(obj))
            return false;
        var index = touchOrder[obj];
        var nowIndex = touchIndex[t];

        return index <= nowIndex;
    }

    public void DestroyAllNotes()
    {
        foreach(var note in notes)
        {
            if(note != null)
                Destroy(note);
        }
    }

    public void AddNote(GameObject obj, int index) => noteOrder.Add(obj, index);
    public void AddTouch(GameObject obj, int index) => touchOrder.Add(obj, index);
    private void FixedUpdate()
    {
        if(transform.childCount == 0 && isNotesLoaded)
        {
            Debug.Log("No More Notes");
            GameObject.Find("ObjectCounter").GetComponent<ObjectCounter>().CalculateFinalResult();
        }
    }
}
