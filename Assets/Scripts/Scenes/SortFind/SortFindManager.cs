using UnityEngine;
using TMPro;
using MajdataPlay.IO;
using MajdataPlay.Editor;

namespace MajdataPlay.Scenes.SortFind
{
#nullable enable
    public class SortFindManager : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI _slotText;
        [SerializeField]
        TMP_InputField _searchBar;
        [SerializeField]
        GameObject _clearButton;

        [SerializeField, ReadOnlyField]
        SortType _sortType;

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
            Focus(_searchBar);
            CabinetLed.SetAllLight(Color.black);
            _searchBar.text = SongStorage.OrderBy.Keyword;
            _searchBar.onValueChanged.AddListener(_ => _clearButton.SetActive(!string.IsNullOrEmpty(_searchBar.text)));
            _clearButton.SetActive(!string.IsNullOrEmpty(_searchBar.text));
            _searchBar.onSubmit.AddListener(_ => SortAndExit());
            _selectIndex = (int)SongStorage.OrderBy.SortBy;
            SetActiveSort((SortType)_selectIndex);
        }
        void Update()
        {
            if (InputManager.IsSensorClickCompletedInThisFrame(SensorArea.D5) ||
                InputManager.IsButtonClickCompletedInThisFrame(ButtonZone.P1) ||
                _isExited)
            {
                SortAndExit();
            }
            else if (InputManager.IsSensorPressedInThisFrame(SensorArea.E6)||
                     InputManager.IsSensorPressedInThisFrame(SensorArea.B5)) // <
            {
                _selectIndex--;
                if(_selectIndex < 0)
                {
                    _selectIndex = _slots.Length - 1;
                }
                SetActiveSort((SortType)_selectIndex);
            }
            else if (InputManager.IsSensorPressedInThisFrame(SensorArea.B4) ||
                     InputManager.IsSensorPressedInThisFrame(SensorArea.E4)) // >
            {
                _selectIndex++;
                if (_selectIndex > _slots.Length - 1)
                {
                    _selectIndex = 0;
                }
                SetActiveSort((SortType)_selectIndex);
            }
            
            if (InputManager.IsSensorPressedInThisFrame(SensorArea.E3))
            {
                _searchBar.text = string.Empty;
            }
            else if (InputManager.IsSensorPressedInThisFrame(SensorArea.E7) ||
                     InputManager.IsSensorPressedInThisFrame(SensorArea.B7) ||
                     InputManager.IsSensorPressedInThisFrame(SensorArea.C) ||
                     InputManager.IsSensorPressedInThisFrame(SensorArea.B2))
            {
                Focus(_searchBar);
            }
        }

        static void Focus(TMP_InputField inputField)
        {
            inputField.Select();
            inputField.ActivateInputField();
            inputField.MoveTextEnd(false);
        }

        void SetActiveSort(SortType sortType)
        {
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
