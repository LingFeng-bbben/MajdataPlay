using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Utils
{
    internal static class LegacyInputFieldHelper
    {
        public static void Focus(EventSystem eventSystem, InputField inputField)
        {
            eventSystem.SetSelectedGameObject(null!);
            eventSystem.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
            SetCaretPosition(inputField, inputField.text?.Length ?? 0);
        }

        public static void HandleMacOSKeyboardEvent(EventSystem? eventSystem, Event current, params InputField[] inputFields)
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (eventSystem is null || current.type is not EventType.KeyDown || current.alt)
            {
                return;
            }

            var selectedGameObject = eventSystem.currentSelectedGameObject;
            if (selectedGameObject is null)
            {
                return;
            }

            for (var i = 0; i < inputFields.Length; i++)
            {
                var inputField = inputFields[i];
                if (inputField is null || inputField.gameObject != selectedGameObject)
                {
                    continue;
                }
                if (!inputField.isActiveAndEnabled || !inputField.interactable || inputField.readOnly)
                {
                    return;
                }
                if (!inputField.isFocused)
                {
                    inputField.ActivateInputField();
                }

                if (TryHandleShortcut(current, inputField) ||
                    TryHandleEditingKey(current, inputField) ||
                    TryHandleTextInput(current, inputField))
                {
                    current.Use();
                }
                return;
            }
#endif
        }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        static bool TryHandleShortcut(Event current, InputField inputField)
        {
            if (!current.command && !current.control)
            {
                return false;
            }

            switch (current.keyCode)
            {
                case KeyCode.A:
                    inputField.selectionAnchorPosition = 0;
                    inputField.selectionFocusPosition = inputField.text.Length;
                    inputField.caretPosition = inputField.text.Length;
                    return true;
                case KeyCode.C:
                    {
                        var selectedText = GetSelectedText(inputField);
                        if (string.IsNullOrEmpty(selectedText))
                        {
                            return false;
                        }
                        GUIUtility.systemCopyBuffer = selectedText;
                        return true;
                    }
                case KeyCode.X:
                    {
                        var selectedText = GetSelectedText(inputField);
                        if (string.IsNullOrEmpty(selectedText))
                        {
                            return false;
                        }
                        GUIUtility.systemCopyBuffer = selectedText;
                        DeleteSelection(inputField);
                        return true;
                    }
                case KeyCode.V:
                    return InsertText(inputField, GUIUtility.systemCopyBuffer ?? string.Empty);
                default:
                    return false;
            }
        }

        static bool TryHandleEditingKey(Event current, InputField inputField)
        {
            switch (current.keyCode)
            {
                case KeyCode.Backspace:
                    return DeleteBackward(inputField);
                case KeyCode.Delete:
                    return DeleteForward(inputField);
                case KeyCode.LeftArrow:
                    MoveCaret(inputField, -1);
                    return true;
                case KeyCode.RightArrow:
                    MoveCaret(inputField, 1);
                    return true;
                case KeyCode.Home:
                    SetCaretPosition(inputField, 0);
                    return true;
                case KeyCode.End:
                    SetCaretPosition(inputField, inputField.text.Length);
                    return true;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    inputField.DeactivateInputField();
                    inputField.onEndEdit.Invoke(inputField.text);
                    return true;
                default:
                    return false;
            }
        }

        static bool TryHandleTextInput(Event current, InputField inputField)
        {
            if (current.command || current.control)
            {
                return false;
            }

            var character = current.character;
            if (character == '\0' || char.IsControl(character))
            {
                return false;
            }

            return InsertText(inputField, character.ToString());
        }

        static bool InsertText(InputField inputField, string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
            {
                return true;
            }

            var text = inputField.text ?? string.Empty;
            GetSelectionRange(inputField, out var selectionStart, out var selectionEnd);

            if (inputField.characterLimit > 0)
            {
                var removableCount = selectionEnd - selectionStart;
                var availableCount = inputField.characterLimit - (text.Length - removableCount);
                if (availableCount <= 0)
                {
                    return true;
                }
                if (rawText.Length > availableCount)
                {
                    rawText = rawText[..availableCount];
                }
            }

            var newText = text.Remove(selectionStart, selectionEnd - selectionStart).Insert(selectionStart, rawText);
            inputField.text = newText;
            SetCaretPosition(inputField, selectionStart + rawText.Length);
            inputField.ForceLabelUpdate();
            return true;
        }

        static bool DeleteBackward(InputField inputField)
        {
            GetSelectionRange(inputField, out var selectionStart, out var selectionEnd);
            if (selectionStart != selectionEnd)
            {
                DeleteSelection(inputField);
                return true;
            }
            if (selectionStart <= 0)
            {
                return true;
            }

            var text = inputField.text ?? string.Empty;
            inputField.text = text.Remove(selectionStart - 1, 1);
            SetCaretPosition(inputField, selectionStart - 1);
            inputField.ForceLabelUpdate();
            return true;
        }

        static bool DeleteForward(InputField inputField)
        {
            GetSelectionRange(inputField, out var selectionStart, out var selectionEnd);
            if (selectionStart != selectionEnd)
            {
                DeleteSelection(inputField);
                return true;
            }

            var text = inputField.text ?? string.Empty;
            if (selectionStart >= text.Length)
            {
                return true;
            }

            inputField.text = text.Remove(selectionStart, 1);
            SetCaretPosition(inputField, selectionStart);
            inputField.ForceLabelUpdate();
            return true;
        }

        static void DeleteSelection(InputField inputField)
        {
            GetSelectionRange(inputField, out var selectionStart, out var selectionEnd);
            var text = inputField.text ?? string.Empty;
            inputField.text = text.Remove(selectionStart, selectionEnd - selectionStart);
            SetCaretPosition(inputField, selectionStart);
            inputField.ForceLabelUpdate();
        }

        static void MoveCaret(InputField inputField, int offset)
        {
            GetSelectionRange(inputField, out var selectionStart, out var selectionEnd);
            if (selectionStart != selectionEnd && offset < 0)
            {
                SetCaretPosition(inputField, selectionStart);
                return;
            }
            if (selectionStart != selectionEnd && offset > 0)
            {
                SetCaretPosition(inputField, selectionEnd);
                return;
            }

            var nextPosition = Mathf.Clamp(inputField.caretPosition + offset, 0, inputField.text.Length);
            SetCaretPosition(inputField, nextPosition);
        }

        static string GetSelectedText(InputField inputField)
        {
            GetSelectionRange(inputField, out var selectionStart, out var selectionEnd);
            if (selectionStart == selectionEnd)
            {
                return string.Empty;
            }

            var text = inputField.text ?? string.Empty;
            return text.Substring(selectionStart, selectionEnd - selectionStart);
        }

        static void GetSelectionRange(InputField inputField, out int selectionStart, out int selectionEnd)
        {
            selectionStart = Mathf.Min(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
            selectionEnd = Mathf.Max(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
        }

        static void SetCaretPosition(InputField inputField, int position)
        {
            position = Mathf.Clamp(position, 0, inputField.text.Length);
            inputField.caretPosition = position;
            inputField.selectionAnchorPosition = position;
            inputField.selectionFocusPosition = position;
        }
#endif
    }
}
