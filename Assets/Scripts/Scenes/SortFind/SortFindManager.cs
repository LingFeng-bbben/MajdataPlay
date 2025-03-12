using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine.EventSystems;
using MajdataPlay.Utils;
using Cysharp.Threading.Tasks;
using MajdataPlay;

namespace MajdataPlay.SortFind
{
#nullable enable
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
            MajInstances.LightManager.SetAllLight(Color.black);
            InputManager.BindAnyArea(OnAreaDown);
            SearchBar.text = SongStorage.OrderBy.Keyword;
            SetActiveSort(SongStorage.OrderBy.SortBy);
        }
        private void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsDown)
                return;
            if (!e.IsButton)
            {
                switch (e.Type)
                {
                    case SensorArea.E6:
                        //sort by add time
                        SetActiveSort(SortType.ByTime);
                        break;
                    case SensorArea.B5:
                        //sort by difficulty
                        SetActiveSort(SortType.ByDiff);
                        break;
                    case SensorArea.B4:
                        //sort by mapper
                        SetActiveSort(SortType.ByDes);
                        break;
                    case SensorArea.E4:
                        //sort by title
                        SetActiveSort(SortType.ByTitle);
                        break;
                    case SensorArea.E5:
                        //default
                        SetActiveSort(SortType.Default);
                        break;

                    case SensorArea.E7:
                    case SensorArea.B7:
                    case SensorArea.C:
                    case SensorArea.B2:
                        EventSystem.current.SetSelectedGameObject(SearchBar.gameObject);
                        break;
                    case SensorArea.E3:
                        SearchBar.text = string.Empty;
                        break;

                    case SensorArea.D5:
                        InputManager.UnbindAnyArea(OnAreaDown);
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
            var task = SongStorage.SortAndFindAsync(SearchBar.text, sortType);
            MajInstances.SceneSwitcher.SwitchSceneAfterTaskAsync("List", task).Forget();
        }
    }
}