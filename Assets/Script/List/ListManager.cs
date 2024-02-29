using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListManager : MonoBehaviour
{
    public CoverListDisplayer CoverListDisplayer;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlaySFX("SelectSong.wav");
        IOManager.Instance.OnButtonDown += IO_OnButtonDown;
        IOManager.Instance.OnTouchAreaDown += IO_OnTouchAreaDown;
    }

    private void IO_OnTouchAreaDown(object sender, TouchAreaEventArgs e)
    {
        if (e.AreaName == "A1")
        {
            CoverListDisplayer.SlideList(1);
        }
        if (e.AreaName == "D2")
        {
            CoverListDisplayer.SlideList(2);
        }
        if (e.AreaName == "A2")
        {
            CoverListDisplayer.SlideList(3);
        }
        if (e.AreaName == "D3")
        {
            CoverListDisplayer.SlideList(4);
        }
        if (e.AreaName == "A3")
        {
            CoverListDisplayer.SlideList(5);
        }
        if (e.AreaName == "D4")
        {
            CoverListDisplayer.SlideList(6);
        }
        if (e.AreaName == "A8")
        {
            CoverListDisplayer.SlideList(-1);
        }
        if (e.AreaName == "D8")
        {
            CoverListDisplayer.SlideList(-2);
        }
        if (e.AreaName == "A7")
        {
            CoverListDisplayer.SlideList(-3);
        }
        if (e.AreaName == "D7")
        {
            CoverListDisplayer.SlideList(-4);
        }
        if (e.AreaName == "A6")
        {
            CoverListDisplayer.SlideList(-5);
        }
        if (e.AreaName == "D6")
        {
            CoverListDisplayer.SlideList(-6);
        }
    }

    private void IO_OnButtonDown(object sender, ButtonEventArgs e)
    {
        if(e.ButtonIndex == 3)
        {
            CoverListDisplayer.SlideList(1);
        }
        if (e.ButtonIndex == 6)
        {
            CoverListDisplayer.SlideList(-1);
        }
    }

    private void OnDestroy()
    {
        IOManager.Instance.OnButtonDown -= IO_OnButtonDown;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
