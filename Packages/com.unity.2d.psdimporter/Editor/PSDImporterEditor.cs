#if UNITY_6000_1_OR_NEWER
#define ENABLE_2D_TILEMAP_EDITOR
#endif

using System;
using System.Collections.Generic;
using System.IO;
using PhotoshopFile;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.Common;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

#if ENABLE_2D_ANIMATION
using UnityEngine.U2D.Animation;
using UnityEditor.U2D.Animation;
#endif

namespace UnityEditor.U2D.PSD
{
    /// <summary>
    /// Inspector for PSDImporter
    /// </summary>
    [CustomEditor(typeof(PSDImporter))]
    [MovedFrom("UnityEditor.Experimental.AssetImporters")]
    [CanEditMultipleObjects]
    public class PSDImporterEditor : ScriptedImporterEditor, ITexturePlatformSettingsDataProvider
    {
        struct InspectorGUI
        {
            public VisualElement container;
            public bool needsRepaint;
            public Action onUpdate;
            public Action onUIActivated;
        }

        const string kReferencePref = "PSDImporterPreviewShowReference";

        SerializedProperty m_TextureType;
        SerializedProperty m_TextureShape;
        SerializedProperty m_SpriteMode;
        SerializedProperty m_SpritePixelsToUnits;
        SerializedProperty m_SpriteMeshType;
        SerializedProperty m_SpriteExtrude;
        SerializedProperty m_Alignment;
        SerializedProperty m_SpritePivot;
        SerializedProperty m_NPOTScale;
        SerializedProperty m_IsReadable;
        SerializedProperty m_sRGBTexture;
        SerializedProperty m_AlphaSource;
        SerializedProperty m_Swizzle;
#if ENABLE_TEXTURE_STREAMING
        SerializedProperty m_StreamingMipmaps;
        SerializedProperty m_StreamingMipmapsPriority;
#endif
        SerializedProperty m_MipMapMode;
        SerializedProperty m_EnableMipMap;
        SerializedProperty m_FadeOut;
        SerializedProperty m_BorderMipMap;
        SerializedProperty m_MipMapsPreserveCoverage;
        SerializedProperty m_AlphaTestReferenceValue;
        SerializedProperty m_MipMapFadeDistanceStart;
        SerializedProperty m_MipMapFadeDistanceEnd;
        SerializedProperty m_AlphaIsTransparency;
        SerializedProperty m_FilterMode;
        SerializedProperty m_Aniso;

        SerializedProperty m_WrapU;
        SerializedProperty m_WrapV;
        SerializedProperty m_WrapW;
        SerializedProperty m_ConvertToNormalMap;
        SerializedProperty m_MosaicLayers;
        SerializedProperty m_ImportHiddenLayers;
        SerializedProperty m_ResliceFromLayer;
        SerializedProperty m_CharacterMode;
        SerializedProperty m_DocumentPivot;
        SerializedProperty m_DocumentAlignment;
        SerializedProperty m_GenerateGOHierarchy;
        SerializedProperty m_KeepDupilcateSpriteName;
        SerializedProperty m_GeneratePhysicsShape;
        SerializedProperty m_LayerMappingOption;
        SerializedProperty m_Padding;
        SerializedProperty m_SpriteSizeExpand;
        SerializedProperty m_SpriteSizeExpandChanged;

#if ENABLE_2D_TILEMAP_EDITOR
        SerializedProperty m_GenerateTileAssets;
        SerializedProperty m_TilePaletteCellLayout;
        SerializedProperty m_TilePaletteHexagonLayout;
        SerializedProperty m_TilePaletteCellSize;
        SerializedProperty m_TilePaletteCellSizing;
        SerializedProperty m_TransparencySortMode;
        SerializedProperty m_TransparencySortAxis;
        SerializedProperty m_TileTemplate;
#endif

#if ENABLE_2D_ANIMATION
        SerializedProperty m_PaperDollMode;
        SerializedProperty m_SkeletonAssetReferenceID;
#endif

        uint m_SpriteSizePreviousSize;

        readonly int[] m_FilterModeOptions = (int[])(Enum.GetValues(typeof(FilterMode)));
        static readonly int s_SwizzleFieldHash = "SwizzleField".GetHashCode();

        bool m_IsPOT = false;
        Dictionary<TextureImporterType, Action[]> m_AdvanceInspectorGUI = new Dictionary<TextureImporterType, Action[]>();
        int m_PlatformSettingsIndex;
        bool m_ShowPerAxisWrapModes = false;
        int m_ActiveEditorIndex = 0;

        TexturePlatformSettingsHelper m_TexturePlatformSettingsHelper;

        PSDImporterEditorFoldOutState m_EditorFoldOutState = new PSDImporterEditorFoldOutState();
        InspectorGUI[] m_InspectorUI;
        PSDImporter m_CurrentTarget;
        bool m_ShowPivot;
        PSDGameObjectPreviewData m_PreviewRenderUtility;
        PSDImporterLayerManagementMultiColumnTreeView m_LayerManagementTreeView;
        IMGUIContainer m_LayerManagementSettingsContainer;
        IMGUIContainer m_ApplyRevertGUIVisualElement;
        VisualElement m_InspectorSettingsView;
        VisualElement m_MultiSupportForLayerManagementNotSupported;
        VisualElement m_RootVisualElement;
        IMGUIContainer m_ToolbarContainer;
        ScrollView m_InspectorScrollView;
        int m_LayerTreeViewUpdateCount = 0;
        SerializedProperty m_PlatformSettingsArrProp;
        SerializedProperty m_Pipeline;

#if ENABLE_2D_ANIMATION
        SkeletonAsset m_SkeletonAsset;
#endif

        /// <summary>
        /// Implementation of AssetImporterEditor.OnEnable
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();
            m_MosaicLayers = serializedObject.FindProperty("m_MosaicLayers");
            m_ImportHiddenLayers = serializedObject.FindProperty("m_ImportHiddenLayers");
            m_ResliceFromLayer = serializedObject.FindProperty("m_ResliceFromLayer");
            m_CharacterMode = serializedObject.FindProperty("m_CharacterMode");
            m_DocumentPivot = serializedObject.FindProperty("m_DocumentPivot");
            m_DocumentAlignment = serializedObject.FindProperty("m_DocumentAlignment");
            m_GenerateGOHierarchy = serializedObject.FindProperty("m_GenerateGOHierarchy");
            m_KeepDupilcateSpriteName = serializedObject.FindProperty("m_KeepDupilcateSpriteName");
            m_GeneratePhysicsShape = serializedObject.FindProperty("m_GeneratePhysicsShape");
            m_LayerMappingOption = serializedObject.FindProperty("m_LayerMappingOption");
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_SpriteSizeExpand = serializedObject.FindProperty("m_SpriteSizeExpand");
            m_SpriteSizePreviousSize = m_SpriteSizeExpand.uintValue;
            m_SpriteSizeExpandChanged = serializedObject.FindProperty("m_SpriteSizeExpandChanged");
            m_Pipeline = serializedObject.FindProperty("m_Pipeline");

            SerializedProperty textureImporterSettingsSP = serializedObject.FindProperty("m_TextureImporterSettings");
            m_TextureType = textureImporterSettingsSP.FindPropertyRelative("m_TextureType");
            m_TextureShape = textureImporterSettingsSP.FindPropertyRelative("m_TextureShape");
            m_ConvertToNormalMap = textureImporterSettingsSP.FindPropertyRelative("m_ConvertToNormalMap");
            m_SpriteMode = textureImporterSettingsSP.FindPropertyRelative("m_SpriteMode");
            m_SpritePixelsToUnits = textureImporterSettingsSP.FindPropertyRelative("m_SpritePixelsToUnits");
            m_SpriteMeshType = textureImporterSettingsSP.FindPropertyRelative("m_SpriteMeshType");
            m_SpriteExtrude = textureImporterSettingsSP.FindPropertyRelative("m_SpriteExtrude");
            m_Alignment = textureImporterSettingsSP.FindPropertyRelative("m_Alignment");
            m_SpritePivot = textureImporterSettingsSP.FindPropertyRelative("m_SpritePivot");
            m_NPOTScale = textureImporterSettingsSP.FindPropertyRelative("m_NPOTScale");
            m_IsReadable = textureImporterSettingsSP.FindPropertyRelative("m_IsReadable");
            m_sRGBTexture = textureImporterSettingsSP.FindPropertyRelative("m_sRGBTexture");
            m_AlphaSource = textureImporterSettingsSP.FindPropertyRelative("m_AlphaSource");
#if ENABLE_TEXTURE_STREAMING
            m_StreamingMipmaps = textureImporterSettingsSP.FindPropertyRelative("m_StreamingMipmaps");
            m_StreamingMipmapsPriority = textureImporterSettingsSP.FindPropertyRelative("m_StreamingMipmapsPriority");
#endif
            m_MipMapMode = textureImporterSettingsSP.FindPropertyRelative("m_MipMapMode");
            m_EnableMipMap = textureImporterSettingsSP.FindPropertyRelative("m_EnableMipMap");
            m_Swizzle = textureImporterSettingsSP.FindPropertyRelative("m_Swizzle");
            m_FadeOut = textureImporterSettingsSP.FindPropertyRelative("m_FadeOut");
            m_BorderMipMap = textureImporterSettingsSP.FindPropertyRelative("m_BorderMipMap");
            m_MipMapsPreserveCoverage = textureImporterSettingsSP.FindPropertyRelative("m_MipMapsPreserveCoverage");
            m_AlphaTestReferenceValue = textureImporterSettingsSP.FindPropertyRelative("m_AlphaTestReferenceValue");
            m_MipMapFadeDistanceStart = textureImporterSettingsSP.FindPropertyRelative("m_MipMapFadeDistanceStart");
            m_MipMapFadeDistanceEnd = textureImporterSettingsSP.FindPropertyRelative("m_MipMapFadeDistanceEnd");
            m_AlphaIsTransparency = textureImporterSettingsSP.FindPropertyRelative("m_AlphaIsTransparency");
            m_FilterMode = textureImporterSettingsSP.FindPropertyRelative("m_FilterMode");
            m_Aniso = textureImporterSettingsSP.FindPropertyRelative("m_Aniso");
            m_WrapU = textureImporterSettingsSP.FindPropertyRelative("m_WrapU");
            m_WrapV = textureImporterSettingsSP.FindPropertyRelative("m_WrapV");
            m_WrapW = textureImporterSettingsSP.FindPropertyRelative("m_WrapW");
            m_PlatformSettingsArrProp = extraDataSerializedObject.FindProperty("platformSettings");

            foreach (UnityEngine.Object t in targets)
            {
                m_IsPOT &= ((PSDImporter)t).isNPOT;
            }

#if ENABLE_2D_ANIMATION
            m_PaperDollMode = serializedObject.FindProperty("m_PaperDollMode");
            m_SkeletonAssetReferenceID = serializedObject.FindProperty("m_SkeletonAssetReferenceID");

            string assetPath = AssetDatabase.GUIDToAssetPath(m_SkeletonAssetReferenceID.stringValue);
            m_SkeletonAsset = AssetDatabase.LoadAssetAtPath<SkeletonAsset>(assetPath);
#endif

#if ENABLE_2D_TILEMAP_EDITOR
            m_GenerateTileAssets = serializedObject.FindProperty("m_GenerateTileAssets");
            m_TilePaletteCellLayout = serializedObject.FindProperty("m_TilePaletteCellLayout");
            m_TilePaletteHexagonLayout = serializedObject.FindProperty("m_TilePaletteHexagonLayout");
            m_TilePaletteCellSize = serializedObject.FindProperty("m_TilePaletteCellSize");
            m_TilePaletteCellSizing = serializedObject.FindProperty("m_TilePaletteCellSizing");
            m_TransparencySortMode = serializedObject.FindProperty("m_TransparencySortMode");
            m_TransparencySortAxis = serializedObject.FindProperty("m_TransparencySortAxis");
            m_TileTemplate = serializedObject.FindProperty("m_TileTemplate");
#endif

            Action[] advanceGUIAction = new Action[]
            {
                ColorSpaceGUI,
                AlphaHandlingGUI,
                POTScaleGUI,
                ReadableGUI,
                MipMapGUI,
                SwizzleGUI,
                CustomPipelineGUI,
            };
            m_AdvanceInspectorGUI.Add(TextureImporterType.Sprite, advanceGUIAction);

            advanceGUIAction = new Action[]
            {
                POTScaleGUI,
                ReadableGUI,
                MipMapGUI,
                SwizzleGUI,
                CustomPipelineGUI,
            };
            m_AdvanceInspectorGUI.Add(TextureImporterType.Default, advanceGUIAction);

            m_TexturePlatformSettingsHelper = new TexturePlatformSettingsHelper(this);

            m_InspectorUI = new[]
            {
                new InspectorGUI()
                {
                    container = new IMGUIContainer(DoInspectorSettings)
                    {
                      name = "DoSettingsUI"
                    },
                    needsRepaint = false,
                },
                new InspectorGUI()
                {
                    container = CreateLayerManagementUI(),
                    needsRepaint = false,
                    onUpdate = OnLayerManagementUIUpdate,
                    onUIActivated = OnLayerManagementViewActivated
                }
            };
            m_ActiveEditorIndex = Mathf.Max(EditorPrefs.GetInt(this.GetType().Name + "ActiveEditorIndex", 0), 0);
            m_ActiveEditorIndex %= m_InspectorUI.Length;
            UpdateLayerTreeView();
            m_ShowPivot = EditorPrefs.GetBool(kReferencePref, true);
            InitPreview();
        }

        void CustomPipelineGUI()
        {
            if (Unsupported.IsDeveloperMode())
            {
                EditorGUILayout.PropertyField(m_Pipeline);
            }
        }

        /// <summary>
        /// Override for AssetImporter.extraDataType
        /// </summary>
        protected override Type extraDataType => typeof(PSDImporterEditorExternalData);

        /// <summary>
        /// Override for AssetImporter.InitializeExtraDataInstance
        /// </summary>
        /// <param name="extraTarget">Target object</param>
        /// <param name="targetIndex">Target index</param>
        protected override void InitializeExtraDataInstance(UnityEngine.Object extraTarget, int targetIndex)
        {
            PSDImporter importer = targets[targetIndex] as PSDImporter;
            PSDImporterEditorExternalData extraData = extraTarget as PSDImporterEditorExternalData;
            List<TextureImporterPlatformSettings> platformSettingsNeeded = TexturePlatformSettingsHelper.PlatformSettingsNeeded(this);
            if (importer != null)
            {
                extraData.Init(importer, platformSettingsNeeded);
            }

        }

        void OnLayerManagementUIUpdate()
        {
            if (m_LayerManagementTreeView != null)
                m_LayerManagementTreeView.Update();
        }

        void OnLayerManagementViewActivated()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                m_MultiSupportForLayerManagementNotSupported.SetHiddenFromLayout(false);
                m_LayerManagementTreeView.SetHiddenFromLayout(true);
                m_LayerManagementSettingsContainer.SetHiddenFromLayout(true);
            }
            else
            {
                m_MultiSupportForLayerManagementNotSupported.SetHiddenFromLayout(true);
                m_LayerManagementTreeView.SetHiddenFromLayout(false);
                m_LayerManagementSettingsContainer.SetHiddenFromLayout(false);
            }
        }

        VisualElement CreateLayerManagementUI()
        {
            VisualElement ve = new VisualElement();
            m_MultiSupportForLayerManagementNotSupported = new Label()
            {
                name = "MultiSupportForLayerManagementNotSupported",
                text = styles.multiEditLayerManagementNotSupported.text
            };
            m_LayerManagementSettingsContainer = new IMGUIContainer(DoLayerManagementUI)
            {
                name = "LayerManagementSettings",
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0
                }
            };

            m_LayerManagementTreeView = new PSDImporterLayerManagementMultiColumnTreeView(serializedObject)
            {
                name = "LayerManagementTreeView"
            };
            m_LayerManagementTreeView.RegisterCallback<AttachToPanelEvent>(OnTreeViewAttachedToPanel);

            m_ApplyRevertGUIVisualElement = new IMGUIContainer(ApplyRevertGUI)
            {
                name = "LayerManagementApplyRevertGUI",
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0
                }
            };
            m_ApplyRevertGUIVisualElement.RegisterCallback<GeometryChangedEvent>(OnUpdateLayerTreeViewHeight);
            ve.Add(m_MultiSupportForLayerManagementNotSupported);
            ve.Add(m_LayerManagementSettingsContainer);
            ve.Add(m_LayerManagementTreeView);
            ve.Add(m_ApplyRevertGUIVisualElement);
            return ve;
        }

        void OnTreeViewAttachedToPanel(AttachToPanelEvent evt)
        {
            m_InspectorScrollView = m_RootVisualElement.panel.visualTree.Q<ScrollView>();
            m_InspectorScrollView.RegisterCallback<GeometryChangedEvent>(OnUpdateLayerTreeViewHeight);
        }

        void OnUpdateLayerTreeViewHeight(GeometryChangedEvent g)
        {
            ++m_LayerTreeViewUpdateCount;
        }

        void UpdateLayerTreeViewHeight()
        {
            // var h = m_InspectorScrollView.worldBound.height - (m_LayerManagementTreeView.worldBound.y - m_InspectorScrollView.worldBound.y) - m_ApplyRevertGUIVisualElement.worldBound.height;
            // if (h != m_LayerManagementTreeView.style.height)
            // {
            //     m_LayerManagementTreeView.style.height = h;
            //     m_LayerManagementTreeView.MarkDirtyRepaint();
            // }
        }

        void InitPreview()
        {
            PSDImporter t = (PSDImporter)target;
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(t.assetPath);

            if (m_PreviewRenderUtility != null)
            {
                m_PreviewRenderUtility.Dispose();
                m_PreviewRenderUtility = null;
            }

            if (gameObject != null)
            {
                Rect documentSize = new Rect(0, 0, t.importData.documentSize.x / t.pixelsPerUnit, t.importData.documentSize.y / t.pixelsPerUnit);
                Vector3 pivot = (Vector3)ImportUtilities.GetPivotPoint(documentSize, (SpriteAlignment)m_DocumentAlignment.intValue, m_DocumentPivot.vector2Value);
                documentSize.x = -pivot.x;
                documentSize.y = -pivot.y;
                m_PreviewRenderUtility = new PSDGameObjectPreviewData(gameObject, m_ShowPivot, documentSize);
            }
        }

        /// <summary>
        /// Implmentation of AssetImporterEditor.OnDisable
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();
            if (m_PreviewRenderUtility != null)
            {
                m_PreviewRenderUtility.Dispose();
                m_PreviewRenderUtility = null;
            }
            if (m_RootVisualElement != null)
                m_RootVisualElement.Clear();

            if (m_LayerManagementTreeView != null)
                m_LayerManagementTreeView.UnregisterCallback<AttachToPanelEvent>(OnTreeViewAttachedToPanel);
            if (m_ApplyRevertGUIVisualElement != null)
                m_ApplyRevertGUIVisualElement.UnregisterCallback<GeometryChangedEvent>(OnUpdateLayerTreeViewHeight);
            if (m_InspectorScrollView != null)
                m_InspectorScrollView.UnregisterCallback<GeometryChangedEvent>(OnUpdateLayerTreeViewHeight);
        }

        void UpdateLayerTreeView()
        {
            if (!ReferenceEquals(m_CurrentTarget, target))
            {
                m_CurrentTarget = (PSDImporter)target;
                m_LayerManagementTreeView.UpdateTreeView(serializedObject);
            }
        }

        /// <summary>
        /// Override from AssetImporterEditor.RequiresConstantRepaint
        /// </summary>
        /// <returns>Returns true when in Layer Management tab for UI feedback update, false otherwise.</returns>
        public override bool RequiresConstantRepaint()
        {
            return m_InspectorUI[m_ActiveEditorIndex].needsRepaint;
        }

        void DoInspectorSettings()
        {
            serializedObject.Update();
            extraDataSerializedObject.Update();

            DoSettingsUI();

            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();

            ApplyRevertGUI();
        }

        /// <summary>
        /// Implementation of virtual method CreateInspectorGUI.
        /// </summary>
        /// <returns>VisualElement container for Inspector visual.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            StyleSheet styleSheet = EditorGUIUtility.Load("packages/com.unity.2d.psdimporter/Editor/Assets/UI/PSDImporterStylesheet.uss") as StyleSheet;
            m_RootVisualElement = new VisualElement()
            {
                name = "Root"
            };
            if (EditorGUIUtility.isProSkin)
                m_RootVisualElement.AddToClassList("psdimporter-editor-dark");
            else
                m_RootVisualElement.AddToClassList("psdimporter-editor-light");
            m_RootVisualElement.styleSheets.Add(styleSheet);
            m_ToolbarContainer = new IMGUIContainer(DoToolBarIMGUI)
            {
                name = "Toolbar"
            };
            m_InspectorSettingsView = new VisualElement()
            {
                name = "InspectorSettings"
            };
            m_RootVisualElement.Add(m_ToolbarContainer);
            m_RootVisualElement.Add(m_InspectorSettingsView);
            m_RootVisualElement.schedule.Execute(VisualElementUpdate);
            ShowInspectorTab(m_ActiveEditorIndex);
            return m_RootVisualElement;
        }

        void VisualElementUpdate()
        {
            if (m_LayerTreeViewUpdateCount > 1)
                UpdateLayerTreeViewHeight();
            m_LayerTreeViewUpdateCount = 0;
            serializedObject.Update();
            extraDataSerializedObject.Update();
            try
            {
                if (m_InspectorUI[m_ActiveEditorIndex].onUpdate != null)
                    m_InspectorUI[m_ActiveEditorIndex].onUpdate.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log("Update:" + e);
            }

            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();
            m_RootVisualElement.schedule.Execute(VisualElementUpdate);
        }

        void ShowInspectorTab(int tab)
        {
            m_InspectorSettingsView.Clear();
            m_InspectorSettingsView.Add(m_InspectorUI[tab].container);
            m_InspectorUI[tab].onUIActivated?.Invoke();
        }

        void DoToolBarIMGUI()
        {
            GUILayout.Space(5);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
                {
                    m_ActiveEditorIndex = GUILayout.Toolbar(m_ActiveEditorIndex, styles.editorTabNames, "LargeButton", GUI.ToolbarButtonSize.FitToContents);
                    if (check.changed)
                    {
                        EditorPrefs.SetInt(GetType().Name + "ActiveEditorIndex", m_ActiveEditorIndex);
                        ShowInspectorTab(m_ActiveEditorIndex);
                    }
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(5);
        }

        void DoLayerManagementUI()
        {
            serializedObject.Update();
            extraDataSerializedObject.Update();

            EditorGUILayout.PropertyField(m_ImportHiddenLayers, styles.importHiddenLayer);

            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();

            Rect headerRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (Event.current.type == EventType.Repaint)
            {
                GUIStyle header = "RL Header";
                headerRect.height = EditorGUIUtility.singleLineHeight;
                header.Draw(headerRect, false, false, false, false);
                GUI.Label(headerRect, "Layer Import Settings");
            }
        }

        void DoSettingsUI()
        {
            if (m_EditorFoldOutState.DoGeneralUI(styles.generalHeaderText))
            {
                EditorGUI.showMixedValue = m_TextureType.hasMultipleDifferentValues;
                m_TextureType.intValue = EditorGUILayout.IntPopup(styles.textureTypeTitle, m_TextureType.intValue, styles.textureTypeOptions, styles.textureTypeValues);
                EditorGUI.showMixedValue = false;

                switch ((TextureImporterType)m_TextureType.intValue)
                {
                    case TextureImporterType.Sprite:
                        DoSpriteTextureTypeInspector();
                        break;
                    case TextureImporterType.Default:
                        DoTextureDefaultTextureTypeInspector();
                        break;
                    default:
                        m_TextureType.intValue = (int)TextureImporterType.Default;
                        break;
                }
                GUILayout.Space(5);
            }

            if ((TextureImporterType)m_TextureType.intValue == TextureImporterType.Sprite)
                DoSpriteInspector();
            CommonTextureSettingsGUI();
            DoPlatformSettings();
            DoAdvanceInspector();

        }

        void MainRigPropertyField()
        {
#if ENABLE_2D_ANIMATION
            EditorGUI.BeginChangeCheck();
            m_SkeletonAsset = EditorGUILayout.ObjectField(styles.mainSkeletonName, m_SkeletonAsset, typeof(SkeletonAsset), false) as SkeletonAsset;
            if (EditorGUI.EndChangeCheck())
            {
                string referencePath = AssetDatabase.GetAssetPath(m_SkeletonAsset);
                if (referencePath == ((AssetImporter)target).assetPath)
                    m_SkeletonAssetReferenceID.stringValue = "";
                else
                    m_SkeletonAssetReferenceID.stringValue = AssetDatabase.GUIDFromAssetPath(referencePath).ToString();
            }
#endif
        }

        /// <summary>
        /// Implementation of AssetImporterEditor.Apply
        /// </summary>
        protected override void Apply()
        {
            InternalEditorBridge.ApplySpriteEditorWindow();
            base.Apply();
            PSDImportPostProcessor.currentApplyAssetPath = ((PSDImporter)target).assetPath;
            if (m_PreviewRenderUtility != null)
            {
                m_PreviewRenderUtility.Dispose();
                m_PreviewRenderUtility = null;
            }
        }

        static bool IsPSD(PsdFile doc)
        {
            return !doc.IsLargeDocument;
        }

        static PsdColorMode FileColorMode(PsdFile doc)
        {
            return doc.ColorMode;
        }

        bool IsCharacterRigged()
        {
#if ENABLE_2D_ANIMATION
            PSDImporter importer = target as PSDImporter;
            if (importer != null)
            {
                ICharacterDataProvider characterProvider = importer.GetDataProvider<ICharacterDataProvider>();
                ISpriteMeshDataProvider meshDataProvider = importer.GetDataProvider<ISpriteMeshDataProvider>();
                if (characterProvider != null && meshDataProvider != null)
                {
                    CharacterData character = characterProvider.GetCharacterData();
                    foreach (CharacterPart parts in character.parts)
                    {
                        Vertex2DMetaData[] vert = meshDataProvider.GetVertices(new GUID(parts.spriteId));
                        int[] indices = meshDataProvider.GetIndices(new GUID(parts.spriteId));
                        if (parts.bones != null && parts.bones.Length > 0 &&
                            vert != null && vert.Length > 0 &&
                            indices != null && indices.Length > 0)
                            return true;
                    }
                }
            }
#endif
            return false;
        }

        void DoPlatformSettings()
        {
            if (m_EditorFoldOutState.DoPlatformSettingsUI(styles.platformSettingsHeaderText))
            {
                GUILayout.Space(5);
                m_TexturePlatformSettingsHelper.ShowPlatformSpecificSettings();
                GUILayout.Space(5);
            }
        }

        void DoAdvanceInspector()
        {
            if (!m_TextureType.hasMultipleDifferentValues)
            {
                if (m_AdvanceInspectorGUI.ContainsKey((TextureImporterType)m_TextureType.intValue))
                {
                    if (m_EditorFoldOutState.DoAdvancedUI(styles.advancedHeaderText))
                    {
                        foreach (Action action in m_AdvanceInspectorGUI[(TextureImporterType)m_TextureType.intValue])
                        {
                            action();
                        }
                    }
                }
            }
        }

        void CommonTextureSettingsGUI()
        {
            if (m_EditorFoldOutState.DoTextureUI(styles.textureHeaderText))
            {
                EditorGUI.BeginChangeCheck();

                // Wrap mode
                bool isVolume = false;
                WrapModePopup(m_WrapU, m_WrapV, m_WrapW, isVolume, ref m_ShowPerAxisWrapModes);


                // Display warning about repeat wrap mode on restricted npot emulation
                if (m_NPOTScale.intValue == (int)TextureImporterNPOTScale.None &&
                    (m_WrapU.intValue == (int)TextureWrapMode.Repeat || m_WrapV.intValue == (int)TextureWrapMode.Repeat) &&
                    !InternalEditorBridge.DoesHardwareSupportsFullNPOT())
                {
                    bool displayWarning = false;
                    foreach (UnityEngine.Object target in targets)
                    {
                        PSDImporter imp = (PSDImporter)target;
                        int w = imp.textureActualWidth;
                        int h = imp.textureActualHeight;
                        if (!Mathf.IsPowerOfTwo(w) || !Mathf.IsPowerOfTwo(h))
                        {
                            displayWarning = true;
                            break;
                        }
                    }

                    if (displayWarning)
                    {
                        EditorGUILayout.HelpBox(styles.warpNotSupportWarning.text, MessageType.Warning, true);
                    }
                }

                // Filter mode
                EditorGUI.showMixedValue = m_FilterMode.hasMultipleDifferentValues;
                FilterMode filter = (FilterMode)m_FilterMode.intValue;
                if ((int)filter == -1)
                {
                    if (m_FadeOut.intValue > 0 || m_ConvertToNormalMap.intValue > 0)
                        filter = FilterMode.Trilinear;
                    else
                        filter = FilterMode.Bilinear;
                }
                filter = (FilterMode)EditorGUILayout.IntPopup(styles.filterMode, (int)filter, styles.filterModeOptions, m_FilterModeOptions);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                    m_FilterMode.intValue = (int)filter;

                // Aniso
                bool showAniso = (FilterMode)m_FilterMode.intValue != FilterMode.Point
                    && m_EnableMipMap.intValue > 0
                    && (TextureImporterShape)m_TextureShape.intValue != TextureImporterShape.TextureCube;
                using (new EditorGUI.DisabledScope(!showAniso))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.showMixedValue = m_Aniso.hasMultipleDifferentValues;
                    int aniso = m_Aniso.intValue;
                    if (aniso == -1)
                        aniso = 1;
                    aniso = EditorGUILayout.IntSlider(styles.anisoLevelLabel, aniso, 0, 16);
                    EditorGUI.showMixedValue = false;
                    if (EditorGUI.EndChangeCheck())
                        m_Aniso.intValue = aniso;

                    if (aniso > 1)
                    {
                        if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Disable)
                            EditorGUILayout.HelpBox(styles.anisotropicDisableInfo.text, MessageType.Info);
                        else if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable)
                            EditorGUILayout.HelpBox(styles.anisotropicForceEnableInfo.text, MessageType.Info);
                    }
                }
                GUILayout.Space(5);
            }
        }

        private static bool IsAnyTextureObjectUsingPerAxisWrapMode(UnityEngine.Object[] objects, bool isVolumeTexture)
        {
            foreach (UnityEngine.Object o in objects)
            {
                int u = 0, v = 0, w = 0;
                // the objects can be Textures themselves, or texture-related importers
                if (o is Texture)
                {
                    Texture ti = (Texture)o;
                    u = (int)ti.wrapModeU;
                    v = (int)ti.wrapModeV;
                    w = (int)ti.wrapModeW;
                }
                if (o is TextureImporter)
                {
                    TextureImporter ti = (TextureImporter)o;
                    u = (int)ti.wrapModeU;
                    v = (int)ti.wrapModeV;
                    w = (int)ti.wrapModeW;
                }
                if (o is IHVImageFormatImporter)
                {
                    IHVImageFormatImporter ti = (IHVImageFormatImporter)o;
                    u = (int)ti.wrapModeU;
                    v = (int)ti.wrapModeV;
                    w = (int)ti.wrapModeW;
                }
                u = Mathf.Max(0, u);
                v = Mathf.Max(0, v);
                w = Mathf.Max(0, w);
                if (u != v)
                {
                    return true;
                }
                if (isVolumeTexture)
                {
                    if (u != w || v != w)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // showPerAxisWrapModes is state of whether "Per-Axis" mode should be active in the main dropdown.
        // It is set automatically if wrap modes in UVW are different, or if user explicitly picks "Per-Axis" option -- when that one is picked,
        // then it should stay true even if UVW wrap modes will initially be the same.
        //
        // Note: W wrapping mode is only shown when isVolumeTexture is true.
        static void WrapModePopup(SerializedProperty wrapU, SerializedProperty wrapV, SerializedProperty wrapW, bool isVolumeTexture, ref bool showPerAxisWrapModes)
        {
            // In texture importer settings, serialized properties for things like wrap modes can contain -1;
            // that seems to indicate "use defaults, user has not changed them to anything" but not totally sure.
            // Show them as Repeat wrap modes in the popups.
            TextureWrapMode wu = (TextureWrapMode)Mathf.Max(wrapU.intValue, 0);
            TextureWrapMode wv = (TextureWrapMode)Mathf.Max(wrapV.intValue, 0);
            TextureWrapMode ww = (TextureWrapMode)Mathf.Max(wrapW.intValue, 0);

            // automatically go into per-axis mode if values are already different
            if (wu != wv)
                showPerAxisWrapModes = true;
            if (isVolumeTexture)
            {
                if (wu != ww || wv != ww)
                    showPerAxisWrapModes = true;
            }

            // It's not possible to determine whether any single texture in the whole selection is using per-axis wrap modes
            // just from SerializedProperty values. They can only tell if "some values in whole selection are different" (e.g.
            // wrap value on U axis is not the same among all textures), and can return value of "some" object in the selection
            // (typically based on object loading order). So in order for more intuitive behavior with multi-selection,
            // we go over the actual objects when there's >1 object selected and some wrap modes are different.
            if (!showPerAxisWrapModes)
            {
                if (wrapU.hasMultipleDifferentValues || wrapV.hasMultipleDifferentValues || (isVolumeTexture && wrapW.hasMultipleDifferentValues))
                {
                    if (IsAnyTextureObjectUsingPerAxisWrapMode(wrapU.serializedObject.targetObjects, isVolumeTexture))
                    {
                        showPerAxisWrapModes = true;
                    }
                }
            }

            int value = showPerAxisWrapModes ? -1 : (int)wu;

            // main wrap mode popup
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = !showPerAxisWrapModes && (wrapU.hasMultipleDifferentValues || wrapV.hasMultipleDifferentValues || (isVolumeTexture && wrapW.hasMultipleDifferentValues));
            value = EditorGUILayout.IntPopup(styles.wrapModeLabel, value, styles.wrapModeContents, styles.wrapModeValues);
            if (EditorGUI.EndChangeCheck() && value != -1)
            {
                // assign the same wrap mode to all axes, and hide per-axis popups
                wrapU.intValue = value;
                wrapV.intValue = value;
                wrapW.intValue = value;
                showPerAxisWrapModes = false;
            }

            // show per-axis popups if needed
            if (value == -1)
            {
                showPerAxisWrapModes = true;
                EditorGUI.indentLevel++;
                WrapModeAxisPopup(styles.wrapU, wrapU);
                WrapModeAxisPopup(styles.wrapV, wrapV);
                if (isVolumeTexture)
                {
                    WrapModeAxisPopup(styles.wrapW, wrapW);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.showMixedValue = false;
        }

        static void WrapModeAxisPopup(GUIContent label, SerializedProperty wrapProperty)
        {
            // In texture importer settings, serialized properties for wrap modes can contain -1, which means "use default".
            TextureWrapMode wrap = (TextureWrapMode)Mathf.Max(wrapProperty.intValue, 0);
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(rect, label, wrapProperty);
            wrap = (TextureWrapMode)EditorGUI.EnumPopup(rect, label, wrap);
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                wrapProperty.intValue = (int)wrap;
            }
        }

        void DoWrapModePopup()
        {
            WrapModePopup(m_WrapU, m_WrapV, m_WrapW, IsVolume(), ref m_ShowPerAxisWrapModes);
        }

        bool IsVolume()
        {
            Texture t = target as Texture;
            return t != null && t.dimension == UnityEngine.Rendering.TextureDimension.Tex3D;
        }

        void DoSpriteTextureTypeInspector()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.IntPopup(m_SpriteMode, styles.spriteModeOptions, new[] { 1, 2, 3 }, styles.spriteMode);

            // Ensure that PropertyField focus will be cleared when we change spriteMode.
            if (EditorGUI.EndChangeCheck())
            {
                GUIUtility.keyboardControl = 0;
            }

            // Show generic attributes
            using (new EditorGUI.DisabledScope(m_SpriteMode.intValue == 0))
            {
                EditorGUILayout.PropertyField(m_SpritePixelsToUnits, styles.spritePixelsPerUnit);

                if (m_SpriteMode.intValue != (int)SpriteImportMode.Polygon && !m_SpriteMode.hasMultipleDifferentValues)
                {
                    EditorGUILayout.IntPopup(m_SpriteMeshType, styles.spriteMeshTypeOptions, new[] { 0, 1 }, styles.spriteMeshType);
                }

                EditorGUILayout.IntSlider(m_SpriteExtrude, 0, 32, styles.spriteExtrude);

                if (m_SpriteMode.intValue == 1)
                {
                    EditorGUILayout.IntPopup(m_Alignment, styles.spriteAlignmentOptions, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, styles.spriteAlignment);

                    if (m_Alignment.intValue == (int)SpriteAlignment.Custom)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(m_SpritePivot, new GUIContent());
                        GUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.PropertyField(m_GeneratePhysicsShape, styles.generatePhysicsShape);
            EditorGUILayout.PropertyField(m_ResliceFromLayer, styles.resliceFromLayer);
            if (m_ResliceFromLayer.boolValue)
            {
                EditorGUILayout.HelpBox(styles.resliceFromLayerWarning.text, MessageType.Info, true);
            }

            DoOpenSpriteEditorButton();
        }

        void DoSpriteInspector()
        {
            if (m_EditorFoldOutState.DoLayerImportUI(styles.layerImportHeaderText))
            {
                EditorGUILayout.PropertyField(m_ImportHiddenLayers, styles.importHiddenLayer);

                using (new EditorGUI.DisabledScope(m_SpriteMode.intValue != (int)SpriteImportMode.Multiple || m_SpriteMode.hasMultipleDifferentValues))
                {
                    using (new EditorGUI.DisabledScope(m_MosaicLayers.boolValue == false))
                    {
                        EditorGUILayout.PropertyField(m_KeepDupilcateSpriteName, styles.keepDuplicateSpriteName);

                        using (new EditorGUI.DisabledScope(!m_MosaicLayers.boolValue || !m_CharacterMode.boolValue))
                        {
                            EditorGUILayout.PropertyField(m_GenerateGOHierarchy, styles.layerGroupLabel);
                        }

                        EditorGUILayout.IntPopup(m_LayerMappingOption, styles.layerMappingOption, styles.layerMappingOptionValues, styles.layerMapping);
                    }

                    int boolToInt = !m_MosaicLayers.boolValue ? 1 : 0;
                    EditorGUI.BeginChangeCheck();
                    boolToInt = EditorGUILayout.IntPopup(styles.mosaicLayers, boolToInt, styles.importModeOptions, styles.falseTrueOptionValue);
                    if (EditorGUI.EndChangeCheck())
                        m_MosaicLayers.boolValue = boolToInt != 1;
                    using (new EditorGUI.DisabledScope(m_MosaicLayers.boolValue == false))
                    {
                        EditorGUILayout.IntSlider(m_Padding, 0, 32, styles.padding);

                        EditorGUILayout.PropertyField(m_SpriteSizeExpand, styles.spriteSizeExpand);
                    }
                }
                GUILayout.Space(5);
            }

#if ENABLE_2D_ANIMATION
            if (m_EditorFoldOutState.DoCharacterRigUI(styles.characterRigHeaderText))
            {
                using (new EditorGUI.DisabledScope(m_SpriteMode.intValue != (int)SpriteImportMode.Multiple || m_SpriteMode.hasMultipleDifferentValues || m_MosaicLayers.boolValue == false))
                {
                    EditorGUILayout.PropertyField(m_CharacterMode, styles.characterMode);
                    using (new EditorGUI.DisabledScope(!m_CharacterMode.boolValue))
                    {
                        MainRigPropertyField();
                        EditorGUILayout.IntPopup(m_DocumentAlignment, styles.spriteAlignmentOptions, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, styles.characterAlignment);
                        if (m_DocumentAlignment.intValue == (int)SpriteAlignment.Custom)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(EditorGUIUtility.labelWidth);
                            EditorGUILayout.PropertyField(m_DocumentPivot, new GUIContent());
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.Space(5);
                //EditorGUILayout.PropertyField(m_PaperDollMode, s_Styles.paperDollMode);
            }
#endif
#if ENABLE_2D_TILEMAP_EDITOR
            if (m_EditorFoldOutState.DoTilePaletteUI(styles.tilePaletteHeaderText))
            {
                EditorGUILayout.PropertyField(m_GenerateTileAssets, styles.generateTileAssets);
                using (new EditorGUI.DisabledScope(!m_GenerateTileAssets.boolValue))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_TilePaletteCellLayout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Set useful user settings for certain layouts
                        switch ((GridLayout.CellLayout)m_TilePaletteCellLayout.enumValueIndex)
                        {
                            case GridLayout.CellLayout.Rectangle:
                            {
                                m_TilePaletteCellSizing.intValue = (int)GridPalette.CellSizing.Automatic;
                                m_TilePaletteCellSize.vector3Value = new Vector3(1, 1, 0);
                                break;
                            }
                            case GridLayout.CellLayout.Hexagon:
                            {
                                m_TilePaletteCellSizing.intValue = (int)GridPalette.CellSizing.Manual;
                                m_TilePaletteCellSize.vector3Value = new Vector3(0.8659766f, 1, 0);
                                break;
                            }
                            case GridLayout.CellLayout.Isometric:
                            {
                                m_TilePaletteCellSizing.intValue = (int)GridPalette.CellSizing.Manual;
                                m_TilePaletteCellSize.vector3Value = new Vector3(1, 0.5f, 1);
                                break;
                            }
                            case GridLayout.CellLayout.IsometricZAsY:
                            {
                                m_TilePaletteCellSizing.intValue = (int)GridPalette.CellSizing.Manual;
                                m_TilePaletteCellSize.vector3Value = new Vector3(1, 0.5f, 1);
                                m_TransparencySortMode.intValue = (int)TransparencySortMode.CustomAxis;
                                m_TransparencySortAxis.vector3Value = new Vector3(0f, 1f, -0.26f);
                                break;
                            }
                        }
                    }

                    if (m_TilePaletteCellLayout.enumValueIndex == (int)GridLayout.CellLayout.Hexagon)
                    {
                        m_TilePaletteHexagonLayout.intValue = EditorGUILayout.Popup(Styles.tilePaletteHexagonLabel, m_TilePaletteHexagonLayout.intValue, Styles.tilePaletteHexagonSwizzleTypeLabel);
                    }

                    EditorGUILayout.PropertyField(m_TilePaletteCellSizing);
                    using (new EditorGUI.DisabledScope(m_TilePaletteCellSizing.enumValueIndex == (int)GridPalette.CellSizing.Automatic))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(Styles.tilePaletteCellSizeLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
                        EditorGUI.BeginChangeCheck();
                        Vector3 val = EditorGUILayout.Vector3Field(GUIContent.none, m_TilePaletteCellSize.vector3Value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_TilePaletteCellSize.vector3Value = val;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.PropertyField(m_TransparencySortMode);
                    using (new EditorGUI.DisabledScope(m_TransparencySortMode.enumValueIndex != (int)TransparencySortMode.CustomAxis))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(Styles.tilePaletteTransparencySortAxisLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
                        EditorGUI.BeginChangeCheck();
                        Vector3 val = EditorGUILayout.Vector3Field(GUIContent.none, m_TransparencySortAxis.vector3Value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_TransparencySortAxis.vector3Value = val;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.PropertyField(m_TileTemplate);
                }
            }
#endif
        }

        void DoOpenSpriteEditorButton()
        {
            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(styles.spriteEditorButtonLabel))
                {
                    if (HasModified())
                    {
                        // To ensure Sprite Editor Window to have the latest texture import setting,
                        // We must applied those modified values first.
                        string dialogText = string.Format(styles.unappliedSettingsDialogContent.text, ((AssetImporter)target).assetPath);
                        if (EditorUtility.DisplayDialog(styles.unappliedSettingsDialogTitle.text,
                            dialogText, styles.applyButtonLabel.text, styles.cancelButtonLabel.text))
                        {
                            SaveChanges();
                            InternalEditorBridge.ShowSpriteEditorWindow(this.assetTarget);

                            // We reimported the asset which destroyed the editor, so we can't keep running the UI here.
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        InternalEditorBridge.ShowSpriteEditorWindow(this.assetTarget);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Implementation of AssetImporterEditor.SaveChanges.
        /// </summary>
        public override void SaveChanges()
        {
            if (m_SpriteSizePreviousSize != m_SpriteSizeExpand.uintValue)
            {
                m_SpriteSizeExpandChanged.boolValue = true;
                serializedObject.ApplyModifiedProperties();
                extraDataSerializedObject.ApplyModifiedProperties();
                m_SpriteSizePreviousSize = m_SpriteSizeExpand.uintValue;
            }

            ApplyTexturePlatformSettings();

            serializedObject.ApplyModifiedProperties();
            extraDataSerializedObject.ApplyModifiedProperties();
            base.SaveChanges();
        }

        void ApplyTexturePlatformSettings()
        {
            for (int i = 0; i < targets.Length && i < extraDataTargets.Length; ++i)
            {
                PSDImporter psdImporter = (PSDImporter)targets[i];
                PSDImporterEditorExternalData externalData = (PSDImporterEditorExternalData)extraDataTargets[i];
                foreach (TextureImporterPlatformSettings ps in externalData.platformSettings)
                {
                    psdImporter.SetImporterPlatformSettings(ps);
                }
            }
        }
        void DoTextureDefaultTextureTypeInspector()
        {
            ColorSpaceGUI();
            AlphaHandlingGUI();
        }

        void ColorSpaceGUI()
        {
            ToggleFromInt(m_sRGBTexture, styles.sRGBTexture);
        }

        void POTScaleGUI()
        {
            using (new EditorGUI.DisabledScope(m_IsPOT || m_TextureType.intValue == (int)TextureImporterType.Sprite))
            {
                EnumPopup(m_NPOTScale, typeof(TextureImporterNPOTScale), styles.npot);
            }
        }

        void ReadableGUI()
        {
            ToggleFromInt(m_IsReadable, styles.readWrite);
        }

        void AlphaHandlingGUI()
        {
            EditorGUI.showMixedValue = m_AlphaSource.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int newAlphaUsage = EditorGUILayout.IntPopup(styles.alphaSource, m_AlphaSource.intValue, styles.alphaSourceOptions, styles.alphaSourceValues);

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                m_AlphaSource.intValue = newAlphaUsage;
            }

            bool showAlphaIsTransparency = (TextureImporterAlphaSource)m_AlphaSource.intValue != TextureImporterAlphaSource.None;
            using (new EditorGUI.DisabledScope(!showAlphaIsTransparency))
            {
                ToggleFromInt(m_AlphaIsTransparency, styles.alphaIsTransparency);
            }
        }

        void MipMapGUI()
        {
            ToggleFromInt(m_EnableMipMap, styles.generateMipMaps);

            if (m_EnableMipMap.boolValue && !m_EnableMipMap.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
                ToggleFromInt(m_BorderMipMap, styles.borderMipMaps);

#if ENABLE_TEXTURE_STREAMING
                ToggleFromInt(m_StreamingMipmaps, styles.streamingMipMaps);
                if (m_StreamingMipmaps.boolValue && !m_StreamingMipmaps.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_StreamingMipmapsPriority, styles.streamingMipmapsPriority);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_StreamingMipmapsPriority.intValue = Mathf.Clamp(m_StreamingMipmapsPriority.intValue, -128, 127);
                    }
                    EditorGUI.indentLevel--;
                }
#endif

                m_MipMapMode.intValue = EditorGUILayout.Popup(styles.mipMapFilter, m_MipMapMode.intValue, styles.mipMapFilterOptions);

                ToggleFromInt(m_MipMapsPreserveCoverage, styles.mipMapsPreserveCoverage);
                if (m_MipMapsPreserveCoverage.intValue != 0 && !m_MipMapsPreserveCoverage.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_AlphaTestReferenceValue, styles.alphaTestReferenceValue);
                    EditorGUI.indentLevel--;
                }

                // Mipmap fadeout
                ToggleFromInt(m_FadeOut, styles.mipmapFadeOutToggle);
                if (m_FadeOut.intValue > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    float min = m_MipMapFadeDistanceStart.intValue;
                    float max = m_MipMapFadeDistanceEnd.intValue;
                    EditorGUILayout.MinMaxSlider(styles.mipmapFadeOut, ref min, ref max, 0, 10);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_MipMapFadeDistanceStart.intValue = Mathf.RoundToInt(min);
                        m_MipMapFadeDistanceEnd.intValue = Mathf.RoundToInt(max);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        void ToggleFromInt(SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            int value = EditorGUILayout.Toggle(label, property.intValue > 0) ? 1 : 0;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                property.intValue = value;
        }

        void EnumPopup(SerializedProperty property, System.Type type, GUIContent label)
        {
            EditorGUILayout.IntPopup(label.text, property.intValue,
                System.Enum.GetNames(type),
                System.Enum.GetValues(type) as int[]);
        }

        void ExportMosaicTexture()
        {
            string assetPath = ((AssetImporter)target).assetPath;
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture2D == null)
                return;
            if (!texture2D.isReadable)
                texture2D = InternalEditorBridge.CreateTemporaryDuplicate(texture2D, texture2D.width, texture2D.height);
            Color[] pixelData = texture2D.GetPixels();
            texture2D = new Texture2D(texture2D.width, texture2D.height);
            texture2D.SetPixels(pixelData);
            texture2D.Apply();
            byte[] bytes = texture2D.EncodeToPNG();
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string filePath = Path.GetDirectoryName(assetPath);
            string savePath = Path.Combine(filePath, fileName + ".png");
            File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();
        }

        static void SwizzleField(SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(EditorGUILayout.BeginHorizontal(), label, property);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.numberField);
            int id = GUIUtility.GetControlID(s_SwizzleFieldHash, FocusType.Keyboard, rect);
            rect = EditorGUI.PrefixLabel(rect, id, label);
            uint value = property.uintValue;
            float w = (rect.width - 3 * EditorGUIUtility.standardVerticalSpacing) / 4;
            Rect subRect = new Rect(rect) { width = w };
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            for (int i = 0; i < 4; i++)
            {
                int shift = 8 * i;
                uint swz = (value >> shift) & 0xFF;
                swz = (uint)EditorGUI.Popup(subRect, (int)swz, styles.swizzleOptions);
                value &= ~(0xFFu << shift);
                value |= swz << shift;
                subRect.x += w + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel = oldIndent;

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                property.uintValue = value;
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndProperty();
        }

        void SwizzleGUI()
        {
            SwizzleField(m_Swizzle, styles.swizzle);
        }

        /// <summary>
        /// Implementation of AssetImporterEditor.ResetValues.
        /// </summary>
        [Obsolete("UnityUpgradeable () -> DiscardChanges")]
        protected override void ResetValues()
        {
            DiscardChanges();
        }

        /// <summary>
        /// Implementation of AssetImporterEditor.DiscardChanges.
        /// </summary>
        public override void DiscardChanges()
        {
            base.DiscardChanges();
            m_TexturePlatformSettingsHelper = new TexturePlatformSettingsHelper(this);
            m_LayerManagementTreeView.UpdateTreeView(serializedObject);
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.GetTargetCount.
        /// </summary>
        /// <returns>Returns the number of selected targets.</returns>
        int ITexturePlatformSettingsDataProvider.GetTargetCount()
        {
            return targets.Length;
        }

        /// <summary>
        /// ITexturePlatformSettingsDataProvider.GetPlatformTextureSettings.
        /// </summary>
        /// <param name="i">Selected target index.</param>
        /// <param name="name">Name of the platform.</param>
        /// <returns>TextureImporterPlatformSettings for the given platform name and selected target index.</returns>
        TextureImporterPlatformSettings ITexturePlatformSettingsDataProvider.GetPlatformTextureSettings(int i, string name)
        {
            PSDImporterEditorExternalData externalData = extraDataSerializedObject.targetObjects[i] as PSDImporterEditorExternalData;
            if (externalData != null)
            {
                foreach (TextureImporterPlatformSettings ps in externalData.platformSettings)
                {
                    if (ps.name == name)
                        return ps;
                }
            }
            return new TextureImporterPlatformSettings()
            {
                name = name,
                overridden = false
            };
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.ShowPresetSettings.
        /// </summary>
        /// <returns>True if valid asset is selected, false otherwiser.</returns>
        bool ITexturePlatformSettingsDataProvider.ShowPresetSettings()
        {
            return assetTarget == null;
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.DoesSourceTextureHaveAlpha.
        /// </summary>
        /// <param name="i">Index to selected target.</param>
        /// <returns>Always returns true since importer deals with source file that has alpha.</returns>
        bool ITexturePlatformSettingsDataProvider.DoesSourceTextureHaveAlpha(int i)
        {
            return true;
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.IsSourceTextureHDR.
        /// </summary>
        /// <param name="i">Index to selected target.</param>
        /// <returns>Always returns false since importer does not handle HDR textures.</returns>
        bool ITexturePlatformSettingsDataProvider.IsSourceTextureHDR(int i)
        {
            return false;
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.SetPlatformTextureSettings.
        /// </summary>
        /// <param name="i">Selected target index.</param>
        /// <param name="platformSettings">TextureImporterPlatformSettings to apply to target.</param>
        void ITexturePlatformSettingsDataProvider.SetPlatformTextureSettings(int i, TextureImporterPlatformSettings platformSettings)
        {
            PSDImporter psdImporter = ((PSDImporter)targets[i]);
            SerializedObject sp = new SerializedObject(psdImporter);
            sp.FindProperty("m_PlatformSettingsDirtyTick").longValue = System.DateTime.Now.Ticks;
            sp.ApplyModifiedProperties();
        }

        /// <summary>
        /// Implementation of ITexturePlatformSettingsDataProvider.GetImporterSettings.
        /// </summary>
        /// <param name="i">Selected target index.</param>
        /// <param name="settings">TextureImporterPlatformSettings reference for data retrieval.</param>
        void ITexturePlatformSettingsDataProvider.GetImporterSettings(int i, TextureImporterSettings settings)
        {
            ((PSDImporter)targets[i]).ReadTextureSettings(settings);
            // Get settings that have been changed in the inspector
            GetSerializedPropertySettings(settings);
        }

        /// <summary>
        /// Get the name property of TextureImporterPlatformSettings from a SerializedProperty.
        /// </summary>
        /// <param name="sp">The SerializedProperty to retrive data.</param>
        /// <returns>The name value in string.</returns>
        public string GetBuildTargetName(SerializedProperty sp)
        {
            return sp.FindPropertyRelative("m_Name").stringValue;
        }

        /// <summary>
        /// The SerializedProperty of an array of TextureImporterPlatformSettings.
        /// </summary>
        public SerializedProperty platformSettingsArray => m_PlatformSettingsArrProp;

        static (TextureImporterSwizzle r, TextureImporterSwizzle g, TextureImporterSwizzle b, TextureImporterSwizzle a) ConvertSwizzleRaw(uint value)
        {
            return ((TextureImporterSwizzle)((int)value & (int)byte.MaxValue),
                (TextureImporterSwizzle)((int)(value >> 8) & (int)byte.MaxValue),
                (TextureImporterSwizzle)((int)(value >> 16) & (int)byte.MaxValue),
                (TextureImporterSwizzle)((int)(value >> 24) & (int)byte.MaxValue));
        }

        internal TextureImporterSettings GetSerializedPropertySettings(TextureImporterSettings settings)
        {
            if (!m_AlphaSource.hasMultipleDifferentValues)
                settings.alphaSource = (TextureImporterAlphaSource)m_AlphaSource.intValue;

            if (!m_ConvertToNormalMap.hasMultipleDifferentValues)
                settings.convertToNormalMap = m_ConvertToNormalMap.intValue > 0;

            if (!m_BorderMipMap.hasMultipleDifferentValues)
                settings.borderMipmap = m_BorderMipMap.intValue > 0;

#if ENABLE_TEXTURE_STREAMING
            if (!m_StreamingMipmaps.hasMultipleDifferentValues)
                settings.streamingMipmaps = m_StreamingMipmaps.intValue > 0;
            if (!m_StreamingMipmapsPriority.hasMultipleDifferentValues)
                settings.streamingMipmapsPriority = m_StreamingMipmapsPriority.intValue;
#endif

            if (!m_MipMapsPreserveCoverage.hasMultipleDifferentValues)
                settings.mipMapsPreserveCoverage = m_MipMapsPreserveCoverage.intValue > 0;

            if (!m_AlphaTestReferenceValue.hasMultipleDifferentValues)
                settings.alphaTestReferenceValue = m_AlphaTestReferenceValue.floatValue;

            if (!m_NPOTScale.hasMultipleDifferentValues)
                settings.npotScale = (TextureImporterNPOTScale)m_NPOTScale.intValue;

            if (!m_IsReadable.hasMultipleDifferentValues)
                settings.readable = m_IsReadable.intValue > 0;

            if (!m_sRGBTexture.hasMultipleDifferentValues)
                settings.sRGBTexture = m_sRGBTexture.intValue > 0;

            if (!m_EnableMipMap.hasMultipleDifferentValues)
                settings.mipmapEnabled = m_EnableMipMap.intValue > 0;

            if (!m_MipMapMode.hasMultipleDifferentValues)
                settings.mipmapFilter = (TextureImporterMipFilter)m_MipMapMode.intValue;

            if (!m_Swizzle.hasMultipleDifferentValues)
            {
                (TextureImporterSwizzle r, TextureImporterSwizzle g, TextureImporterSwizzle b, TextureImporterSwizzle a) swizzleValue = ConvertSwizzleRaw(m_Swizzle.uintValue);
                settings.swizzleR = swizzleValue.r;
                settings.swizzleG = swizzleValue.g;
                settings.swizzleB = swizzleValue.b;
                settings.swizzleA = swizzleValue.a;
            }

            if (!m_FadeOut.hasMultipleDifferentValues)
                settings.fadeOut = m_FadeOut.intValue > 0;

            if (!m_MipMapFadeDistanceStart.hasMultipleDifferentValues)
                settings.mipmapFadeDistanceStart = m_MipMapFadeDistanceStart.intValue;

            if (!m_MipMapFadeDistanceEnd.hasMultipleDifferentValues)
                settings.mipmapFadeDistanceEnd = m_MipMapFadeDistanceEnd.intValue;

            if (!m_SpriteMode.hasMultipleDifferentValues)
                settings.spriteMode = m_SpriteMode.intValue;

            if (!m_SpritePixelsToUnits.hasMultipleDifferentValues)
                settings.spritePixelsPerUnit = m_SpritePixelsToUnits.floatValue;

            if (!m_SpriteExtrude.hasMultipleDifferentValues)
                settings.spriteExtrude = (uint)m_SpriteExtrude.intValue;

            if (!m_SpriteMeshType.hasMultipleDifferentValues)
                settings.spriteMeshType = (SpriteMeshType)m_SpriteMeshType.intValue;

            if (!m_Alignment.hasMultipleDifferentValues)
                settings.spriteAlignment = m_Alignment.intValue;

            if (!m_SpritePivot.hasMultipleDifferentValues)
                settings.spritePivot = m_SpritePivot.vector2Value;

            if (!m_WrapU.hasMultipleDifferentValues)
                settings.wrapModeU = (TextureWrapMode)m_WrapU.intValue;
            if (!m_WrapV.hasMultipleDifferentValues)
                settings.wrapModeU = (TextureWrapMode)m_WrapV.intValue;
            if (!m_WrapW.hasMultipleDifferentValues)
                settings.wrapModeU = (TextureWrapMode)m_WrapW.intValue;

            if (!m_FilterMode.hasMultipleDifferentValues)
                settings.filterMode = (FilterMode)m_FilterMode.intValue;

            if (!m_Aniso.hasMultipleDifferentValues)
                settings.aniso = m_Aniso.intValue;


            if (!m_AlphaIsTransparency.hasMultipleDifferentValues)
                settings.alphaIsTransparency = m_AlphaIsTransparency.intValue > 0;

            if (!m_TextureType.hasMultipleDifferentValues)
                settings.textureType = (TextureImporterType)m_TextureType.intValue;

            if (!m_TextureShape.hasMultipleDifferentValues)
                settings.textureShape = (TextureImporterShape)m_TextureShape.intValue;

            return settings;
        }
        /// <summary>
        /// Override of AssetImporterEditor.showImportedObject
        /// The property always returns false so that imported objects does not show up in the Inspector.
        /// </summary>
        /// <value>false</value>
        public override bool showImportedObject
        {
            get { return false; }
        }

        bool ITexturePlatformSettingsDataProvider.textureTypeHasMultipleDifferentValues
        {
            get { return m_TextureType.hasMultipleDifferentValues; }
        }

        TextureImporterType ITexturePlatformSettingsDataProvider.textureType
        {
            get { return (TextureImporterType)m_TextureType.intValue; }
        }

        SpriteImportMode ITexturePlatformSettingsDataProvider.spriteImportMode => spriteImportMode;

        SpriteImportMode spriteImportMode
        {
            get { return (SpriteImportMode)m_SpriteMode.intValue; }
        }

        /// <summary>
        /// Override of AssetImporterEditor.HasModified.
        /// </summary>
        /// <returns>Returns True if has modified data. False otherwise.</returns>
        public override bool HasModified()
        {
            if (base.HasModified())
                return true;

            return m_TexturePlatformSettingsHelper.HasModified();
        }

        bool shouldProduceGameObject
        {
            get { return m_CharacterMode.boolValue && m_MosaicLayers.boolValue && spriteImportMode == SpriteImportMode.Multiple; }
        }

        /// <summary>
        /// Override from AssetImporterEditor to show preview settings.
        /// </summary>
        public override void OnPreviewSettings()
        {
            base.OnPreviewSettings();
            using (new EditorGUI.DisabledScope(!shouldProduceGameObject || m_PreviewRenderUtility == null))
            {
                EditorGUI.BeginChangeCheck();
                m_ShowPivot = GUILayout.Toggle(m_ShowPivot, styles.previewPivotButtonContent, "toolbarbutton");
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetBool(kReferencePref, m_ShowPivot);
            }
        }

        /// <summary>
        /// Override from AssetImporterEditor to show custom preview.
        /// </summary>
        /// <param name="r">Preview Rect.</param>
        public override void DrawPreview(Rect r)
        {
            if (shouldProduceGameObject)
            {
                if (m_PreviewRenderUtility == null)
                {
                    InitPreview();
                }

                if (m_PreviewRenderUtility != null)
                {
                    PSDImporter t = (PSDImporter)target;
                    Rect prefabBounds = new Rect(0, 0, t.importData.documentSize.x / t.pixelsPerUnit, t.importData.documentSize.y / t.pixelsPerUnit);
                    Vector2 documentPivot = ImportUtilities.GetPivotPoint(prefabBounds, (SpriteAlignment)m_DocumentAlignment.intValue, m_DocumentPivot.vector2Value);
                    m_PreviewRenderUtility.DrawPreview(r, "PreBackgroundSolid", documentPivot, m_ShowPivot);
                }
                else
                    base.DrawPreview(r);
            }
            else
            {
                base.DrawPreview(r);
            }
        }

        internal class Styles
        {
            public readonly GUIContent padding = EditorGUIUtility.TrTextContent("Mosaic Padding", "Padding between each SpriteRect in pixel unit.");
            public readonly GUIContent spriteSizeExpand = EditorGUIUtility.TrTextContent("Sprite Padding", "Internal padding within each SpriteRect generated from the Photoshop file.");
            public readonly GUIContent previewPivotButtonContent = EditorGUIUtility.TrIconContent("AvatarPivot", "Displays generated Prefab's pivot. The option will only be enabled if a Prefab is already generated and the import options will generate a Prefab.");
            public readonly GUIContent textureTypeTitle = new GUIContent("Texture Type", "What will this texture be used for?");
            public readonly GUIContent[] textureTypeOptions =
            {
                new GUIContent("Default", "Texture is a normal image such as a diffuse texture or other."),
                new GUIContent("Sprite (2D and UI)", "Texture is used for a sprite."),
            };
            public readonly int[] textureTypeValues =
            {
                (int)TextureImporterType.Default,
                (int)TextureImporterType.Sprite,
            };

            private readonly GUIContent textureShape2D = new GUIContent("2D, Texture is 2D.");
            private readonly GUIContent textureShapeCube = new GUIContent("Cube", "Texture is a Cubemap.");
            public readonly Dictionary<TextureImporterShape, GUIContent[]> textureShapeOptionsDictionnary = new Dictionary<TextureImporterShape, GUIContent[]>();
            public readonly Dictionary<TextureImporterShape, int[]> textureShapeValuesDictionnary = new Dictionary<TextureImporterShape, int[]>();


            public readonly GUIContent filterMode = new GUIContent("Filter Mode");
            public readonly GUIContent[] filterModeOptions =
            {
                new GUIContent("Point (no filter)"),
                new GUIContent("Bilinear"),
                new GUIContent("Trilinear")
            };

            public readonly GUIContent mipmapFadeOutToggle = new GUIContent("Fadeout Mip Maps");
            public readonly GUIContent mipmapFadeOut = new GUIContent("Fade Range");
            public readonly GUIContent readWrite = new GUIContent("Read/Write Enabled", "Enable to be able to access the raw pixel data from code.");

            public readonly GUIContent alphaSource = new GUIContent("Alpha Source", "How is the alpha generated for the imported texture.");
            public readonly GUIContent[] alphaSourceOptions =
            {
                new GUIContent("None", "No Alpha will be used."),
                new GUIContent("Input Texture Alpha", "Use Alpha from the input texture if one is provided."),
                new GUIContent("From Gray Scale", "Generate Alpha from image gray scale."),
            };
            public readonly int[] alphaSourceValues =
            {
                (int)TextureImporterAlphaSource.None,
                (int)TextureImporterAlphaSource.FromInput,
                (int)TextureImporterAlphaSource.FromGrayScale,
            };

            public readonly GUIContent generateMipMaps = new GUIContent("Generate Mip Maps");
            public readonly GUIContent sRGBTexture = new GUIContent("sRGB (Color Texture)", "Texture content is stored in gamma space. Non-HDR color textures should enable this flag (except if used for IMGUI).");
            public readonly GUIContent borderMipMaps = new GUIContent("Border Mip Maps");
#if ENABLE_TEXTURE_STREAMING
            public readonly GUIContent streamingMipMaps = EditorGUIUtility.TrTextContent("Mip Streaming", "Only load larger mipmaps as needed to render the current game cameras. Requires texture streaming to be enabled in quality settings.");
            public readonly GUIContent streamingMipmapsPriority = EditorGUIUtility.TrTextContent("Priority", "Mipmap streaming priority when there's contention for resources. Positive numbers represent higher priority. Valid range is -128 to 127.");
#endif
            public readonly GUIContent mipMapsPreserveCoverage = new GUIContent("Preserve Coverage", "The alpha channel of generated Mip Maps will preserve coverage during the alpha test.");
            public readonly GUIContent alphaTestReferenceValue = new GUIContent("Alpha Cutoff Value", "The reference value used during the alpha test. Controls Mip Map coverage.");
            public readonly GUIContent mipMapFilter = new GUIContent("Mip Map Filtering");
            public readonly GUIContent[] mipMapFilterOptions =
            {
                new GUIContent("Box"),
                new GUIContent("Kaiser"),
            };
            public readonly GUIContent npot = new GUIContent("Non Power of 2", "How non-power-of-two textures are scaled on import.");

            public readonly GUIContent spriteMode = new GUIContent("Sprite Mode");
            public readonly GUIContent[] spriteModeOptions =
            {
                new GUIContent("Single"),
                new GUIContent("Multiple"),
                new GUIContent("Polygon"),
            };
            public readonly GUIContent[] spriteMeshTypeOptions =
            {
                new GUIContent("Full Rect"),
                new GUIContent("Tight"),
            };

            public readonly GUIContent spritePixelsPerUnit = new GUIContent("Pixels Per Unit", "How many pixels in the sprite correspond to one unit in the world.");
            public readonly GUIContent spriteExtrude = new GUIContent("Extrude Edges", "How much empty area to leave around the sprite in the generated mesh.");
            public readonly GUIContent spriteMeshType = new GUIContent("Mesh Type", "Type of sprite mesh to generate.");
            public readonly GUIContent spriteAlignment = new GUIContent("Pivot", "Sprite pivot point in its local space. May be used for syncing animation frames of different sizes.");
            public readonly GUIContent characterAlignment = new GUIContent("Pivot", "Character pivot point in its local space using normalized value i.e. 0 - 1");

            public readonly GUIContent[] spriteAlignmentOptions =
            {
                new GUIContent("Center"),
                new GUIContent("Top Left"),
                new GUIContent("Top"),
                new GUIContent("Top Right"),
                new GUIContent("Left"),
                new GUIContent("Right"),
                new GUIContent("Bottom Left"),
                new GUIContent("Bottom"),
                new GUIContent("Bottom Right"),
                new GUIContent("Custom"),
            };

            public readonly GUIContent warpNotSupportWarning = new GUIContent("Graphics device doesn't support Repeat wrap mode on NPOT textures. Falling back to Clamp.");
            public readonly GUIContent anisoLevelLabel = new GUIContent("Aniso Level");
            public readonly GUIContent anisotropicDisableInfo = new GUIContent("Anisotropic filtering is disabled for all textures in Quality Settings.");
            public readonly GUIContent anisotropicForceEnableInfo = new GUIContent("Anisotropic filtering is enabled for all textures in Quality Settings.");
            public readonly GUIContent unappliedSettingsDialogTitle = new GUIContent("Unapplied import settings");
            public readonly GUIContent unappliedSettingsDialogContent = new GUIContent("Unapplied import settings for \'{0}\'.\nApply and continue to sprite editor or cancel.");
            public readonly GUIContent applyButtonLabel = new GUIContent("Apply");
            public readonly GUIContent cancelButtonLabel = new GUIContent("Cancel");
            public readonly GUIContent spriteEditorButtonLabel = new GUIContent("Open Sprite Editor");
            public readonly GUIContent resliceFromLayerWarning = new GUIContent("This will reinitialize and recreate all Sprites based on the file’s layer data. Existing Sprite metadata from previously generated Sprites are copied over.");
            public readonly GUIContent alphaIsTransparency = new GUIContent("Alpha Is Transparency", "If the provided alpha channel is transparency, enable this to pre-filter the color to avoid texture filtering artifacts. This is not supported for HDR textures.");

            public readonly GUIContent advancedHeaderText = new GUIContent("Advanced", "Show advanced settings.");

            public readonly GUIContent platformSettingsHeaderText = new GUIContent("Platform Settings");

            public readonly GUIContent wrapModeLabel = new GUIContent("Wrap Mode");
            public readonly GUIContent wrapU = new GUIContent("U axis");
            public readonly GUIContent wrapV = new GUIContent("V axis");
            public readonly GUIContent wrapW = new GUIContent("W axis");


            public readonly GUIContent[] wrapModeContents =
            {
                new GUIContent("Repeat"),
                new GUIContent("Clamp"),
                new GUIContent("Mirror"),
                new GUIContent("Mirror Once"),
                new GUIContent("Per-axis")
            };
            public readonly int[] wrapModeValues =
            {
                (int)TextureWrapMode.Repeat,
                (int)TextureWrapMode.Clamp,
                (int)TextureWrapMode.Mirror,
                (int)TextureWrapMode.MirrorOnce,
                -1
            };

            public readonly GUIContent layerMapping = EditorGUIUtility.TrTextContent("Layer Mapping", "Options for indicating how layer to Sprite mapping.");
            public readonly GUIContent generatePhysicsShape = EditorGUIUtility.TrTextContent("Generate Physics Shape", "Generates a default physics shape from the outline of the Sprite/s when a physics shape has not been set in the Sprite Editor.");
            public readonly GUIContent generateTileAssets = EditorGUIUtility.TrTextContent("Generate Tile Assets", "Generates Tile assets from Sprite/s generated by importer.");
            public readonly GUIContent importHiddenLayer = EditorGUIUtility.TrTextContent("Include Hidden Layers", "Settings to determine when hidden layers should be imported.");
            public readonly GUIContent mosaicLayers = EditorGUIUtility.TrTextContent("Import Mode", "Layers will be imported as individual Sprites.");
            public readonly GUIContent characterMode = EditorGUIUtility.TrTextContent("Use as Rig", "Enable to support 2D Animation character rigging.");
            public readonly GUIContent layerGroupLabel = EditorGUIUtility.TrTextContent("Use Layer Group", "GameObjects are grouped according to source file layer grouping.");
            public readonly GUIContent resliceFromLayer = EditorGUIUtility.TrTextContent("Automatic Reslice", "Recreate Sprite rects from file.");
            public readonly GUIContent paperDollMode = EditorGUIUtility.TrTextContent("Paper Doll Mode", "Special mode to generate a Prefab for Paper Doll use case.");
            public readonly GUIContent keepDuplicateSpriteName = EditorGUIUtility.TrTextContent("Keep Duplicate Names", "Keep Sprite name same as Layer Name even if there are duplicated Layer Name.");
            public readonly GUIContent mainSkeletonName = EditorGUIUtility.TrTextContent("Main Skeleton", "Main Skeleton to use for Rigging.");
            public readonly GUIContent generalHeaderText = EditorGUIUtility.TrTextContent("General", "General settings.");
            public readonly GUIContent layerImportHeaderText = EditorGUIUtility.TrTextContent("Layer Import", "Layer Import settings.");
            public readonly GUIContent textureHeaderText = EditorGUIUtility.TrTextContent("Texture", "Texture settings.");
            public readonly GUIContent multiEditLayerManagementNotSupported = EditorGUIUtility.TrTextContent("Multi editing in Layer Management is not supported.", "");
            public readonly GUIContent characterRigHeaderText = EditorGUIUtility.TrTextContent("Character Rig", "Character Rig settings.");

            public readonly GUIContent tilePaletteHeaderText = EditorGUIUtility.TrTextContent("Tile Palette", "Tile Palette settings.");
            public static readonly GUIContent tilePaletteHexagonLabel = EditorGUIUtility.TrTextContent("Hexagon Type");
            public static readonly GUIContent[] tilePaletteHexagonSwizzleTypeLabel =
            {
                EditorGUIUtility.TrTextContent("Point Top"),
                EditorGUIUtility.TrTextContent("Flat Top"),
            };
            public static readonly GUIContent tilePaletteCellSizeLabel = EditorGUIUtility.TrTextContent("Tile Palette Cell Size");
            public static readonly GUIContent tilePaletteTransparencySortAxisLabel = EditorGUIUtility.TrTextContent("Transparency Sort Axis");

            public readonly int[] falseTrueOptionValue =
            {
                0,
                1
            };

            public readonly GUIContent[] importModeOptions =
            {
                EditorGUIUtility.TrTextContent("Individual Sprites (Mosaic)","Import individual Photoshop layers as Sprites."),
                new GUIContent("Merged","Merge Photoshop layers and import as single Sprite.")
            };


            public readonly GUIContent[] layerMappingOption =
            {
                EditorGUIUtility.TrTextContent("Use Layer ID","Use layer's ID to identify layer and Sprite mapping."),
                EditorGUIUtility.TrTextContent("Use Layer Name","Use layer's name to identify layer and Sprite mapping."),
                EditorGUIUtility.TrTextContent("Use Layer Name (Case Sensitive)","Use layer's name to identify layer and Sprite mapping."),
            };

            public readonly int[] layerMappingOptionValues =
            {
                (int)PSDImporter.ELayerMappingOption.UseLayerId,
                (int)PSDImporter.ELayerMappingOption.UseLayerName,
                (int)PSDImporter.ELayerMappingOption.UseLayerNameCaseSensitive
            };

            public readonly GUIContent[] layerGroupOption =
            {
                EditorGUIUtility.TrTextContent("Ignore Layer Groups","Only layers will generate GameObjects."),
                EditorGUIUtility.TrTextContent("User Layer Groups", "Group GameObjects according to source file's layer grouping")
            };

            public readonly GUIContent[] editorTabNames =
            {
                EditorGUIUtility.TrTextContent("Settings", "Importer Settings."),
                EditorGUIUtility.TrTextContent("Layer Management", "Layer merge settings.")
            };

            public readonly GUIContent swizzle = EditorGUIUtility.TrTextContent("Swizzle",
                "Reorder and invert texture color channels. For each of R,G,B,A channels pick where the channel data comes from.");
            public readonly string[] swizzleOptions = new[] { "R", "G", "B", "A", "1-R", "1-G", "1-B", "1-A", "0", "1" };

            public Styles()
            {
                // This is far from ideal, but it's better than having tons of logic in the GUI code itself.
                // The combination should not grow too much anyway since only Texture3D will be added later.
                GUIContent[] s2D_Options = { textureShape2D };
                GUIContent[] sCube_Options = { textureShapeCube };
                GUIContent[] s2D_Cube_Options = { textureShape2D, textureShapeCube };
                textureShapeOptionsDictionnary.Add(TextureImporterShape.Texture2D, s2D_Options);
                textureShapeOptionsDictionnary.Add(TextureImporterShape.TextureCube, sCube_Options);
                textureShapeOptionsDictionnary.Add(TextureImporterShape.Texture2D | TextureImporterShape.TextureCube, s2D_Cube_Options);

                int[] s2D_Values = { (int)TextureImporterShape.Texture2D };
                int[] sCube_Values = { (int)TextureImporterShape.TextureCube };
                int[] s2D_Cube_Values = { (int)TextureImporterShape.Texture2D, (int)TextureImporterShape.TextureCube };
                textureShapeValuesDictionnary.Add(TextureImporterShape.Texture2D, s2D_Values);
                textureShapeValuesDictionnary.Add(TextureImporterShape.TextureCube, sCube_Values);
                textureShapeValuesDictionnary.Add(TextureImporterShape.Texture2D | TextureImporterShape.TextureCube, s2D_Cube_Values);
            }
        }

        static Styles m_Styles;

        internal static Styles styles
        {
            get
            {
                if (m_Styles == null)
                    m_Styles = new Styles();
                return m_Styles;
            }
        }
    }

    class PSDImporterEditorFoldOutState
    {
        SavedBool m_GeneralFoldout;
        SavedBool m_LayerImportFoldout;
        SavedBool m_CharacterRigFoldout;
        SavedBool m_TilePaletteFoldout;
        SavedBool m_AdvancedFoldout;
        SavedBool m_TextureFoldout;
        SavedBool m_PlatformSettingsFoldout;

        public PSDImporterEditorFoldOutState()
        {
            m_GeneralFoldout = new SavedBool("PSDImporterEditor.m_GeneralFoldout", true);
            m_LayerImportFoldout = new SavedBool("PSDImporterEditor.m_LayerImportFoldout", true);
            m_CharacterRigFoldout = new SavedBool("PSDImporterEditor.m_CharacterRigFoldout", false);
            m_TilePaletteFoldout = new SavedBool("PSDImporterEditor.m_TilePaletteFoldout", false);
            m_AdvancedFoldout = new SavedBool("PSDImporterEditor.m_AdvancedFoldout", false);
            m_TextureFoldout = new SavedBool("PSDImporterEditor.m_TextureFoldout", false);
            m_PlatformSettingsFoldout = new SavedBool("PSDImporterEditor.m_PlatformSettingsFoldout", false);
        }

        bool DoFoldout(GUIContent title, bool state)
        {
            InspectorUtils.DrawSplitter();
            return InspectorUtils.DrawHeaderFoldout(title, state);
        }

        public bool DoGeneralUI(GUIContent title)
        {
            m_GeneralFoldout.value = DoFoldout(title, m_GeneralFoldout.value);
            return m_GeneralFoldout.value;
        }

        public bool DoLayerImportUI(GUIContent title)
        {
            m_LayerImportFoldout.value = DoFoldout(title, m_LayerImportFoldout.value);
            return m_LayerImportFoldout.value;
        }

        public bool DoCharacterRigUI(GUIContent title)
        {
            m_CharacterRigFoldout.value = DoFoldout(title, m_CharacterRigFoldout.value);
            return m_CharacterRigFoldout.value;
        }

        public bool DoTilePaletteUI(GUIContent title)
        {
            m_TilePaletteFoldout.value = DoFoldout(title, m_TilePaletteFoldout.value);
            return m_TilePaletteFoldout.value;
        }

        public bool DoAdvancedUI(GUIContent title)
        {
            m_AdvancedFoldout.value = DoFoldout(title, m_AdvancedFoldout.value);
            return m_AdvancedFoldout.value;
        }

        public bool DoPlatformSettingsUI(GUIContent title)
        {
            m_PlatformSettingsFoldout.value = DoFoldout(title, m_PlatformSettingsFoldout.value);
            return m_PlatformSettingsFoldout.value;
        }

        public bool DoTextureUI(GUIContent title)
        {
            m_TextureFoldout.value = DoFoldout(title, m_TextureFoldout.value);
            return m_TextureFoldout.value;
        }

        class SavedBool
        {
            bool m_Value;
            string m_Name;
            bool m_Loaded;

            public SavedBool(string name, bool value)
            {
                m_Name = name;
                m_Loaded = false;
                m_Value = value;
            }

            private void Load()
            {
                if (m_Loaded)
                    return;

                m_Loaded = true;
                m_Value = EditorPrefs.GetBool(m_Name, m_Value);
            }

            public bool value
            {
                get { Load(); return m_Value; }
                set
                {
                    Load();
                    if (m_Value == value)
                        return;
                    m_Value = value;
                    EditorPrefs.SetBool(m_Name, value);
                }
            }

            public static implicit operator bool(SavedBool s)
            {
                return s.value;
            }
        }
    }
}
