using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoverBigDisplayer : MonoBehaviour
{
    public Image bgCard;
    public Image Cover;
    public TMP_Text Level;
    public TMP_Text Charter;
    public TMP_Text Title;
    public TMP_Text Artist;
    public TMP_Text ArchieveRate;

    public Color[] diffColors = new Color[6];

    private void Start()
    {
       /* Level = transform.Find("Level").GetComponent<TMP_Text>();
        Charter = transform.Find("Designer").GetComponent<TMP_Text>();
        Title = transform.Find("Title").GetComponent<TMP_Text>();
        Artist = transform.Find("Artist").GetComponent<TMP_Text>();
        ArchieveRate = transform.Find("Rate").GetComponent<TMP_Text>();*/
    }
    public void SetDifficulty(int i)
    {
        bgCard.color = diffColors[i];
    }
    public void SetCover(Sprite sp)
    {
        Cover.sprite = sp;
    }
    public void SetMeta(string _Title,string _Artist,string _Charter, string _Level)
    {
        Title.text = _Title;
        Artist.text = _Artist;
        Charter.text = _Charter;
        Level.text = _Level;
    }
}
