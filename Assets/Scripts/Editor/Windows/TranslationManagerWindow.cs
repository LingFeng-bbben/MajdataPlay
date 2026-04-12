using MajdataPlay.Buffers;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace MajdataPlay.Editor.Windows
{
    public class TranslationManagerWindow : EditorWindow
    {

        VisualElement _leftRoot;
        ListView _translatiosList;

        VisualElement _rightRoot;
        ScrollView _keyValuePairViewer;

        [SerializeField]
        List<string> _templateContent = new();

        Language[] _languages = Array.Empty<Language>();

        SerializedObject _windowSerializedObject;
        List<VisualElement> _tabPages = new();

        void Awake()
        {
            var langJsonPaths = Resources.LoadAll<TextAsset>("Langs");
            var templateFile = Resources.Load<TextAsset>("Langs/template.tpl");
            if(templateFile != null)
            {
                if(Serializer.Json.TryDeserialize<string[]>(templateFile.text, out var templateContent, out var e))
                {
                    _templateContent.AddRange(templateContent);
                }
                else
                {
                    Debug.LogError($"Failed to parse template content: {e}");
                }
            }
            _windowSerializedObject = new SerializedObject(this);
            using var jsons = new RentedList<string>();
            foreach (var lang in langJsonPaths)
            {
                if (lang == null || lang.name.EndsWith(".tpl"))
                {
                    continue;
                }
                jsons.Add(lang.text);
            }
            var langs = Localization.Parse(jsons);
            _languages = langs;
        }

        void CreateTranslationEditorTab(VisualElement rootElement)
        {
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

            rootElement.Add(splitView);

            _leftRoot = new VisualElement();
            splitView.Add(_leftRoot);

            var header = new VisualElement();
            header.style.height = 24;
            header.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            header.style.justifyContent = Justify.Center;
            _leftRoot.Add(header);

            var label = new Label("Translations");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.paddingLeft = 6;
            header.Add(label);

            _translatiosList = new ListView();
            _translatiosList.selectionType = SelectionType.Single;
            _translatiosList.makeItem = () =>
            {
                var label = new Label();
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.paddingLeft = 6;
                return label;
            };
            _translatiosList.bindItem = (element, index) =>
            {
                if (index < 0 || index >= _languages.Length)
                {
                    return;
                }
                var lang = _languages[index];
                if (element is Label label)
                {
                    label.text = lang.Code;
                }
            };
            _translatiosList.itemsSource = _languages;
            _translatiosList.selectionChanged += OnSelectionChanged;
            _translatiosList.RegisterCallback<FocusOutEvent>(_ =>
            {
                _translatiosList.SetSelectionWithoutNotify(Array.Empty<int>());
            });
            _leftRoot.Add(_translatiosList);



            _rightRoot = new VisualElement();
            splitView.Add(_rightRoot);

            _keyValuePairViewer = new();
            _rightRoot.Add(_keyValuePairViewer);
        }
        void CreateTemplateEditorTab(VisualElement rootElement)
        {
            //var listView = new ListView();
            //listView.itemsSource = _templateContent;
            //listView.makeItem = () =>
            //{
            //    var container = new VisualElement();
            //    var keyField = new TextField("Key");
            //    container.Add(keyField);
            //    return container;
            //};
            //listView.bindItem = (element, index) =>
            //{
            //    var k = _templateContent[index];

            //    var container = element;

            //    var keyField = container.ElementAt(0) as TextField;

            //    keyField.SetValueWithoutNotify(k);
            //};
            var property = _windowSerializedObject.FindProperty("_templateContent");

            var field = new PropertyField(property);
            field.Bind(_windowSerializedObject);
            var listView = new ListView
            {
                bindingPath = property.propertyPath,
                showFoldoutHeader = false,
                showAddRemoveFooter = true,
                reorderable = true
            };
            listView.Bind(_windowSerializedObject);
            rootElement.Add(listView);
        }

        void CreateGUI()
        {
            var toolbar = new Toolbar();

            var templateTabBtn = new ToolbarButton(() => SwitchTab(0)) { text = "Template" };
            var translationsTabBtn = new ToolbarButton(() => SwitchTab(1)) { text = "Translations" };

            toolbar.Add(templateTabBtn);
            toolbar.Add(translationsTabBtn);

            rootVisualElement.Add(toolbar);

            var templateTabPage = new VisualElement();
            templateTabPage.style.flexGrow = 1;
            CreateTemplateEditorTab(templateTabPage);
            _tabPages.Add(templateTabPage);
            rootVisualElement.Add(templateTabPage);

            var translationsTabPage = new VisualElement();
            translationsTabPage.style.flexGrow = 1;
            CreateTranslationEditorTab(translationsTabPage);
            _tabPages.Add(translationsTabPage);
            rootVisualElement.Add(translationsTabPage);

            SwitchTab(0);
        }
        void SwitchTab(int index)
        {
            if (index < 0 || index >= _tabPages.Count)
            {
                return;
            }
            for (var i = 0; i < _tabPages.Count; i++)
            {
                if(i != index)
                {
                    _tabPages[i].style.display = DisplayStyle.None;
                }
                else
                {
                    _tabPages[i].style.display = DisplayStyle.Flex;
                }
            }
        }
        void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = selectedItems.FirstOrDefault();
            if (selected is null)
            {
                return;
            }
        }

        [MenuItem("Window/Manage translations")]
        public static void ShowWindow()
        {
            var window = GetWindow<TranslationManagerWindow>();
            window.titleContent = new GUIContent("Translation editor");
        }
    }
}
