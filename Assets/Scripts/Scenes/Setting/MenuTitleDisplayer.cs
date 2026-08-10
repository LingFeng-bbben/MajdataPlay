using MajdataPlay.Extensions;
using MajdataPlay.i18n;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public sealed class MenuTitleDisplayer : MonoBehaviour
    {
        const float X_POS_STEP = 164f;
        const float X_POS_WITH_DELTA_1 = 218f;

        [SerializeField]
        Image _unselectedBackground = null!;

        [SerializeField]
        Image _selectedBackground = null!;

        [SerializeField]
        TextMeshProUGUI _titleDisplayer = null!;

        [SerializeField]
        Color _selectedTextColor = Color.white;

        [SerializeField]
        float _selectedFontSizeMax = 32f;

        RectTransform _rectTransform = null!;
        RectTransform _titleRectTransform = null!;
        Vector2 _unselectedSize;
        Vector2 _selectedSize;
        Vector2 _unselectedTitleSizeDelta;
        Color _unselectedBackgroundColor;
        Color _selectedBackgroundColor;
        Color _unselectedTextColor;
        float _unselectedFontSizeMax;
        float _selectedTitleScale = 1f;
        string _localizationKey = string.Empty;

        void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _titleRectTransform = _titleDisplayer.rectTransform;
            _unselectedSize = _rectTransform.rect.size;
            _selectedSize = _selectedBackground.rectTransform.rect.size;
            _unselectedTitleSizeDelta = _titleRectTransform.sizeDelta;
            _unselectedBackgroundColor = _unselectedBackground.color;
            _selectedBackgroundColor = _selectedBackground.color;
            _unselectedTextColor = _titleDisplayer.color;
            _unselectedFontSizeMax = _titleDisplayer.fontSizeMax;
            _selectedTitleScale = _selectedFontSizeMax / Mathf.Max(1f, _unselectedFontSizeMax);
        }

        void OnEnable()
        {
            Localization.OnLanguageChanged += OnLanguageChanged;
            UpdateLocalizedText();
        }

        void OnDisable()
        {
            Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        internal void Initialize(string localizationKey)
        {
            _localizationKey = localizationKey;
            UpdateLocalizedText();
        }

        internal void SetDistance(float distance)
        {
            var absDistance = Mathf.Abs(distance);
            var selectedAmount = 1f - Mathf.Clamp01(absDistance);

            _rectTransform.anchoredPosition = new Vector2(GetHorizontalPosition(distance, absDistance), 0);
            _rectTransform.sizeDelta = Vector2.Lerp(_unselectedSize, _selectedSize, selectedAmount);
            _titleRectTransform.sizeDelta = Vector2.Lerp(_unselectedTitleSizeDelta, Vector2.zero, selectedAmount);

            _unselectedBackground.color = WithAlpha(
                _unselectedBackgroundColor,
                _unselectedBackgroundColor.a * (1f - selectedAmount));
            _selectedBackground.color = WithAlpha(
                _selectedBackgroundColor,
                _selectedBackgroundColor.a * selectedAmount);

            var textColor = Color.Lerp(_unselectedTextColor, _selectedTextColor, selectedAmount);
            _titleDisplayer.color = textColor;
            var titleScale = Mathf.Lerp(1f, _selectedTitleScale, selectedAmount);
            _titleRectTransform.localScale = new Vector3(titleScale, titleScale, 1f);
        }

        internal void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }

        void OnLanguageChanged(object? sender, Language language)
        {
            UpdateLocalizedText();
        }

        void UpdateLocalizedText()
        {
            if (!string.IsNullOrEmpty(_localizationKey))
            {
                _titleDisplayer.text = _localizationKey.i18n();
            }
        }

        static float GetHorizontalPosition(float distance, float absDistance)
        {
            if (absDistance <= 1f)
            {
                return X_POS_WITH_DELTA_1 * distance;
            }

            return Mathf.Sign(distance) * (X_POS_WITH_DELTA_1 + X_POS_STEP * (absDistance - 1f));
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
