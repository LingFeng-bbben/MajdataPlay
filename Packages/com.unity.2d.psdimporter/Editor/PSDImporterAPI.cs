#if UNITY_6000_1_OR_NEWER
#define ENABLE_2D_TILEMAP_EDITOR
#endif

using System;
using UnityEditor.AssetImporters;
#if ENABLE_2D_TILEMAP_EDITOR
using UnityEditor.Tilemaps;
#endif
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    public partial class PSDImporter : ScriptedImporter, ISpriteEditorDataProvider
    {
        /// <summary>
        /// Set this to true if you want texture data to be readable from scripts. Set it to false to prevent scripts from reading texture data.
        /// <br/><br/>In order for Texture2D.GetPixel, Texture2D.GetPixels, ImageConversion.EncodeToEXR, ImageConversion.EncodeToJPG, ImageConversion.EncodeToPNG and similar functions to work, the Texture must be readable from scripts. The isReadable setting determines whether scripts can access texture data through these functions.
        /// <br/><br/>Textures are not set as readable by default.
        /// <br/><br/>When a Texture is not readable, it consumes much less memory because an uncompressed copy of the texture data in system memory is not required after the texture is uploaded to the graphics API. Readable Textures require an uncompressed system memory copy of the texture data so that once edited, the updated texture data can be uploaded to the graphics API.
        /// </summary>
        public bool isReadable
        {
            get => m_TextureImporterSettings.readable;
            set
            {
                m_TextureImporterSettings.readable = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Anisotropic filtering level of the texture.
        /// </summary>
        public int anisoLevel
        {
            get => m_TextureImporterSettings.aniso;
            set
            {
                m_TextureImporterSettings.aniso = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Keeps texture borders the same when generating mipmaps.
        /// </summary>
        public bool borderMipmap
        {
            get => m_TextureImporterSettings.borderMipmap;
            set
            {
                m_TextureImporterSettings.borderMipmap = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Fades out mip levels to a gray color.
        /// </summary>
        public bool fadeout
        {
            get => m_TextureImporterSettings.fadeOut;
            set
            {
                m_TextureImporterSettings.fadeOut = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Filtering mode of the texture.
        /// </summary>
        public FilterMode filterMode
        {
            get => m_TextureImporterSettings.filterMode;
            set
            {
                m_TextureImporterSettings.filterMode = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Mip map bias of the texture.
        /// </summary>
        public float mipMapBias
        {
            get => m_TextureImporterSettings.mipmapBias;
            set
            {
                m_TextureImporterSettings.mipmapBias = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Generate Mip Maps.
        /// <br/><br/>Select this to enable mip-map generation. Mipmaps are smaller versions of the Texture that get used when the Texture is very small on screen.
        /// </summary>
        public bool mipmapEnabled
        {
            get => m_TextureImporterSettings.mipmapEnabled;
            set
            {
                m_TextureImporterSettings.mipmapEnabled = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Mip level where texture is faded out completely.
        /// </summary>
        public int mipmapFadeDistanceEnd
        {
            get => m_TextureImporterSettings.mipmapFadeDistanceEnd;
            set
            {
                m_TextureImporterSettings.mipmapFadeDistanceEnd = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Mip level where texture begins to fade out.
        /// </summary>
        public int mipmapFadeDistanceStart
        {
            get => m_TextureImporterSettings.mipmapFadeDistanceStart;
            set
            {
                m_TextureImporterSettings.mipmapFadeDistanceEnd = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Enable mipmap streaming for the texture.
        /// <br/><br/>Only load larger mipmaps as needed to render the current game cameras. Requires texture streaming to be enabled in quality settings.
        /// </summary>
        public bool streamingMipmaps
        {
            get => m_TextureImporterSettings.streamingMipmaps;
            set
            {
                m_TextureImporterSettings.streamingMipmaps = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Mipmap streaming priority when there's contention for resources. Positive numbers represent higher priority. Valid range is -128 to 127.
        /// </summary>
        public int streamingMipmapsPriority
        {
            get => m_TextureImporterSettings.streamingMipmapsPriority;
            set
            {
                m_TextureImporterSettings.streamingMipmapsPriority = Mathf.Clamp(value, -128, 127);
                SetDirty();
            }
        }

        /// <summary>
        /// Mip level where texture is faded out completely.
        /// </summary>
        public TextureImporterMipFilter mipmapFilter
        {
            get => m_TextureImporterSettings.mipmapFilter;
            set
            {
                m_TextureImporterSettings.mipmapFilter = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Enables or disables coverage-preserving alpha mipmapping.
        /// <br/><br/>Enable this to rescale the alpha values of computed mipmaps so coverage is preserved. This means a higher percentage of pixels passes the alpha test and lower mipmap levels do not become more transparent. This is disabled by default (set to false).
        /// </summary>
        public bool mipMapsPreserveCoverage
        {
            get => m_TextureImporterSettings.mipMapsPreserveCoverage;
            set
            {
                m_TextureImporterSettings.mipMapsPreserveCoverage = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Selects Single or Manual import mode for Sprite textures.
        /// </summary>
        /// <value>Valid values are SpriteImportMode.Multiple or SpriteImportMode.Single.</value>
        /// <exception cref="ArgumentException">Exception when non valid values are set.</exception>
        public SpriteImportMode spriteImportMode
        {
            get { return (SpriteImportMode)m_TextureImporterSettings.spriteMode; }
            set
            {
                if (value == SpriteImportMode.Multiple || value == SpriteImportMode.Single)
                {
                    m_TextureImporterSettings.spriteMode = (int)value;
                    SetDirty();
                }
                else
                    throw new ArgumentException("Invalid value. Valid values are SpriteImportMode.Multiple or SpriteImportMode.Single");
            }
        }

        /// <summary>
        /// Sets the type of mesh to ge generated for each Sprites.
        /// </summary>
        public SpriteMeshType spriteMeshType
        {
            get { return m_TextureImporterSettings.spriteMeshType; }
            set
            {
                m_TextureImporterSettings.spriteMeshType = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Which type of texture are we dealing with here.
        /// </summary>
        /// <value>Valid values are TextureImporterType.Default or TextureImporterType.Sprite.</value>
        /// <exception cref="ArgumentException">Exception when non valid values are set.</exception>
        public TextureImporterType textureType
        {
            get { return (TextureImporterType)m_TextureImporterSettings.textureType; }
            set
            {
                if (value == TextureImporterType.Sprite || value == TextureImporterType.Default)
                {
                    m_TextureImporterSettings.textureType = value;
                    SetDirty();
                }
                else
                    throw new ArgumentException("Invalid value. Valid values are TextureImporterType.Sprite or TextureImporterType.Default");
            }
        }

        /// <summary>
        /// Texture coordinate wrapping mode.
        /// <br/><br/>Using wrapMode sets the same wrapping mode on all axes. Different per-axis wrap modes can be set using wrapModeU, wrapModeV, wrapModeW. Querying the value returns the U axis wrap mode (same as wrapModeU getter).
        /// </summary>
        public TextureWrapMode wrapMode
        {
            get => m_TextureImporterSettings.wrapMode;
            set
            {
                m_TextureImporterSettings.wrapMode = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Texture U coordinate wrapping mode.
        /// <br/><br/>Controls wrapping mode along texture U (horizontal) axis.
        /// </summary>
        public TextureWrapMode wrapModeU
        {
            get => m_TextureImporterSettings.wrapModeU;
            set
            {
                m_TextureImporterSettings.wrapModeU = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Texture V coordinate wrapping mode.
        /// <br/><br/>Controls wrapping mode along texture V (vertical) axis.
        /// </summary>
        public TextureWrapMode wrapModeV
        {
            get => m_TextureImporterSettings.wrapModeV;
            set
            {
                m_TextureImporterSettings.wrapModeV = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Texture W coordinate wrapping mode for Texture3D.
        /// <br/><br/>Controls wrapping mode along texture W (depth, only relevant for Texture3D) axis.
        /// </summary>
        public TextureWrapMode wrapModeW
        {
            get => m_TextureImporterSettings.wrapModeW;
            set
            {
                m_TextureImporterSettings.wrapModeW = value;
                SetDirty();
            }
        }

        /// <summary>
        /// The number of pixels in the sprite that correspond to one unit in world space.
        /// </summary>
        public float spritePixelsPerUnit
        {
            get => m_TextureImporterSettings.spritePixelsPerUnit;
            set
            {
                m_TextureImporterSettings.spritePixelsPerUnit = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Retrieves the platform settings used by the importer for a given build target.
        /// </summary>
        /// <param name="buildTarget">The build target to query.</param>
        /// <returns>TextureImporterPlatformSettings used for importing the texture for the build target.</returns>
        public TextureImporterPlatformSettings GetImporterPlatformSettings(BuildTarget buildTarget)
        {
            return TextureImporterUtilities.GetPlatformTextureSettings(buildTarget, in m_PlatformSettings);
        }

        /// <summary>
        /// Sets the platform settings used by the importer for a given build target.
        /// </summary>
        /// <param name="setting">TextureImporterPlatformSettings to be used by the importer for the build target indicated by TextureImporterPlatformSettings.</param>
        public void SetImporterPlatformSettings(TextureImporterPlatformSettings setting)
        {
            SetPlatformTextureSettings(setting);
            SetDirty();
        }

        /// <summary>
        /// Secondary textures for the imported Sprites.
        /// </summary>
        public SecondarySpriteTexture[] secondarySpriteTextures
        {
            get => secondaryTextures;
            set
            {
                secondaryTextures = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Sets if importer should generate a prefab as sub-asset.
        /// To generate a Prefab useMosaicMode needs to be set to true and importer needs to be set to import
        /// Sprites in multiple mode.
        /// </summary>
        public bool useCharacterMode
        {
            get => m_CharacterMode;
            set
            {
                m_CharacterMode = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Sets if importer should generate a mosaic texture from the source layers.
        /// To generate such texture, the importer needs to be set to import Sprites in multiple mode.
        /// </summary>
        public bool useMosaicMode
        {
            get => m_MosaicLayers;
            set
            {
                m_MosaicLayers = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Sets the padding between each Sprites in the mosaic texture.
        /// </summary>
        public uint mosiacPadding
        {
            get => (uint)m_Padding;
            set
            {
                m_Padding = (int)value;
                SetDirty();
            }
        }

        /// <summary>
        /// Sets the value to increase the Sprite size by.
        /// </summary>
        public ushort spriteSizeExpand
        {
            get => m_SpriteSizeExpand;
            set
            {
                m_SpriteSizeExpand = value;
                m_SpriteSizeExpandChanged = true;
                SetDirty();
            }
        }

#if ENABLE_2D_TILEMAP_EDITOR
        /// <summary>
        /// Sets whether to generate Tile assets.
        /// </summary>
        public bool generateTileAssets
        {
            get => m_GenerateTileAssets;
            set
            {
                m_GenerateTileAssets = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Cell Layout for generated Tile Palette
        /// </summary>
        public GridLayout.CellLayout tilePaletteCellLayout
        {
            get => m_TilePaletteCellLayout;
            set
            {
                m_TilePaletteCellLayout = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Hexagonal Layout for generated Tile Palette
        /// </summary>
        public int tilePaletteHexagonLayout
        {
            get => m_TilePaletteHexagonLayout;
            set
            {
                m_TilePaletteHexagonLayout = value != 0 ? 1 : 0;
                SetDirty();
            }
        }

        /// <summary>
        /// Cell Size for generated Tile Palette
        /// </summary>
        public Vector3 tilePaletteCellSize
        {
            get => m_TilePaletteCellSize;
            set
            {
                m_TilePaletteCellSize = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Cell Sizing for generated Tile Palette
        /// </summary>
        public GridPalette.CellSizing tilePaletteCellSizing
        {
            get => m_TilePaletteCellSizing;
            set
            {
                m_TilePaletteCellSizing = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Transparency Sort Mode for generated Tile Palette
        /// </summary>
        public TransparencySortMode transparencySortMode
        {
            get => m_TransparencySortMode;
            set
            {
                m_TransparencySortMode = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Transparency Sort Axis for generated Tile Palette
        /// </summary>
        public Vector3 transparencySortAxis
        {
            get => m_TransparencySortAxis;
            set
            {
                m_TransparencySortAxis = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Tile Template for importing Tile Palette
        /// </summary>
        public TileTemplate tileTemplate
        {
            get => m_TileTemplate;
            set
            {
                m_TileTemplate = value;
                SetDirty();
            }
        }
#endif

        internal TextureImporterSwizzle swizzleR
        {
            get => m_TextureImporterSettings.swizzleR;
            set
            {
                m_TextureImporterSettings.swizzleR = value;
                SetDirty();
            }
        }

        internal TextureImporterSwizzle swizzleG
        {
            get => m_TextureImporterSettings.swizzleG;
            set
            {
                m_TextureImporterSettings.swizzleG = value;
                SetDirty();
            }
        }

        internal TextureImporterSwizzle swizzleB
        {
            get => m_TextureImporterSettings.swizzleB;
            set
            {
                m_TextureImporterSettings.swizzleB = value;
                SetDirty();
            }
        }

        internal TextureImporterSwizzle swizzleA
        {
            get => m_TextureImporterSettings.swizzleA;
            set
            {
                m_TextureImporterSettings.swizzleA = value;
                SetDirty();
            }
        }

        internal bool sRGBTexture
        {
            get => m_TextureImporterSettings.sRGBTexture;
            set
            {
                m_TextureImporterSettings.sRGBTexture = value;
                SetDirty();
            }
        }

        void SetDirty()
        {
            EditorUtility.SetDirty(this);
        }
    }
}
