using MajdataPlay.IO;
using MajdataPlay.Types;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class CoverListDisplayer : MonoBehaviour
{
    List<string> songlist = new List<string>(3);
    List<GameObject> covers = new List<GameObject>();
    public string soundEffectName;
    public GameObject CoverSmallPrefab;
    public CoverBigDisplayer CoverBigDisplayer;

    public int desiredListPos = 0;
    public float listPosReal;
    public float turnSpeed;
    public float radius;
    public float offset;

    public int selectedDifficulty = 0;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (var song in GameManager.Instance.SongList)
        {
            var obj = Instantiate(CoverSmallPrefab, transform);
            var coversmall = obj.GetComponent<CoverSmallDisplayer>();
            coversmall.SetCover(song.SongCover);
            covers.Add(obj);
        }
    }

    public void SlideDifficulty(int delta)
    {
        selectedDifficulty += delta;
        SlideToDifficulty(selectedDifficulty);
    }

    public void SlideToDifficulty(int pos)
    {
        selectedDifficulty = pos;
        if (selectedDifficulty > 6)
        {
            selectedDifficulty = 0;
        }
        if (selectedDifficulty < 0)
        {
            selectedDifficulty = 6;
        }
        GameManager.Instance.SelectedDiff = (ChartLevel)selectedDifficulty;
        var songinfo = GameManager.Instance.SongList[desiredListPos];
        var songScore = ScoreManager.Instance.GetScore(songinfo, GameManager.Instance.SelectedDiff);
        CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
        CoverBigDisplayer.SetScore(songScore);
        CoverBigDisplayer.SetDifficulty(selectedDifficulty);
        for(int i=0;i<covers.Count;i++)
        {
            var text = GameManager.Instance.SongList[i].Levels[selectedDifficulty];
            if (text == null || text == "") text = "-";
            covers[i].GetComponent<CoverSmallDisplayer>().SetLevelText(text);
        }
    }

    public void SlideList(int delta)
    {
        desiredListPos+=delta;
        SlideToList(desiredListPos);
    }
    
    public void SlideToList(int pos)
    {
        AudioManager.Instance.PlaySFX(soundEffectName);
        desiredListPos = pos;
        if (desiredListPos >= covers.Count)
        {
            desiredListPos = covers.Count - 1;
        }
        if (desiredListPos <= 0)
        {
            desiredListPos = 0;
        }
        var songinfo = GameManager.Instance.SongList[desiredListPos];
        var songScore = ScoreManager.Instance.GetScore(songinfo, GameManager.Instance.SelectedDiff);
        CoverBigDisplayer.SetCover(songinfo.SongCover);
        CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
        CoverBigDisplayer.SetScore(songScore);
        GameManager.Instance.SelectedIndex = desiredListPos;
    }

    private void Update()
    {
        listPosReal += (desiredListPos - listPosReal) * turnSpeed * Time.deltaTime;
        if (Mathf.Abs(desiredListPos - listPosReal) < 0.01f) listPosReal = desiredListPos;
        for (int i = 0;i < covers.Count;i++) {
            var distance = i - listPosReal;
            if (Mathf.Abs(distance) > 7) {
                covers[i].SetActive(false);
                continue; 
            }
            covers[i].SetActive(true);
            covers[i].GetComponent<RectTransform>().anchoredPosition = GetCoverPosition(radius, (distance) * Mathf.Deg2Rad * 22.5f);
            var scd = covers[i].GetComponent<CoverSmallDisplayer>();
            if (Mathf.Abs(distance) > 6)
            {
                scd.SetOpacity( -Mathf.Abs(distance) +7);
            }
            else
            {
                scd.SetOpacity(1f);
            }
        }
    }


    Vector3 GetCoverPosition(float radius, float position)
    {
        return new Vector3(radius * Mathf.Sin(position), radius * Mathf.Cos(position));
    }
}
