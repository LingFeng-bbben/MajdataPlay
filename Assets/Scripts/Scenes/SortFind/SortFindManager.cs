using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MajdataPlay.IO;
using UnityEngine.EventSystems;
using MajdataPlay.Utils;
using MajdataPlay.Editor;

namespace MajdataPlay.Scenes.SortFind
{
#nullable enable
    public class SortFindManager : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI _slotText;
        [SerializeField]
        InputField _searchBar;

        [SerializeField]
        Color _selectedColor;
        [SerializeField, ReadOnlyField]
        SortType _sortType;

        EventSystem _eventSystem;

        bool _isExited = false;
        int _selectIndex = (int)SortType.Default;

        readonly SortType[] _slots = new SortType[6];
        readonly string[] _slotTexts = new string[6]    
        {
            "MAJTEXT_SORTFIND_SortbyDefault",
            "MAJTEXT_SORTFIND_SortbyTime",
            "MAJTEXT_SORTFIND_SortbyDiff",
            "MAJTEXT_SORTFIND_SortbyDes",
            "MAJTEXT_SORTFIND_SortbyTitle",
            "MAJTEXT_SORTFIND_SortbyRank"
        };

        // Start is called before the first frame update
        void Start()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                _slots[i] = (SortType)i;
            }
            _eventSystem = EventSystem.current;
            _eventSystem.SetSelectedGameObject(_searchBar.gameObject);
            LedRing.SetAllLight(Color.black);
            _searchBar.text = SongStorage.OrderBy.Keyword;
            _selectIndex = (int)SongStorage.OrderBy.SortBy;
            SetActiveSort((SortType)_selectIndex);
        }
        void Update()
        {
            if (InputManager.IsSensorClickedInThisFrame(SensorArea.D5) || _isExited)
            {
                SortAndExit();
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E6)||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.B5)) // <
            {
                _selectIndex--;
                if(_selectIndex < 0)
                {
                    _selectIndex = _slots.Length - 1;
                }
                SetActiveSort((SortType)_selectIndex);
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.B4) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.E4)) // >
            {
                _selectIndex++;
                if (_selectIndex > _slots.Length - 1)
                {
                    _selectIndex = 0;
                }
                SetActiveSort((SortType)_selectIndex);
            }
            
            if (InputManager.IsSensorClickedInThisFrame(SensorArea.E3))
            {
                _searchBar.text = string.Empty;
            }
            else if (InputManager.IsSensorClickedInThisFrame(SensorArea.E7) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.B7) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.C) ||
                     InputManager.IsSensorClickedInThisFrame(SensorArea.B2))
            {
                _eventSystem.SetSelectedGameObject(_searchBar.gameObject);
            }
        }

        void SetActiveSort(SortType sortType)
        {
            _slotText.color = _selectedColor;
            _slotText.text = _slotTexts[(int)sortType].i18n();
            _sortType = sortType;
        }

        void SortAndExit()
        {
            if(_isExited)
            {
                return;
            }
            _isExited = true;
            var orderBy = SongStorage.OrderBy;
            orderBy.Keyword = _searchBar.text;
            orderBy.SortBy = _sortType;
            MajInstances.SceneSwitcher.SwitchScene("List", false);
        }
    }
}