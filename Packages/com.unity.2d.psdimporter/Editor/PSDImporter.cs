#if UNITY_6000_1_OR_NEWER
#define ENABLE_2D_TILEMAP_EDITOR
#endif

using System;
using System.Collections.Generic;
using System.IO;
using PDNWrapper;
using UnityEngine;
using Unity.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.Common;
using UnityEditor.U2D.Sprites;
using UnityEngine.U2D;
using UnityEngine.Scripting.APIUpdating;

#if ENABLE_2D_TILEMAP_EDITOR
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;
#endif

#if ENABLE_2D_ANIMATION
using UnityEditor.U2D.Animation;
using UnityEngine.U2D.Animation;
#endif

namespace UnityEditor.U2D.PSD
{
    /// <summary>
    /// ScriptedImporter to import Photoshop files
    /// </summary>
    // Version using unity release + 5 digit padding for future upgrade. Eg 2021.2 -> 21200000, 6000.5 -> 60500000
    [ScriptedImporter(60500000, new string[] { "psb" }, new[] { "psd" }, AllowCaching = true)]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.psdimporter@15.0")]
    [MovedFrom("UnityEditor.Experimental.AssetImporters")]
    public partial class PSDImporter : ScriptedImporter, ISpriteEditorDataProvider
    {
        internal enum ELayerMappingOption
        {
            UseLayerName,
            UseLayerNameCaseSensitive,
            UseLayerId
        }

        IPSDLayerMappingStrategy[] m_MappingCompare =
        {
            new LayerMappingUseLayerName(),
            new LayerMappingUseLayerNameCaseSensitive(),
            new LayerMappingUserLayerID(),
        };

        [SerializeField]
        TextureImporterSettings m_TextureImporterSettings = new TextureImporterSettings()
        {
            mipmapEnabled = true,
            mipmapFilter = TextureImporterMipFilter.BoxFilter,
            sRGBTexture = true,
            borderMipmap = false,
            mipMapsPreserveCoverage = false,
            alphaTestReferenceValue = 0.5f,
            readable = false,

#if ENABLE_TEXTURE_STREAMING
            streamingMipmaps = false,
            streamingMipmapsPriority = 0,
#endif

            fadeOut = false,
            mipmapFadeDistanceStart = 1,
            mipmapFadeDistanceEnd = 3,

            convertToNormalMap = false,
            heightmapScale = 0.25F,
            normalMapFilter = 0,

            generateCubemap = TextureImporterGenerateCubemap.AutoCubemap,
            cubemapConvolution = 0,

            seamlessCubemap = false,

            npotScale = TextureImporterNPOTScale.ToNearest,

            spriteMode = (int)SpriteImportMode.Multiple,
            spriteExtrude = 1,
            spriteMeshType = SpriteMeshType.Tight,
            spriteAlignment = (int)SpriteAlignment.Center,
            spritePivot = new Vector2(0.5f, 0.5f),
            spritePixelsPerUnit = 100.0f,
            spriteBorder = new Vector4(0.0f, 0.0f, 0.0f, 0.0f),

            alphaSource = TextureImporterAlphaSource.FromInput,
            alphaIsTransparency = true,
            spriteTessellationDetail = -1.0f,

            textureType = TextureImporterType.Sprite,
            textureShape = TextureImporterShape.Texture2D,

            filterMode = FilterMode.Bilinear,
            aniso = 1,
            mipmapBias = 0.0f,
            wrapModeU = TextureWrapMode.Repeat,
            wrapModeV = TextureWrapMode.Repeat,
            wrapModeW = TextureWrapMode.Repeat,
            swizzleR = TextureImporterSwizzle.R,
            swizzleG = TextureImporterSwizzle.G,
            swizzleB = TextureImporterSwizzle.B,
            swizzleA = TextureImporterSwizzle.A,
        };

        [SerializeField] List<SpriteMetaData> m_SingleSpriteImportData = new List<SpriteMetaData>(1) { new SpriteMetaData() };
        [SerializeField] List<SpriteMetaData> m_MultiSpriteImportData = new List<SpriteMetaData>();
        [SerializeField] List<SpriteMetaData> m_LayeredSpriteImportData = new List<SpriteMetaData>();

        // --- Obsolete sprite import data containers

        // SpriteData for both single and multiple mode
        [Obsolete("This data has now been merged into m_MultiSpriteImportData and m_SingleSpriteImportData")]
        [SerializeField] List<SpriteMetaData> m_SpriteImportData = new List<SpriteMetaData>(); // we use index 0 for single sprite and the rest for multiple sprites
        // SpriteData for Rig mode
        [Obsolete("This data has now been merged into m_LayeredSpriteImportData")]
        [SerializeField] List<SpriteMetaData> m_RigSpriteImportData = new List<SpriteMetaData>();
        // SpriteData for shared rig mode
        [Obsolete("This data has now been merged into m_LayeredSpriteImportData")]
        [SerializeField] List<SpriteMetaData> m_SharedRigSpriteImportData = new List<SpriteMetaData>();
        [Obsolete("This data has now been merged into m_LayeredSpriteImportData")]
        [SerializeField] List<SpriteMetaData> m_MosaicSpriteImportData = new List<SpriteMetaData>();

        // --- End obsolete sprite import data containers

#if ENABLE_2D_ANIMATION
        // CharacterData for shared rig mode
        [SerializeField] CharacterData m_SharedRigCharacterData = new CharacterData();
        // CharacterData for Rig mode
        [SerializeField] CharacterData m_CharacterData = new CharacterData();
#endif

        [SerializeField]
        List<TextureImporterPlatformSettings> m_PlatformSettings = new List<TextureImporterPlatformSettings>();
        [SerializeField]
        bool m_MosaicLayers = true;
        [SerializeField]
        bool m_CharacterMode = true;
        [SerializeField]
        Vector2 m_DocumentPivot = Vector2.zero;
        [SerializeField]
        SpriteAlignment m_DocumentAlignment = SpriteAlignment.BottomCenter;
        [SerializeField]
        bool m_ImportHiddenLayers = false;
        [SerializeField]
        ELayerMappingOption m_LayerMappingOption = ELayerMappingOption.UseLayerId;
        [SerializeField]
        bool m_GeneratePhysicsShape = false;

        [SerializeField]
        bool m_PaperDollMode = false;

        [SerializeField]
        bool m_KeepDupilcateSpriteName = true;

#if ENABLE_2D_TILEMAP_EDITOR
        [SerializeField]
        bool m_GenerateTileAssets = false;

        [SerializeField]
        GridLayout.CellLayout m_TilePaletteCellLayout = GridLayout.CellLayout.Rectangle;

        private static readonly GridLayout.CellSwizzle[] s_TilePaletteexagonSwizzleTypeValue =
        {
            GridLayout.CellSwizzle.XYZ,
            GridLayout.CellSwizzle.YXZ,
        };

        [SerializeField]
        int m_TilePaletteHexagonLayout;

        [SerializeField]
        Vector3 m_TilePaletteCellSize = new Vector3(1, 1, 0);

        [SerializeField]
        GridPalette.CellSizing m_TilePaletteCellSizing = GridPalette.CellSizing.Automatic;

        [SerializeField]
        TransparencySortMode m_TransparencySortMode = TransparencySortMode.Default;

        [SerializeField]
        Vector3 m_TransparencySortAxis = new Vector3(0f, 0f, 1f);

        [SerializeField]
        TileTemplate m_TileTemplate = null;
#endif

        [SerializeField]
        int m_Padding = 4;

        [SerializeField]
        ushort m_SpriteSizeExpand = 0;

        [SerializeField]
        string m_SkeletonAssetReferenceID = null;

        [SerializeField]
        ScriptableObject m_Pipeline;
        [SerializeField]
        string m_PipelineVersion;

#if ENABLE_2D_ANIMATION
        [SerializeField]
        SpriteCategoryList m_SpriteCategoryList = new SpriteCategoryList() { categories = new List<SpriteCategory>() };
#endif

        GameObjectCreationFactory m_GameObjectFactory = new GameObjectCreationFactory(null);

        PSDImportData m_ImportData;

        internal PSDImportData importData
        {
            get
            {
                PSDImportData returnValue = m_ImportData;
                if (returnValue == null && !PSDImporterAssetPostProcessor.ContainsImporter(this))
                    // Using LoadAllAssetsAtPath because PSDImportData is hidden
                    returnValue = AssetDatabase.LoadAllAssetsAtPath(assetPath).FirstOrDefault(x => x is PSDImportData) as PSDImportData;

                if (returnValue == null)
                    returnValue = ScriptableObject.CreateInstance<PSDImportData>();

                m_ImportData = returnValue;
                return returnValue;
            }
        }

        internal int textureActualWidth
        {
            get => importData.textureActualWidth;
            private set => importData.textureActualWidth = value;
        }

        internal int textureActualHeight
        {
            get => importData.textureActualHeight;
            private set => importData.textureActualHeight = value;
        }

        [SerializeField]
        string m_SpritePackingTag = "";

        [SerializeField]
        bool m_ResliceFromLayer = false;

        [SerializeField]
        PSDLayerImportSetting[] m_PSDLayerImportSetting;

        [SerializeField]
        List<PSDLayer> m_PsdLayers = new List<PSDLayer>();

        // --- Obsolete psd layer containers

        [Obsolete("This data has now been merged into m_PsdLayers")]
        [SerializeField] List<PSDLayer> m_MosaicPSDLayers = new List<PSDLayer>();
        [Obsolete("This data has now been merged into m_PsdLayers")]
        [SerializeField] List<PSDLayer> m_RigPSDLayers = new List<PSDLayer>();
        [Obsolete("This data has now been merged into m_PsdLayers")]
        [SerializeField] List<PSDLayer> m_SharedRigPSDLayers = new List<PSDLayer>();

        // --- End obsolete psd layer containers

        // Use for inspector to check if the file node is checked
        [SerializeField]
#pragma warning disable 169, 414
        bool m_ImportFileNodeState = true;

        // Used by platform settings to mark it dirty so that it will trigger a reimport
        [SerializeField]
#pragma warning disable 169, 414
        long m_PlatformSettingsDirtyTick;

        [SerializeField]
        bool m_SpriteSizeExpandChanged = false;

        [SerializeField]
        bool m_GenerateGOHierarchy = false;

        [SerializeField]
        string m_TextureAssetName = null;

        [SerializeField]
        string m_PrefabAssetName = null;

        [SerializeField]
        string m_SpriteLibAssetName = null;

        [SerializeField]
        string m_SkeletonAssetName = null;

        [SerializeField]
        SecondarySpriteTexture[] m_SecondarySpriteTextures;

        PSDExtractLayerData[] m_ExtractData;

        internal bool isNPOT => Mathf.IsPowerOfTwo(importData.textureActualWidth) && Mathf.IsPowerOfTwo(importData.textureActualHeight);

        bool shouldProduceGameObject => m_CharacterMode && m_MosaicLayers && spriteImportModeToUse == SpriteImportMode.Multiple;
        bool shouldResliceFromLayer => m_ResliceFromLayer && m_MosaicLayers && spriteImportModeToUse == SpriteImportMode.Multiple;
        bool inCharacterMode => inMosaicMode && m_CharacterMode;

        float definitionScale
        {
            get
            {
                float definitionScaleW = importData.importedTextureWidth / (float)textureActualWidth;
                float definitionScaleH = importData.importedTextureHeight / (float)textureActualHeight;
                return Mathf.Min(definitionScaleW, definitionScaleH);
            }
        }

        internal SecondarySpriteTexture[] secondaryTextures
        {
            get => m_SecondarySpriteTextures;
            set => m_SecondarySpriteTextures = value;
        }

        internal SpriteBone[] mainSkeletonBones
        {
            get
            {
#if ENABLE_2D_ANIMATION
                SkeletonAsset skeleton = skeletonAsset;
                return skeleton != null ? skeleton.GetSpriteBones() : null;
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// PSDImporter constructor.
        /// </summary>
        public PSDImporter()
        {
            m_TextureImporterSettings.swizzleA = TextureImporterSwizzle.A;
            m_TextureImporterSettings.swizzleR = TextureImporterSwizzle.R;
            m_TextureImporterSettings.swizzleG = TextureImporterSwizzle.G;
            m_TextureImporterSettings.swizzleB = TextureImporterSwizzle.B;
        }

        void PackImage(NativeArray<Color32>[] buffers, int[] width, int[] height, int padding, uint spriteSizeExpand, out NativeArray<Color32> outPackedBuffer, out int outPackedBufferWidth, out int outPackedBufferHeight, out RectInt[] outPackedRect, out Vector2Int[] outUVTransform, bool requireSquarePOT = false)
        {
            try
            {
                ScriptableObject pipeline = m_Pipeline;
                if (pipeline == null)
                    pipeline = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Packages/com.unity.2d.psdimporter/Editor/Pipeline.asset");

                object[] args = new object[] { buffers, width, height, padding, spriteSizeExpand, null, 0, 0, null, null, requireSquarePOT };
                pipeline.GetType().InvokeMember("PackImage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static, null,
                    pipeline, args);
                outPackedBuffer = (NativeArray<Color32>)args[5];
                outPackedBufferWidth = (int)args[6];
                outPackedBufferHeight = (int)args[7];
                outPackedRect = (RectInt[])args[8];
                outUVTransform = (Vector2Int[])args[9];
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to pack image. ex:" + e.ToString(), this);
                ImagePacker.Pack(buffers, width, height, padding, spriteSizeExpand, out outPackedBuffer, out outPackedBufferWidth, out outPackedBufferHeight, out outPackedRect, out outUVTransform, requireSquarePOT);
            }
        }

        /// <summary>
        /// Implementation of ScriptedImporter.OnImportAsset
        /// </summary>
        /// <param name="ctx">
        /// This argument contains all the contextual information needed to process the import
        /// event and is also used by the custom importer to store the resulting Unity Asset.
        /// </param>
        public override void OnImportAsset(AssetImportContext ctx)
        {

            FileStream fileStream = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read);
            Document doc = null;

            if (m_ImportData == null)
                m_ImportData = ScriptableObject.CreateInstance<PSDImportData>();
            m_ImportData.hideFlags = HideFlags.HideInHierarchy;

            try
            {
                UnityEngine.Profiling.Profiler.BeginSample("OnImportAsset");

                UnityEngine.Profiling.Profiler.BeginSample("PsdLoad");
                doc = PaintDotNet.Data.PhotoshopFileType.PsdLoad.Load(fileStream);
                UnityEngine.Profiling.Profiler.EndSample();

                m_ImportData.CreatePSDLayerData(doc.Layers);

                ValidatePSDLayerId(doc, m_LayerMappingOption);
                SetDocumentImportData(doc);

                importData.documentSize = new Vector2Int(doc.width, doc.height);

                bool singleSpriteMode = m_TextureImporterSettings.textureType == TextureImporterType.Sprite && m_TextureImporterSettings.spriteMode != (int)SpriteImportMode.Multiple;
                TextureGenerationOutput output;
                if (m_TextureImporterSettings.textureType != TextureImporterType.Sprite ||
                    m_MosaicLayers == false || singleSpriteMode)
                {
                    output = ImportFlattenImage(doc, ctx);
                }
                else
                {
                    output = ImportFromLayers(ctx);
                }

                if (output.texture != null && output.sprites != null)
                    SetPhysicsOutline(GetDataProvider<ISpritePhysicsOutlineDataProvider>(), output.sprites, definitionScale, pixelsPerUnit, m_GeneratePhysicsShape);

                RegisterAssets(ctx, output);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to import file {assetPath}. Error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                fileStream.Close();
                if (doc != null)
                    doc.Dispose();
                UnityEngine.Profiling.Profiler.EndSample();
                EditorUtility.SetDirty(this);
            }

        }

        void ValidatePSDLayerId(Document doc, ELayerMappingOption layerMappingOption)
        {
            if (layerMappingOption == ELayerMappingOption.UseLayerId)
            {
                UniqueNameGenerator uniqueNameGenerator = new UniqueNameGenerator();
                ImportUtilities.ValidatePSDLayerId(GetPSDLayers(), doc.Layers, uniqueNameGenerator, this);
            }
        }

        TextureGenerationOutput ImportTexture(AssetImportContext ctx, NativeArray<Color32> imageData, int textureWidth, int textureHeight, SpriteMetaData[] sprites)
        {
            if (!imageData.IsCreated || imageData.Length == 0)
                return new TextureGenerationOutput();

            TextureGenerationOutput output = new TextureGenerationOutput();
            UnityEngine.Profiling.Profiler.BeginSample("ImportTexture");
            try
            {
                TextureImporterPlatformSettings platformSettings = TextureImporterUtilities.GetPlatformTextureSettings(ctx.selectedBuildTarget, in m_PlatformSettings);

                TextureSettings textureSettings = m_TextureImporterSettings.ExtractTextureSettings();
                textureSettings.assetPath = ctx.assetPath;
                textureSettings.enablePostProcessor = true;
                textureSettings.containsAlpha = true;
                textureSettings.hdr = false;

                TextureAlphaSettings textureAlphaSettings = m_TextureImporterSettings.ExtractTextureAlphaSettings();
                TextureMipmapSettings textureMipmapSettings = m_TextureImporterSettings.ExtractTextureMipmapSettings();
                TextureCubemapSettings textureCubemapSettings = m_TextureImporterSettings.ExtractTextureCubemapSettings();
                TextureWrapSettings textureWrapSettings = m_TextureImporterSettings.ExtractTextureWrapSettings();

                switch (m_TextureImporterSettings.textureType)
                {
                    case TextureImporterType.Default:
                        output = TextureGeneratorHelper.GenerateTextureDefault(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureAlphaSettings, textureMipmapSettings, textureCubemapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.NormalMap:
                        TextureNormalSettings textureNormalSettings = m_TextureImporterSettings.ExtractTextureNormalSettings();
                        output = TextureGeneratorHelper.GenerateNormalMap(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureNormalSettings, textureMipmapSettings, textureCubemapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.GUI:
                        output = TextureGeneratorHelper.GenerateTextureGUI(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureAlphaSettings, textureMipmapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.Sprite:
                        TextureSpriteSettings textureSpriteSettings = m_TextureImporterSettings.ExtractTextureSpriteSettings();
                        textureSpriteSettings.packingTag = m_SpritePackingTag;
                        textureSpriteSettings.qualifyForPacking = !string.IsNullOrEmpty(m_SpritePackingTag);
                        textureSpriteSettings.spriteSheetData = new SpriteImportData[sprites.Length];
                        textureSettings.npotScale = TextureImporterNPOTScale.None;
                        textureSettings.secondaryTextures = secondaryTextures;

                        for (int i = 0; i < sprites.Length; ++i)
                            textureSpriteSettings.spriteSheetData[i] = sprites[i];

                        output = TextureGeneratorHelper.GenerateTextureSprite(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureSpriteSettings, textureAlphaSettings, textureMipmapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.Cursor:
                        output = TextureGeneratorHelper.GenerateTextureCursor(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureAlphaSettings, textureMipmapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.Cookie:
                        output = TextureGeneratorHelper.GenerateCookie(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureAlphaSettings, textureMipmapSettings, textureCubemapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.Lightmap:
                        output = TextureGeneratorHelper.GenerateLightmap(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureMipmapSettings, textureWrapSettings);
                        break;
                    case TextureImporterType.SingleChannel:
                        output = TextureGeneratorHelper.GenerateTextureSingleChannel(imageData, textureWidth, textureHeight, textureSettings, platformSettings, textureAlphaSettings, textureMipmapSettings, textureCubemapSettings, textureWrapSettings);
                        break;
                    default:
                        Debug.LogAssertion("Unknown texture type for import");
                        output = default(TextureGenerationOutput);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to generate Texture2D. Possibly texture size is too big to be generated. ex:" + e.ToString(), this);
            }
            finally
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }

            return output;
        }

        void SetDocumentImportData(IEnumerable<BitmapLayer> layers, PSDExtractLayerData[] extractData, IPSDLayerMappingStrategy mappingStrategy, List<PSDLayer> psdLayers, PSDExtractLayerData parent = null)
        {
            for (int i = 0; i < layers.Count(); ++i)
            {
                BitmapLayer layer = layers.ElementAt(i);
                PSDLayerImportSetting importSetting = null;
                if (m_PSDLayerImportSetting != null && m_PSDLayerImportSetting.Length > 0)
                {
                    importSetting = m_PSDLayerImportSetting.FirstOrDefault(x => mappingStrategy.Compare(x, layer));
                }
                PSDLayer c = psdLayers?.FirstOrDefault(x => mappingStrategy.Compare(x, layer));
                if (c != null)
                {
                    if (c.spriteID.Empty())
                        c.spriteID = importSetting != null ? importSetting.spriteId : GUID.Generate();
                    if (importSetting == null)
                    {
                        importSetting = new PSDLayerImportSetting()
                        {
                            flatten = c.flatten,
                            importLayer = layer.ShouldImport(m_ImportHiddenLayers),
                        };
                    }

                    importSetting.spriteId = c.spriteID;
                }

                if (importSetting == null)
                {
                    importSetting = new PSDLayerImportSetting()
                    {
                        flatten = false,
                        importLayer = layer.ShouldImport(m_ImportHiddenLayers),
                    };
                }

                extractData[i] = new PSDExtractLayerData()
                {
                    bitmapLayer = layer,
                    importSetting = importSetting,
                };

                PSDExtractLayerData[] childrenExtractData = null;
                if (layer.ChildLayer != null)
                {
                    childrenExtractData = new PSDExtractLayerData[layer.ChildLayer.Count()];
                    SetDocumentImportData(layer.ChildLayer, childrenExtractData, mappingStrategy, psdLayers, extractData[i]);
                }

                extractData[i].children = childrenExtractData;
            }
        }

        void SetDocumentImportData(Document doc)
        {
            List<PSDLayer> oldPsdLayers = GetPSDLayers();
            IPSDLayerMappingStrategy mappingStrategy = GetLayerMappingStrategy();
            m_ExtractData = new PSDExtractLayerData[doc.Layers.Count];
            SetDocumentImportData(doc.Layers, m_ExtractData, mappingStrategy, oldPsdLayers);
        }

        TextureGenerationOutput ImportFlattenImage(Document doc, AssetImportContext ctx)
        {
            TextureGenerationOutput output;
            NativeArray<Color32> outputImageBuffer = new NativeArray<Color32>(doc.width * doc.height, Allocator.Persistent);
            try
            {
                FlattenImageTask.Execute(m_ExtractData, ref outputImageBuffer, m_ImportHiddenLayers, canvasSize);

                m_ImportData.singleSpriteTextureImporterSettings = m_TextureImporterSettings;
                importData.importedTextureWidth = textureActualWidth = doc.width;
                importData.importedTextureHeight = textureActualHeight = doc.height;

                List<SpriteMetaData> spriteImportData = GetSpriteImportData();
                if (m_ResliceFromLayer)
                {
                    UniqueNameGenerator spriteNameHash = new UniqueNameGenerator();
                    List<PSDLayer> oldPsdLayers = GetPSDLayers();
                    List<PSDLayer> psdLayers = null;
                    try
                    {
                        ExtractLayerTask.Execute(in m_ExtractData, out psdLayers, m_ImportHiddenLayers, canvasSize);

                        IPSDLayerMappingStrategy mappingStrategy = GetLayerMappingStrategy();
                        string layerUnique = mappingStrategy.LayersUnique(psdLayers.ConvertAll(x => (IPSDLayerMappingStrategyComparable)x));
                        if (!string.IsNullOrEmpty(layerUnique))
                        {
                            Debug.LogWarning(layerUnique, this);
                        }
                        List<int> layerIndex = new List<int>();
                        List<RectInt> spriteData = new List<RectInt>();
                        for (int i = 0; i < psdLayers.Count; ++i)
                        {
                            PSDLayer l = psdLayers[i];
                            int expectedBufferLength = l.width * l.height;
                            if (l.texture.IsCreated && l.texture.Length == expectedBufferLength && l.isImported)
                            {
                                layerIndex.Add(i);
                                RectInt rect = new RectInt((int)l.layerPosition.x, (int)l.layerPosition.y, l.width, l.height);
                                spriteData.Add(rect);
                            }
                        }

                        List<SpriteMetaData> newSpriteMeta = new List<SpriteMetaData>();

                        for (int i = 0; i < spriteData.Count && i < layerIndex.Count; ++i)
                        {
                            PSDLayer psdLayer = psdLayers[layerIndex[i]];
                            SpriteMetaData spriteSheet = spriteImportData.FirstOrDefault(x => x.spriteID == psdLayer.spriteID);
                            if (spriteSheet == null)
                            {
                                spriteSheet = new SpriteMetaData();
                                spriteSheet.border = Vector4.zero;
                                spriteSheet.alignment = (SpriteAlignment)m_TextureImporterSettings.spriteAlignment;
                                spriteSheet.pivot = m_TextureImporterSettings.spritePivot;
                                spriteSheet.rect = new Rect(spriteData[i].x, spriteData[i].y, spriteData[i].width, spriteData[i].height);
                                spriteSheet.spriteID = psdLayer.spriteID;
                            }
                            else
                            {
                                Rect r = spriteSheet.rect;
                                r.position = r.position - psdLayer.mosaicPosition + spriteData[i].position;
                                spriteSheet.rect = r;
                            }

                            psdLayer.spriteName = ImportUtilities.GetUniqueSpriteName(psdLayer.name, spriteNameHash, m_KeepDupilcateSpriteName);
                            spriteSheet.name = psdLayer.spriteName;
                            spriteSheet.spritePosition = psdLayer.layerPosition;

                            spriteSheet.rect = new Rect(spriteData[i].x, spriteData[i].y, spriteData[i].width, spriteData[i].height);

                            psdLayer.spriteID = spriteSheet.spriteID;
                            psdLayer.mosaicPosition = spriteData[i].position;
                            newSpriteMeta.Add(spriteSheet);
                        }
                        spriteImportData.Clear();
                        spriteImportData.AddRange(newSpriteMeta);
                    }
                    finally
                    {
                        if (psdLayers != null)
                            foreach (PSDLayer l in psdLayers)
                                l.Dispose();
                    }
                }

                output = ImportTexture(ctx, outputImageBuffer, doc.width, doc.height, spriteImportData.ToArray());
                importData.importedTextureWidth = output.texture.width;
                importData.importedTextureHeight = output.texture.height;
            }
            finally
            {
                outputImageBuffer.Dispose();
            }

            return output;
        }

        TextureGenerationOutput ImportFromLayers(AssetImportContext ctx)
        {
            TextureGenerationOutput output;
            NativeArray<Color32> outputImageBuffer = default(NativeArray<Color32>);

            List<int> layerIndex = new List<int>();
            UniqueNameGenerator spriteNameHash = new UniqueNameGenerator();

#pragma warning disable CS0618
            TextureImporterPlatformSettings platformSettings = TextureImporterUtilities.GetPlatformTextureSettings(ctx.selectedBuildTarget, in m_PlatformSettings);
            bool requireSquarePOT = (TextureImporterFormat.PVRTC_RGB2 <= platformSettings.format && platformSettings.format <= TextureImporterFormat.PVRTC_RGBA4);
#pragma warning restore CS0618

            List<PSDLayer> oldPsdLayers = GetPSDLayers();
            List<PSDLayer> psdLayers = null;
            try
            {
                ExtractLayerTask.Execute(in m_ExtractData, out psdLayers, m_ImportHiddenLayers, canvasSize);

                IPSDLayerMappingStrategy mappingStrategy = GetLayerMappingStrategy();
                string layerUnique = mappingStrategy.LayersUnique(psdLayers.ConvertAll(x => (IPSDLayerMappingStrategyComparable)x));
                if (!string.IsNullOrEmpty(layerUnique))
                {
                    Debug.LogWarning(layerUnique, this);
                }
                GUID[] removedLayersSprite = oldPsdLayers.Where(x => psdLayers.FirstOrDefault(y => mappingStrategy.Compare(y, x)) == null).Select(z => z.spriteID).ToArray();
                bool hasNewLayer = false;
                for (int i = 0; i < psdLayers.Count; ++i)
                {
                    int j = 0;
                    PSDLayer psdLayer = psdLayers[i];
                    for (; j < oldPsdLayers.Count; ++j)
                    {
                        if (mappingStrategy.Compare(psdLayer, oldPsdLayers[j]))
                        {
                            psdLayer.spriteName = oldPsdLayers[j].spriteName;
                            psdLayer.mosaicPosition = oldPsdLayers[j].mosaicPosition;
                            if (psdLayer.isImported != oldPsdLayers[j].isImported)
                                hasNewLayer = true;
                            break;
                        }
                    }

                    if (j >= oldPsdLayers.Count)
                        hasNewLayer = true;
                }

                List<NativeArray<Color32>> layerBuffers = new List<NativeArray<Color32>>();
                List<int> layerWidth = new List<int>();
                List<int> layerHeight = new List<int>();
                for (int i = 0; i < psdLayers.Count; ++i)
                {
                    PSDLayer l = psdLayers[i];
                    int expectedBufferLength = l.width * l.height;
                    if (l.texture.IsCreated && l.texture.Length == expectedBufferLength && l.isImported)
                    {
                        layerBuffers.Add(l.texture);
                        layerIndex.Add(i);
                        layerWidth.Add(l.width);
                        layerHeight.Add(l.height);
                    }
                }

                PackImage(layerBuffers.ToArray(), layerWidth.ToArray(), layerHeight.ToArray(), m_Padding, m_SpriteSizeExpand, out outputImageBuffer, out int width, out int height, out RectInt[] spriteData, out Vector2Int[] uvTransform, requireSquarePOT);

                Vector2[] packOffsets = new Vector2[spriteData.Length];
                for (int i = 0; i < packOffsets.Length; ++i)
                    packOffsets[i] = new Vector2((uvTransform[i].x - spriteData[i].position.x) / -1f, (uvTransform[i].y - spriteData[i].position.y) / -1f);

                List<SpriteMetaData> spriteImportData = GetSpriteImportData();
                if (spriteImportData.Count <= 0 || shouldResliceFromLayer || hasNewLayer)
                {
                    List<SpriteMetaData> newSpriteMeta = new List<SpriteMetaData>();

                    for (int i = 0; i < spriteData.Length && i < layerIndex.Count; ++i)
                    {
                        PSDLayer psdLayer = psdLayers[layerIndex[i]];
                        SpriteMetaData spriteSheet = spriteImportData.FirstOrDefault(x => x.spriteID == psdLayer.spriteID);
                        if (spriteSheet == null)
                        {
                            spriteSheet = new SpriteMetaData();
                            spriteSheet.border = Vector4.zero;
                            spriteSheet.alignment = (SpriteAlignment)m_TextureImporterSettings.spriteAlignment;
                            spriteSheet.pivot = m_TextureImporterSettings.spritePivot;
                            spriteSheet.rect = new Rect(spriteData[i].x, spriteData[i].y, spriteData[i].width, spriteData[i].height);
                            spriteSheet.spriteID = psdLayer.spriteID;
                        }
                        else
                        {
                            Rect r = spriteSheet.rect;
                            r.position = r.position - psdLayer.mosaicPosition + spriteData[i].position;
                            spriteSheet.rect = r;
                        }

                        psdLayer.spriteName = ImportUtilities.GetUniqueSpriteName(psdLayer.name, spriteNameHash, m_KeepDupilcateSpriteName);
                        spriteSheet.name = psdLayer.spriteName;
                        spriteSheet.spritePosition = psdLayer.layerPosition + packOffsets[i];

                        if (shouldResliceFromLayer)
                            spriteSheet.rect = new Rect(spriteData[i].x, spriteData[i].y, spriteData[i].width, spriteData[i].height);

                        spriteSheet.uvTransform = uvTransform[i];

                        psdLayer.spriteID = spriteSheet.spriteID;
                        psdLayer.mosaicPosition = spriteData[i].position;
                        newSpriteMeta.Add(spriteSheet);
                    }
                    spriteImportData.Clear();
                    spriteImportData.AddRange(newSpriteMeta);
                }
                else
                {
                    spriteImportData.RemoveAll(x => removedLayersSprite.Contains(x.spriteID));

                    // First look for any user created SpriteRect and add those into the name hash
                    foreach (SpriteMetaData importData in spriteImportData)
                    {
                        PSDLayer psdLayer = psdLayers.FirstOrDefault(x => x.spriteID == importData.spriteID);
                        if (psdLayer == null)
                            spriteNameHash.AddHash(importData.name);
                    }

                    foreach (SpriteMetaData importData in spriteImportData)
                    {
                        PSDLayer psdLayer = psdLayers.FirstOrDefault(x => x.spriteID == importData.spriteID);
                        if (psdLayer == null)
                            importData.uvTransform = new Vector2Int((int)importData.rect.position.x, (int)importData.rect.position.y);
                        // If it is user created rect or the name has been changed before
                        // add it into the spriteNameHash and we don't copy it over from the layer
                        if (psdLayer == null || psdLayer.spriteName != importData.name)
                            spriteNameHash.AddHash(importData.name);

                        // If the sprite name has not been changed, we ensure the new
                        // layer name is still unique and use it as the sprite name
                        if (psdLayer != null && psdLayer.spriteName == importData.name)
                        {
                            psdLayer.spriteName = ImportUtilities.GetUniqueSpriteName(psdLayer.name, spriteNameHash, m_KeepDupilcateSpriteName);
                            importData.name = psdLayer.spriteName;
                        }
                    }

                    //Update names for those user has not changed and add new sprite rect based on PSD file.
                    for (int k = 0; k < layerIndex.Count; ++k)
                    {
                        int i = layerIndex[k];
                        SpriteMetaData spriteSheet = spriteImportData.FirstOrDefault(x => x.spriteID == psdLayers[i].spriteID);
                        bool inOldLayer = oldPsdLayers.FindIndex(x => mappingStrategy.Compare(x, psdLayers[i])) != -1;
                        if (spriteSheet == null && !inOldLayer)
                        {
                            spriteSheet = new SpriteMetaData();
                            spriteImportData.Add(spriteSheet);
                            spriteSheet.rect = new Rect(spriteData[k].x, spriteData[k].y, spriteData[k].width, spriteData[k].height);
                            spriteSheet.border = Vector4.zero;
                            spriteSheet.alignment = (SpriteAlignment)m_TextureImporterSettings.spriteAlignment;
                            spriteSheet.pivot = m_TextureImporterSettings.spritePivot;
                            spriteSheet.spritePosition = psdLayers[i].layerPosition;
                            psdLayers[i].spriteName = ImportUtilities.GetUniqueSpriteName(psdLayers[i].name, spriteNameHash, m_KeepDupilcateSpriteName);
                            spriteSheet.name = psdLayers[i].spriteName;
                        }
                        else if (spriteSheet != null)
                        {
                            Rect r = spriteSheet.rect;
                            r.position = spriteSheet.rect.position - psdLayers[i].mosaicPosition + spriteData[k].position;

                            if (inOldLayer && (m_SpriteSizeExpand > 0 || m_SpriteSizeExpandChanged))
                            {
                                r.width = spriteData[k].width;
                                r.height = spriteData[k].height;
                            }

                            spriteSheet.rect = r;
                            spriteSheet.spritePosition = psdLayers[i].layerPosition + packOffsets[k];
                        }

                        if (spriteSheet != null)
                        {
                            spriteSheet.uvTransform = uvTransform[k];
                            psdLayers[i].spriteID = spriteSheet.spriteID;
                            psdLayers[i].mosaicPosition = spriteData[k].position;
                        }
                    }
                }

                foreach (PSDLayer l in oldPsdLayers)
                    l.Dispose();
                oldPsdLayers.Clear();

                oldPsdLayers.AddRange(psdLayers);
                importData.importedTextureHeight = textureActualHeight = height;
                importData.importedTextureWidth = textureActualWidth = width;

                output = ImportTexture(ctx, outputImageBuffer, width, height, spriteImportData.ToArray());
                if (output.texture)
                {
                    importData.importedTextureHeight = output.texture.height;
                    importData.importedTextureWidth = output.texture.width;
                }
            }
            finally
            {
                if (outputImageBuffer.IsCreated)
                    outputImageBuffer.Dispose();
                foreach (PSDLayer l in psdLayers)
                    l.Dispose();
            }

            return output;
        }

        internal void MigrateOlderData()
        {
            MigrateOlderSpriteImportData();
            MigrateOlderPsdLayerData();
        }

        void MigrateOlderSpriteImportData()
        {
            // Suppressing Obsolete warning, as this method migrate data from those obsolete containers.
#pragma warning disable 0618
            bool hasMigratedData = m_LayeredSpriteImportData.Count > 0 ||
                                  m_MultiSpriteImportData.Count > 0 ||
                                  !ImportUtilities.IsSpriteMetaDataDefault(m_SingleSpriteImportData[0]);
            if (hasMigratedData)
                return;

            if (inCharacterMode)
            {
                if (!string.IsNullOrEmpty(m_SkeletonAssetReferenceID) && m_SharedRigSpriteImportData.Count > 0)
                    m_LayeredSpriteImportData = new List<SpriteMetaData>(m_SharedRigSpriteImportData);
                else if (m_RigSpriteImportData.Count > 0)
                    m_LayeredSpriteImportData = new List<SpriteMetaData>(m_RigSpriteImportData);
            }
            else if (m_MosaicSpriteImportData.Count > 0)
                m_LayeredSpriteImportData = new List<SpriteMetaData>(m_MosaicSpriteImportData);

            if (m_SpriteImportData.Count > 0)
            {
                m_SingleSpriteImportData[0] = m_SpriteImportData[0];

                if (m_SpriteImportData.Count > 1)
                    m_MultiSpriteImportData = m_SpriteImportData.GetRange(1, m_SpriteImportData.Count - 1);
            }
#pragma warning restore 0618
        }

        void MigrateOlderPsdLayerData()
        {
            // Suppressing Obsolete warning, as this method migrate data from those obsolete containers.
#pragma warning disable 0618
            bool hasMigratedData = m_PsdLayers.Count > 0;
            if (hasMigratedData)
                return;

            if (inCharacterMode)
            {
                if (!string.IsNullOrEmpty(m_SkeletonAssetReferenceID) && m_SharedRigPSDLayers.Count > 0)
                    m_PsdLayers = new List<PSDLayer>(m_SharedRigPSDLayers);
                else if (m_RigPSDLayers.Count > 0)
                    m_PsdLayers = new List<PSDLayer>(m_RigPSDLayers);
            }

            if (m_PsdLayers.Count == 0)
                m_PsdLayers = new List<PSDLayer>(m_MosaicPSDLayers);
#pragma warning restore 0618
        }

        void RegisterAssets(AssetImportContext ctx, TextureGenerationOutput output)
        {
            ctx.AddObjectToAsset("PSDImportData", m_ImportData);
            if ((output.sprites == null || output.sprites.Length == 0) && output.texture == null)
            {
                Debug.LogWarning(TextContent.noSpriteOrTextureImportWarning, this);
                return;
            }

            UniqueNameGenerator assetNameGenerator = new UniqueNameGenerator();
            if (!string.IsNullOrEmpty(output.importInspectorWarnings))
            {
                Debug.LogWarning(output.importInspectorWarnings);
            }
            if (output.importWarnings != null && output.importWarnings.Length != 0)
            {
                foreach (string warning in output.importWarnings)
                    Debug.LogWarning(warning);
            }
            if (output.thumbNail == null)
                Debug.LogWarning("Thumbnail generation fail");
            if (output.texture == null)
            {
                throw new Exception("Texture import fail");
            }

            string assetName = assetNameGenerator.GetUniqueName(System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath), true, this);
            UnityEngine.Object mainAsset = null;

            RegisterTextureAsset(ctx, output, assetName, ref mainAsset);
            RegisterSpriteLibraryAsset(ctx, output, assetName);
            RegisterGameObjects(ctx, output, ref mainAsset);
            RegisterSprites(ctx, output, assetNameGenerator);
            RegisterSkeletonAsset(ctx, output, assetName);
            RegisterTilemapAsset(ctx, output, assetName, ref mainAsset);
            ctx.SetMainObject(mainAsset);
        }

        void RegisterTextureAsset(AssetImportContext ctx, TextureGenerationOutput output, string assetName, ref UnityEngine.Object mainAsset)
        {
            string registerTextureNameId = string.IsNullOrEmpty(m_TextureAssetName) ? "Texture" : m_TextureAssetName;

            output.texture.name = assetName;
            ctx.AddObjectToAsset(registerTextureNameId, output.texture, output.thumbNail);
            mainAsset = output.texture;
        }

        void RegisterSpriteLibraryAsset(AssetImportContext ctx, TextureGenerationOutput output, string assetName)
        {
#if ENABLE_2D_ANIMATION
            if (output.sprites == null)
                return;

            SpriteLibraryAsset slAsset = ProduceSpriteLibAsset(output.sprites);
            if (slAsset == null)
                return;

            slAsset.name = assetName;
            string spriteLibAssetNameId = string.IsNullOrEmpty(m_SpriteLibAssetName) ? "SpriteLibAsset" : m_SpriteLibAssetName;
            ctx.AddObjectToAsset(spriteLibAssetNameId, slAsset);
#endif
        }

        void RegisterGameObjects(AssetImportContext ctx, TextureGenerationOutput output, ref UnityEngine.Object mainAsset)
        {
            if (output.sprites == null)
                return;
            if (!shouldProduceGameObject)
                return;

            string prefabRootNameId = string.IsNullOrEmpty(m_TextureAssetName) ? "root" : m_TextureAssetName;
            string registerPrefabNameId = string.IsNullOrEmpty(m_PrefabAssetName) ? "Prefab" : m_PrefabAssetName;

            GameObject prefab = null;
            if (m_PaperDollMode)
                prefab = OnProducePaperDollPrefab(ctx, prefabRootNameId, output.sprites);
            else
                prefab = OnProducePrefab(ctx, prefabRootNameId, output.sprites);

            if (prefab != null)
            {
                ctx.AddObjectToAsset(registerPrefabNameId, prefab);
                mainAsset = prefab;
            }
        }

        void RegisterSprites(AssetImportContext ctx, TextureGenerationOutput output, UniqueNameGenerator assetNameGenerator)
        {
            if (output.sprites == null)
                return;

            foreach (Sprite s in output.sprites)
            {
                string spriteAssetName = assetNameGenerator.GetUniqueName(s.GetSpriteID().ToString(), false, s);
                ctx.AddObjectToAsset(spriteAssetName, s);
            }
        }

        void RegisterSkeletonAsset(AssetImportContext ctx, TextureGenerationOutput output, string assetName)
        {
#if ENABLE_2D_ANIMATION
            string skeletonAssetNameId = string.IsNullOrEmpty(m_SkeletonAssetName) ? "SkeletonAsset" : m_SkeletonAssetName;

            if (output.sprites != null)
            {
                if (inCharacterMode && skeletonAsset == null)
                {
                    SkeletonAsset characterRig = ScriptableObject.CreateInstance<SkeletonAsset>();
                    characterRig.name = assetName + " Skeleton";
                    SpriteBone[] bones = GetDataProvider<ICharacterDataProvider>().GetCharacterData().bones;
                    characterRig.SetSpriteBones(bones);
                    ctx.AddObjectToAsset(skeletonAssetNameId, characterRig);
                }
            }

            if (!string.IsNullOrEmpty(m_SkeletonAssetReferenceID))
            {
                string primaryAssetPath = AssetDatabase.GUIDToAssetPath(m_SkeletonAssetReferenceID);
                if (!string.IsNullOrEmpty(primaryAssetPath) && primaryAssetPath != assetPath)
                {
                    ctx.DependsOnArtifact(primaryAssetPath);
                }
            }
#endif
        }

        void RegisterTilemapAsset(AssetImportContext ctx
            , TextureGenerationOutput output
            , string assetName
            , ref UnityEngine.Object mainAsset)
        {
            if (output.texture == null)
                return;

            if (output.sprites == null)
                return;

#if ENABLE_2D_TILEMAP_EDITOR
            if (!m_GenerateTileAssets)
                return;

            List<Sprite> sprites = new List<Sprite>();
            foreach (Sprite s in output.sprites)
            {
                if (s.rect.width > 4 && s.rect.height > 4)
                    sprites.Add(s);
            }
            GameObject paletteGO = GridPaletteUtility.CreateNewPaletteAsSubAsset(assetName
                , m_TilePaletteCellLayout
                , m_TilePaletteCellSizing
                , m_TilePaletteCellSize
                , s_TilePaletteexagonSwizzleTypeValue[m_TilePaletteHexagonLayout]
                , m_TransparencySortMode
                , m_TransparencySortAxis
                , new[] { output.texture }
                , new[] { sprites }
                , new[] { m_TileTemplate }
                , out GridPalette palette
                , out List<TileBase> tiles);

            ctx.AddObjectToAsset("GridPalette", palette);
            foreach (TileBase tile in tiles)
            {
                // Use Sprite ID + Tile for pure Tile
                if (tile is Tile t && t.sprite != null)
                {
                    ctx.AddObjectToAsset($"{t.sprite.GetSpriteID()} Tile", tile);
                }
                else
                {
                    ctx.AddObjectToAsset(tile.name, tile);
                }
            }

            ctx.AddObjectToAsset("Palette", paletteGO);
            mainAsset = paletteGO;
#endif
        }

        void BuildGroupGameObject(List<PSDLayer> psdGroup, int index, Transform root)
        {
            PSDLayer psdData = psdGroup[index];
            if (psdData.gameObject == null)
            {
                bool spriteImported = !psdGroup[index].spriteID.Empty() && psdGroup[index].isImported;
                bool isVisibleGroup = psdData.isGroup && (ImportUtilities.VisibleInHierarchy(psdGroup, index) || m_ImportHiddenLayers) && m_GenerateGOHierarchy;
                if (spriteImported || isVisibleGroup)
                {
                    SpriteMetaData spriteData = GetSpriteImportData().FirstOrDefault(x => x.spriteID == psdData.spriteID);
#if ENABLE_2D_ANIMATION
                    // Determine if need to create GameObject i.e. if the sprite is not in a SpriteLib or if it is the first one
                    bool shouldCreateGo = ImportUtilities.SpriteIsMainFromSpriteLib(m_SpriteCategoryList.categories, psdData.spriteID.ToString(), out string categoryName);
                    string goName = string.IsNullOrEmpty(categoryName) ? spriteData != null ? spriteData.name : psdData.name : categoryName;
                    if (shouldCreateGo)
#else
                    string goName = spriteData != null ? spriteData.name : psdData.name;
#endif
                    {
                        psdData.gameObject = m_GameObjectFactory.CreateGameObject(goName);
                    }
                }

                if (psdData.parentIndex >= 0 && m_GenerateGOHierarchy && psdData.gameObject != null)
                {
                    BuildGroupGameObject(psdGroup, psdData.parentIndex, root);
                    root = psdGroup[psdData.parentIndex].gameObject.transform;
                }

                if (psdData.gameObject != null)
                {
                    psdData.gameObject.transform.SetParent(root);
                    psdData.gameObject.transform.SetSiblingIndex(root.childCount - 1);
                }
            }
        }

        void CreateBoneGO(int index, SpriteBone[] bones, BoneGO[] bonesGO, Transform defaultRoot)
        {
            if (bonesGO[index].go != null)
                return;
            SpriteBone bone = bones[index];
            if (bone.parentId != -1 && bonesGO[bone.parentId].go == null)
                CreateBoneGO(bone.parentId, bones, bonesGO, defaultRoot);

            GameObject go = m_GameObjectFactory.CreateGameObject(bone.name);
            if (bone.parentId == -1)
                go.transform.SetParent(defaultRoot);
            else
                go.transform.SetParent(bonesGO[bone.parentId].go.transform);
            go.transform.localPosition = bone.position * 1 / pixelsPerUnit;
            go.transform.localRotation = bone.rotation;
            bonesGO[index] = new BoneGO()
            {
                go = go,
                index = index
            };
        }

        BoneGO[] CreateBonesGO(Transform root)
        {
#if ENABLE_2D_ANIMATION
            if (inCharacterMode)
            {
                CharacterData characterSkeleton = GetDataProvider<ICharacterDataProvider>().GetCharacterData();
                SpriteBone[] bones = characterSkeleton.bones;
                if (bones != null)
                {
                    BoneGO[] boneGOs = new BoneGO[bones.Length];
                    for (int i = 0; i < bones.Length; ++i)
                    {
                        CreateBoneGO(i, bones, boneGOs, root);
                    }
                    return boneGOs;
                }
            }
#endif
            return new BoneGO[0];
        }

        void GetSpriteLibLabel(string spriteId, out string category, out string label)
        {
            category = "";
            label = "";

#if ENABLE_2D_ANIMATION
            foreach (SpriteCategory cat in m_SpriteCategoryList.categories)
            {
                int index = cat.labels.FindIndex(x => x.spriteId == spriteId);
                if (index != -1)
                {
                    category = cat.name;
                    label = cat.labels[index].name;
                    break;
                }
            }
#endif
        }

        GameObject OnProducePaperDollPrefab(AssetImportContext ctx, string assetName, Sprite[] sprites)
        {
            GameObject root = null;
            if (sprites != null && sprites.Length > 0)
            {
                root = new GameObject();
                root.name = assetName + "_GO";

#if ENABLE_2D_ANIMATION
                List<SpriteMetaData> spriteImportData = GetSpriteImportData();
                List<PSDLayer> psdLayers = GetPSDLayers();
                BoneGO[] boneGOs = CreateBonesGO(root.transform);

                List<UnityEngine.Object> contextObjects = new List<UnityEngine.Object>();
                ctx.GetObjects(contextObjects);
                SpriteLibraryAsset spriteLib = contextObjects.Find(x => x.GetType() == typeof(SpriteLibraryAsset)) as SpriteLibraryAsset;

                if (spriteLib != null)
                    root.AddComponent<SpriteLibrary>().spriteLibraryAsset = spriteLib;

                CharacterData currentCharacterData = characterData;
                for (int i = 0; i < sprites.Length; ++i)
                {
                    if (ImportUtilities.SpriteIsMainFromSpriteLib(m_SpriteCategoryList.categories, sprites[i].GetSpriteID().ToString(), out string categoryName))
                    {
                        int[] spriteBones = currentCharacterData.parts.FirstOrDefault(x => new GUID(x.spriteId) == sprites[i].GetSpriteID()).bones;
                        GameObject rootBone = root;
                        if (spriteBones != null && spriteBones.Any())
                        {
                            IOrderedEnumerable<BoneGO> b = spriteBones.Where(x => x >= 0 && x < boneGOs.Length).Select(x => boneGOs[x]).OrderBy(x => x.index);
                            if (b.Any())
                                rootBone = b.First().go;
                        }

                        GameObject srGameObject = m_GameObjectFactory.CreateGameObject(string.IsNullOrEmpty(categoryName) ? sprites[i].name : categoryName);
                        SpriteRenderer sr = srGameObject.AddComponent<SpriteRenderer>();
                        sr.sprite = sprites[i];
                        sr.sortingOrder = psdLayers.Count - psdLayers.FindIndex(x => x.spriteID == sprites[i].GetSpriteID());
                        srGameObject.transform.parent = rootBone.transform;
                        SpriteMetaData spriteMetaData = spriteImportData.FirstOrDefault(x => x.spriteID == sprites[i].GetSpriteID());
                        if (spriteMetaData != null)
                        {
                            Vector2Int uvTransform = spriteMetaData.uvTransform;
                            Vector2 outlineOffset = new Vector2(spriteMetaData.rect.x - uvTransform.x + (spriteMetaData.pivot.x * spriteMetaData.rect.width),
                                spriteMetaData.rect.y - uvTransform.y + (spriteMetaData.pivot.y * spriteMetaData.rect.height)) * definitionScale / sprites[i].pixelsPerUnit;
                            srGameObject.transform.position = new Vector3(outlineOffset.x, outlineOffset.y, 0);
                        }

                        GetSpriteLibLabel(sprites[i].GetSpriteID().ToString(), out string category, out string labelName);
                        if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(labelName))
                        {
                            SpriteResolver resolver = srGameObject.AddComponent<SpriteResolver>();
                            resolver.SetCategoryAndLabel(category, labelName);
                            resolver.ResolveSpriteToSpriteRenderer();
                        }
                    }
                }
#endif
            }
            return root;
        }

        internal void SetPlatformTextureSettings(TextureImporterPlatformSettings platformSettings)
        {
            int index = m_PlatformSettings.FindIndex(x => x.name == platformSettings.name);
            if (index < 0)
                m_PlatformSettings.Add(platformSettings);
            else
                m_PlatformSettings[index] = platformSettings;
        }

        internal TextureImporterPlatformSettings[] GetAllPlatformSettings()
        {
            return m_PlatformSettings.ToArray();
        }

        GameObject OnProducePrefab(AssetImportContext ctx, string assetname, Sprite[] sprites)
        {
            GameObject root = null;
            if (sprites != null && sprites.Length > 0)
            {
                List<SpriteMetaData> spriteImportData = GetSpriteImportData();
                root = new GameObject();
                root.transform.SetSiblingIndex(0);
                root.name = assetname + "_GO";

#if ENABLE_2D_ANIMATION
                CharacterData currentCharacterData = characterData;

                List<UnityEngine.Object> contextObjects = new List<UnityEngine.Object>();
                ctx.GetObjects(contextObjects);
                SpriteLibraryAsset spriteLib = contextObjects.Find(x => x.GetType() == typeof(SpriteLibraryAsset)) as SpriteLibraryAsset;

                if (spriteLib != null)
                    root.AddComponent<SpriteLibrary>().spriteLibraryAsset = spriteLib;

                CharacterData? characterSkeleton = inCharacterMode ? new CharacterData?(GetDataProvider<ICharacterDataProvider>().GetCharacterData()) : null;
#endif

                List<PSDLayer> psdLayers = GetPSDLayers();
                for (int i = 0; i < psdLayers.Count; ++i)
                {
                    BuildGroupGameObject(psdLayers, i, root.transform);
                }
                BoneGO[] boneGOs = CreateBonesGO(root.transform);
                for (int i = 0; i < psdLayers.Count; ++i)
                {
                    PSDLayer l = psdLayers[i];
                    GUID layerSpriteID = l.spriteID;
                    Sprite sprite = sprites.FirstOrDefault(x => x.GetSpriteID() == layerSpriteID);
                    SpriteMetaData spriteMetaData = spriteImportData.FirstOrDefault(x => x.spriteID == layerSpriteID);
                    if (sprite != null && spriteMetaData != null && l.gameObject != null)
                    {
                        SpriteRenderer spriteRenderer = l.gameObject.AddComponent<SpriteRenderer>();
                        spriteRenderer.sprite = sprite;
                        spriteRenderer.sortingOrder = psdLayers.Count - i;

                        Vector2 pivot = spriteMetaData.pivot;
                        pivot.x *= spriteMetaData.rect.width;
                        pivot.y *= spriteMetaData.rect.height;

                        Vector2 spritePosition = spriteMetaData.spritePosition;
                        spritePosition.x += pivot.x;
                        spritePosition.y += pivot.y;
                        spritePosition *= (definitionScale / sprite.pixelsPerUnit);

                        l.gameObject.transform.position = new Vector3(spritePosition.x, spritePosition.y, 0f);

#if ENABLE_2D_ANIMATION
                        if (characterSkeleton != null)
                        {
                            CharacterPart part = characterSkeleton.Value.parts.FirstOrDefault(x => x.spriteId == spriteMetaData.spriteID.ToString());
                            if (part.bones != null && part.bones.Length > 0)
                            {
                                SpriteSkin spriteSkin = l.gameObject.AddComponent<SpriteSkin>();
                                if (spriteRenderer.sprite != null && spriteRenderer.sprite.GetBindPoses().Length > 0)
                                {
                                    IEnumerable<BoneGO> spriteBones = currentCharacterData.parts.FirstOrDefault(x => new GUID(x.spriteId) == spriteRenderer.sprite.GetSpriteID()).bones.Where(x => x >= 0 && x < boneGOs.Length).Select(x => boneGOs[x]);
                                    if (spriteBones.Any())
                                    {
                                        spriteSkin.SetRootBone(root.transform);
                                        spriteSkin.SetBoneTransforms(spriteBones.Select(x => x.go.transform).ToArray());
                                        if (spriteSkin.isValid)
                                            spriteSkin.CalculateBounds();
                                    }
                                }
                            }
                        }

                        GetSpriteLibLabel(layerSpriteID.ToString(), out string category, out string labelName);
                        if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(labelName))
                        {
                            SpriteResolver resolver = l.gameObject.AddComponent<SpriteResolver>();
                            resolver.SetCategoryAndLabel(category, labelName);
                            resolver.ResolveSpriteToSpriteRenderer();
                        }
#endif
                    }
                }

                Rect prefabBounds = new Rect(0, 0, importData.documentSize.x / pixelsPerUnit, importData.documentSize.y / pixelsPerUnit);
                Vector3 documentPivot = (Vector3)ImportUtilities.GetPivotPoint(prefabBounds, m_DocumentAlignment, m_DocumentPivot);
                for (int i = 0; i < psdLayers.Count; ++i)
                {
                    PSDLayer l = psdLayers[i];
                    if (l.gameObject == null || l.gameObject.GetComponent<SpriteRenderer>() == null)
                        continue;
                    Vector3 p = l.gameObject.transform.localPosition;
                    p -= documentPivot;
                    l.gameObject.transform.localPosition = p;
                }
                for (int i = 0; i < boneGOs.Length; ++i)
                {
                    if (boneGOs[i].go.transform.parent != root.transform)
                        continue;
                    Vector3 p = boneGOs[i].go.transform.position;
                    p -= documentPivot;
                    boneGOs[i].go.transform.position = p;
                }
            }

            return root;
        }

        int spriteDataCount
        {
            get
            {
                List<SpriteMetaData> spriteImportData = GetSpriteImportData();
                if (inMosaicMode)
                    return spriteImportData.Count;
                if (spriteImportModeToUse != SpriteImportMode.Multiple)
                    return 1;
                return spriteImportData.Count - 1;
            }
        }

        internal void Apply()
        {
            // Do this so that asset change save dialog will not show
            bool originalValue = EditorPrefs.GetBool("VerifySavingAssets", false);
            EditorPrefs.SetBool("VerifySavingAssets", false);
            AssetDatabase.ForceReserializeAssets(new string[] { assetPath }, ForceReserializeAssetsOptions.ReserializeMetadata);
            EditorPrefs.SetBool("VerifySavingAssets", originalValue);
        }

#if ENABLE_2D_ANIMATION
        SkeletonAsset skeletonAsset =>
            AssetDatabase.LoadAssetAtPath<SkeletonAsset>(AssetDatabase.GUIDToAssetPath(m_SkeletonAssetReferenceID));
#endif

        internal List<PSDLayer> GetPSDLayers() => m_PsdLayers;

        List<SpriteMetaData> GetSpriteImportData()
        {
            if (spriteImportModeToUse == SpriteImportMode.Multiple)
            {
                if (inMosaicMode)
                    return m_LayeredSpriteImportData;
                return m_MultiSpriteImportData;
            }

            return new List<SpriteMetaData> { GetSingleSpriteImportData() };
        }

        internal SpriteMetaData[] GetSpriteMetaData()
        {
            if (spriteImportModeToUse == SpriteImportMode.Multiple)
            {
                if (inMosaicMode)
                    return m_LayeredSpriteImportData.ToArray();
                return m_MultiSpriteImportData.ToArray();
            }

            return new[] { GetSingleSpriteImportData() };
        }

        internal SpriteRect GetSpriteData(GUID guid)
        {
            if (spriteImportModeToUse == SpriteImportMode.Multiple)
            {
                if (inMosaicMode)
                    return m_LayeredSpriteImportData.FirstOrDefault(x => x.spriteID == guid);
                return m_MultiSpriteImportData.FirstOrDefault(x => x.spriteID == guid);
            }

            return GetSingleSpriteImportData();
        }

        SpriteMetaData GetSingleSpriteImportData()
        {
            SpriteMetaData spriteMetaData = new SpriteMetaData();
            spriteMetaData.spriteID = AssetDatabase.GUIDFromAssetPath(assetPath);
            if (m_SingleSpriteImportData == null || m_SingleSpriteImportData.Count < 1 && m_SingleSpriteImportData[0] != null)
                spriteMetaData.Copy(m_SingleSpriteImportData[0]);
            if (assetPath != null)
                spriteMetaData.name = System.IO.Path.GetFileNameWithoutExtension(assetPath) + "_1";
            if (importData != null)
            {
                spriteMetaData.rect = new Rect(0, 0, importData.importedTextureWidth, importData.importedTextureHeight);
                spriteMetaData.pivot = m_TextureImporterSettings.spritePivot;
                spriteMetaData.alignment = (SpriteAlignment)m_TextureImporterSettings.spriteAlignment;
                spriteMetaData.border = m_TextureImporterSettings.spriteBorder;
            }

            return spriteMetaData;
        }

        internal Vector2 GetDocumentPivot()
        {
            return ImportUtilities.GetPivotPoint(new Rect(0, 0, 1, 1), m_DocumentAlignment, m_DocumentPivot);
        }

        internal void SetDocumentPivot(Vector2 pivot)
        {
            ImportUtilities.TranslatePivotPoint(pivot, new Rect(0, 0, 1, 1), out m_DocumentAlignment, out m_DocumentPivot);
        }

        bool inMosaicMode => spriteImportModeToUse == SpriteImportMode.Multiple && m_MosaicLayers;

        SpriteImportMode spriteImportModeToUse =>
            m_TextureImporterSettings.textureType != TextureImporterType.Sprite ?
                SpriteImportMode.None :
                (SpriteImportMode)m_TextureImporterSettings.spriteMode;

        internal Vector2Int canvasSize => importData.documentSize;

#if ENABLE_2D_ANIMATION
        internal CharacterData characterData
        {
            get
            {
                if (skeletonAsset != null)
                    return m_SharedRigCharacterData;
                return m_CharacterData;
            }
            set
            {
                if (skeletonAsset != null)
                    m_SharedRigCharacterData = value;
                else
                    m_CharacterData = value;
            }
        }

        SpriteLibraryAsset ProduceSpriteLibAsset(Sprite[] sprites)
        {
            if (!inCharacterMode || m_SpriteCategoryList.categories == null)
                return null;
            List<SpriteLibCategory> categories = m_SpriteCategoryList.categories.Select(x =>
                new SpriteLibCategory()
                {
                    name = x.name,
                    categoryList = x.labels.Select(y =>
                    {
                        Sprite sprite = sprites.FirstOrDefault(z => z.GetSpriteID().ToString() == y.spriteId);
                        return new SpriteCategoryEntry()
                        {
                            name = y.name,
                            sprite = sprite
                        };
                    }).ToList()
                }).ToList();
            categories.RemoveAll(x => x.categoryList.Count == 0);
            if (categories.Count > 0)
            {
                // Always set version to 0 since we will never be updating this
                return SpriteLibraryAsset.CreateAsset(categories, "Sprite Lib", 0);
            }
            return null;
        }
#endif

        internal void ReadTextureSettings(TextureImporterSettings dest)
        {
            m_TextureImporterSettings.CopyTo(dest);
        }

        internal IPSDLayerMappingStrategy GetLayerMappingStrategy()
        {
            return m_MappingCompare[(int)m_LayerMappingOption];
        }

        static void SetPhysicsOutline(ISpritePhysicsOutlineDataProvider physicsOutlineDataProvider, Sprite[] sprites, float definitionScale, float pixelsPerUnit, bool generatePhysicsShape)
        {
            foreach (Sprite sprite in sprites)
            {
                GUID guid = sprite.GetSpriteID();
                List<Vector2[]> outline = physicsOutlineDataProvider.GetOutlines(guid);

                Vector2 outlineOffset = sprite.rect.size / 2;
                bool generated = false;
                if ((outline == null || outline.Count == 0) && generatePhysicsShape)
                {
                    InternalEditorBridge.GenerateOutlineFromSprite(sprite, 0.25f, 200, true, out Vector2[][] defaultOutline);
                    outline = new List<Vector2[]>(defaultOutline.Length);
                    for (int i = 0; i < defaultOutline.Length; ++i)
                    {
                        outline.Add(defaultOutline[i]);
                    }

                    generated = true;
                }
                if (outline != null && outline.Count > 0)
                {
                    // Ensure that outlines are all valid.
                    int validOutlineCount = 0;
                    for (int i = 0; i < outline.Count; ++i)
                        validOutlineCount += ((outline[i].Length > 2) ? 1 : 0);

                    int index = 0;
                    Vector2[][] convertedOutline = new Vector2[validOutlineCount][];
                    float useScale = generated ? pixelsPerUnit * definitionScale : definitionScale;

                    for (int i = 0; i < outline.Count; ++i)
                    {
                        if (outline[i].Length > 2)
                        {
                            convertedOutline[index] = new Vector2[outline[i].Length];
                            for (int j = 0; j < outline[i].Length; ++j)
                            {
                                convertedOutline[index][j] = outline[i][j] * useScale + outlineOffset;
                            }
                            index++;
                        }
                    }
                    sprite.OverridePhysicsOutline(convertedOutline);
                }
            }
        }
    }
}
