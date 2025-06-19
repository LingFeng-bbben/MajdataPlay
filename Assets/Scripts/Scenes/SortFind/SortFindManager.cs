using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.IO;
using UnityEngine.EventSystems;
using MajdataPlay.Utils;

namespace MajdataPlay.SortFind
{
#nullable enable
    public class SortFindManager : MonoBehaviour
    {
        
        public TextMeshProUGUI[] Sorts;
        public InputField SearchBar;

        public Color selectedColor;
        public SortType sortType;

        EventSystem _eventSystem;

        bool _isExited = false;

        // Start is called before the first frame update
        void Start()
        {
            _eventSystem = EventSystem.current;
            _eventSystem.SetSelectedGameObject(SearchBar.gameObject);
            LedRing.SetAllLight(Color.black);
            SearchBar.text = SongStorage.OrderBy.Keyword;
            SetActiveSort(SongStorage.OrderBy.SortBy);
        }
        void Update()
        {
            if (InputManager.IsSensorClickedInThisFrame(SensorArea.D5) || _isExited)
            {
                SortAndExit();
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E6))
            {
                //sort by add time
                SetActiveSort(SortType.ByTime);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.B5))
            {
                //sort by difficulty
                SetActiveSort(SortType.ByDiff);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.B4))
            {
                //sort by mapper
                SetActiveSort(SortType.ByDes);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E4))
            {
                //sort by title
                SetActiveSort(SortType.ByTitle);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E5))
            {
                //default
                SetActiveSort(SortType.Default);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E3))
            {
                SearchBar.text = string.Empty;
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E7) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.B7) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.C) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.B2))
            {
                _eventSystem.SetSelectedGameObject(SearchBar.gameObject);
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
            if(_isExited)
            {
                return;
            }
            _isExited = true;
            var orderBy = SongStorage.OrderBy;
            orderBy.Keyword = SearchBar.text;
            orderBy.SortBy = sortType;
            MajInstances.SceneSwitcher.SwitchScene("List", false);
        }
    }
}