using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.IO;
using MajdataPlay.List;
using MajdataPlay.Types;
using UnityEngine.EventSystems;

public class SortFindManager : MonoBehaviour
{
    public TextMeshProUGUI[] Sorts;
    public InputField SearchBar;

    public Color selectedColor;

    // Start is called before the first frame update
    void Start()
    {
        //TODO: disable button input
        EventSystem.current.SetSelectedGameObject(SearchBar.gameObject);
        LightManager.Instance.SetAllLight(Color.black);
        InputManager.Instance.BindAnyArea(OnAreaDown);
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
                    ClearColor();
                    Sorts[0].color = selectedColor;
                    break;
                case SensorType.B5:
                    //sort by difficulty
                    ClearColor();
                    Sorts[1].color = selectedColor;
                    break;
                case SensorType.B4:
                    //sort by mapper
                    ClearColor();
                    Sorts[2].color = selectedColor;
                    break;
                case SensorType.E4:
                    //sort by title
                    ClearColor();
                    Sorts[3].color = selectedColor;
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

    void ClearColor()
    {
        foreach (var s in Sorts)
        {
            s.color = Color.white;
        }
    }

    void SortAndExit()
    {
        SceneSwitcher.Instance.SwitchScene(1);
    }
}
