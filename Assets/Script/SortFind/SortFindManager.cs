using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.IO;
using MajdataPlay.List;
using MajdataPlay.Types;
using UnityEngine.EventSystems;
using MajdataPlay.Utils;
using MajdataPlay;

public class SortFindManager : MonoBehaviour
{
    public TextMeshProUGUI[] Sorts;
    public InputField SearchBar;

    public Color selectedColor;
    public SortType sortType;
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(SearchBar.gameObject);
        LightManager.Instance.SetAllLight(Color.black);
        InputManager.Instance.BindAnyArea(OnAreaDown);
        SearchBar.text = SongStorage.lastFindKey;
        SetActiveSort(SongStorage.lastSortType);
    }

    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (!e.IsClick)
            return;
        if (!e.IsButton)
        {
            switch (e.Type)
            {
                case SensorType.E6:
                    //sort by add time
                    SetActiveSort(SortType.ByTime);
                    break;
                case SensorType.B5:
                    //sort by difficulty
                    SetActiveSort(SortType.ByDiff);
                    break;
                case SensorType.B4:
                    //sort by mapper
                    SetActiveSort(SortType.ByDes);
                    break;
                case SensorType.E4:
                    //sort by title
                    SetActiveSort(SortType.ByTitle);
                    break;
                case SensorType.E5:
                    //default
                    SetActiveSort(SortType.Default);
                    break;

                case SensorType.E7:
                case SensorType.B7:
                case SensorType.C:
                case SensorType.B2:
                    EventSystem.current.SetSelectedGameObject(SearchBar.gameObject);
                    break;
                case SensorType.E3:
                    SearchBar.text = string.Empty;
                    break;

                case SensorType.D5:
                    InputManager.Instance.UnbindAnyArea(OnAreaDown);
                    SortAndExit();
                    break;
            }
        }

    }

    void SetActiveSort(SortType _sortType)
    {
        ClearColor();
        Sorts[(int)_sortType].color = selectedColor;
        sortType = _sortType;
    }

    void ClearColor()
    {
        foreach (var s in Sorts)
        {
            s.color = Color.white;
        }
    }

    void SortAndExit()
    {
        SongStorage.SortAndFind(SearchBar.text,sortType);
        if (GameManager.Instance.Collection.Count != 0)
            GameManager.Instance.Collection.Index = 0;
        SceneSwitcher.Instance.SwitchScene(1);
    }
}
