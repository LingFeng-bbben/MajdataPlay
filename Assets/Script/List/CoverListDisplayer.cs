using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class CoverListDisplayer : MonoBehaviour
{
    List<GameObject> covers = new List<GameObject>();
    public string soundEffectName;
    public GameObject CoverSmallPrefab;
    public GameObject DirSmallPrefab;
    public CoverBigDisplayer CoverBigDisplayer;

    public int desiredListPos = 0;
    public float listPosReal;
    public float turnSpeed;
    public float radius;
    public float offset;

    public int selectedDifficulty = 0;
    public bool isDirList = true;

    private SongCollection[] dirs = Array.Empty<SongCollection>();
    private SongCollection songs = new SongCollection();
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    public void SetDirList(SongCollection[] _dirs)
    {
        foreach(var cover in covers)
        {
            Destroy(cover);
        }
        covers.Clear();
        isDirList = true;
        dirs = _dirs;
        desiredListPos = GameManager.Instance.SelectedDir;
        foreach (var dir in _dirs)
        {
            var obj = Instantiate(DirSmallPrefab, transform);
            var coversmall = obj.GetComponent<CoverSmallDisplayer>();
            //coversmall.SetCover(song.SongCover);
            coversmall.SetLevelText(dir.Name);
            covers.Add(obj);
            obj.SetActive(false);
        }
        if (desiredListPos > covers.Count) desiredListPos = 0;
        listPosReal = desiredListPos;
        SlideToList(desiredListPos);
    }

    public void SetSongList()
    {
        if (songs.Count == 0) return;
        foreach (var cover in covers)
        {
            Destroy(cover);
        }
        covers.Clear();
        isDirList = false;
        desiredListPos = GameManager.Instance.Collection.Index;
        foreach (var song in songs)
        {
            var obj = Instantiate(CoverSmallPrefab, transform);
            var coversmall = obj.GetComponent<CoverSmallDisplayer>();
            coversmall.SetOpacity(0f);
            coversmall.SetCover(song);
            coversmall.SetLevelText(song.Levels[selectedDifficulty]);
            covers.Add(obj);
            obj.SetActive(false);
        }
        if (desiredListPos > covers.Count) desiredListPos = 0;
        listPosReal = desiredListPos;
        SlideToList(desiredListPos);
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
        CoverBigDisplayer.SetDifficulty(selectedDifficulty);
        if (!isDirList)
        {
            var songinfo = songs.ToArray()[desiredListPos];
            var songScore = ScoreManager.Instance.GetScore(songinfo, GameManager.Instance.SelectedDiff);
            CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
            CoverBigDisplayer.SetScore(songScore);

            for (int i = 0; i < covers.Count; i++)
            {
                var text = songs.ToArray()[i].Levels[selectedDifficulty];
                if (text == null || text == "") text = "-";
                covers[i].GetComponent<CoverSmallDisplayer>().SetLevelText(text);
            }
        }
    }

    public void SlideList(int delta)
    {
        desiredListPos+=delta;
        SlideToList(desiredListPos);
    }
    
    public void SlideToList(int pos)
    {
        AudioManager.Instance.PlaySFX(SFXSampleType.JUDGE);
        desiredListPos = pos;
        if (desiredListPos >= covers.Count)
        {
            desiredListPos = covers.Count - 1;
        }
        if (desiredListPos <= 0)
        {
            desiredListPos = 0;
        }
        if (!isDirList)
        {
            var songinfo = songs.ToArray()[desiredListPos];
            var songScore = ScoreManager.Instance.GetScore(songinfo, GameManager.Instance.SelectedDiff);
            CoverBigDisplayer.SetCover(songinfo);
            CoverBigDisplayer.SetMeta(songinfo.Title, songinfo.Artist, songinfo.Designers[selectedDifficulty], songinfo.Levels[selectedDifficulty]);
            CoverBigDisplayer.SetScore(songScore);
            GameManager.Instance.Collection.Index = desiredListPos;
        }
        else {
            songs = dirs[desiredListPos];
            CoverBigDisplayer.SetMeta(songs.Name, "", "", "");
            CoverBigDisplayer.SetScore(new MaiScore());
            GameManager.Instance.SelectedDir = desiredListPos;
        }
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
        if (isDirList && Time.frameCount%50==0) {
            if (coveri >= songs.Count) coveri = 0;
            CoverBigDisplayer.SetCover(songs.ToArray()[coveri++]);
        }
    }
    private int coveri = 0;

    Vector3 GetCoverPosition(float radius, float position)
    {
        return new Vector3(radius * Mathf.Sin(position), radius * Mathf.Cos(position));
    }
}
