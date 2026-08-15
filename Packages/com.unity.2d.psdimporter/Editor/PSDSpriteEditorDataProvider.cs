using System;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.Sprites;

#if ENABLE_2D_ANIMATION
using UnityEditor.U2D.Animation;
#endif

namespace UnityEditor.U2D.PSD
{
    public partial class PSDImporter : ScriptedImporter, ISpriteEditorDataProvider
    {
        SpriteImportMode ISpriteEditorDataProvider.spriteImportMode => spriteImportModeToUse;
        UnityEngine.Object ISpriteEditorDataProvider.targetObject => targetObject;
        internal UnityEngine.Object targetObject => this;

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.pixelsPerUnit.
        /// </summary>
        float ISpriteEditorDataProvider.pixelsPerUnit => pixelsPerUnit;
        internal float pixelsPerUnit => m_TextureImporterSettings.spritePixelsPerUnit;

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.GetDataProvider.
        /// </summary>
        /// <typeparam name="T">Data provider type to retrieve.</typeparam>
        /// <returns></returns>
        T ISpriteEditorDataProvider.GetDataProvider<T>()
        {
            return GetDataProvider<T>();
        }

        internal T GetDataProvider<T>() where T : class
        {
            if (typeof(T) == typeof(ISpriteBoneDataProvider))
            {
                return new SpriteBoneDataProvider { dataProvider = this } as T;
            }
            if (typeof(T) == typeof(ISpriteMeshDataProvider))
            {
                return new SpriteMeshDataProvider { dataProvider = this } as T;
            }
            if (typeof(T) == typeof(ISpriteOutlineDataProvider))
            {
                return new SpriteOutlineDataProvider { dataProvider = this } as T;
            }
            if (typeof(T) == typeof(ISpritePhysicsOutlineDataProvider))
            {
                return new SpritePhysicsOutlineProvider { dataProvider = this } as T;
            }
            if (typeof(T) == typeof(ITextureDataProvider))
            {
                return new TextureDataProvider { dataProvider = this } as T;
            }
            if (typeof(T) == typeof(ISecondaryTextureDataProvider))
            {
                return new SecondaryTextureDataProvider() { dataProvider = this } as T;
            }
#if USE_SPRITE_FRAME_CAPABILITY
            if (typeof(T) == typeof(ISpriteFrameEditCapability))
            {
                return new SpriteFrameEditCapabilityDataProvider() { dataProvider = this } as T;
            }
#endif
#if ENABLE_2D_ANIMATION
            if (typeof(T) == typeof(ICharacterDataProvider))
            {
                return inCharacterMode ? new CharacterDataProvider { dataProvider = this } as T : null;
            }
            if (typeof(T) == typeof(IMainSkeletonDataProvider))
            {
                return inCharacterMode && skeletonAsset != null ? new MainSkeletonDataProvider() { dataProvider = this } as T : null;
            }
#endif
            else
                return this as T;
        }

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.HasDataProvider.
        /// </summary>
        /// <param name="type">Data provider type to query.</param>
        /// <returns>True if data provider is supported, false otherwise.</returns>
        bool ISpriteEditorDataProvider.HasDataProvider(Type type)
        {
            return HasDataProvider(type);
        }

        internal bool HasDataProvider(Type type)
        {
#if ENABLE_2D_ANIMATION
            if (inCharacterMode)
            {
                if (type == typeof(ICharacterDataProvider))
                    return true;
                if (type == typeof(IMainSkeletonDataProvider) && skeletonAsset != null)
                    return true;
            }
#endif
            if (type == typeof(ISpriteBoneDataProvider) ||
                type == typeof(ISpriteMeshDataProvider) ||
                type == typeof(ISpriteOutlineDataProvider) ||
                type == typeof(ISpritePhysicsOutlineDataProvider) ||
                type == typeof(ITextureDataProvider) ||
                type == typeof(ISecondaryTextureDataProvider))
            {
                return true;
            }
            else
                return type.IsAssignableFrom(GetType());
        }

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.Apply.
        /// </summary>
        void ISpriteEditorDataProvider.Apply()
        {
            Apply();
        }

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.InitSpriteEditorDataProvider.
        /// </summary>
        void ISpriteEditorDataProvider.InitSpriteEditorDataProvider()
        {
            InitSpriteEditorDataProvider();
        }

        void InitSpriteEditorDataProvider() { }

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.GetSpriteRects.
        /// </summary>
        /// <returns>An array of SpriteRect for the current import mode.</returns>
        SpriteRect[] ISpriteEditorDataProvider.GetSpriteRects()
        {
            return GetSpriteRects();
        }

        internal SpriteRect[] GetSpriteRects()
        {
            if (spriteImportModeToUse == SpriteImportMode.Multiple)
            {
                if (inMosaicMode)
                    return m_LayeredSpriteImportData.Select(x => new SpriteMetaData(x) as SpriteRect).ToArray();
                return m_MultiSpriteImportData.Select(x => new SpriteMetaData(x) as SpriteRect).ToArray();
            }

            return new[] { GetSingleSpriteImportData() };
        }

        /// <summary>
        /// Implementation for ISpriteEditorDataProvider.SetSpriteRects.
        /// </summary>
        /// <param name="spriteRects">Set the SpriteRect data for the current import mode.</param>
        void ISpriteEditorDataProvider.SetSpriteRects(SpriteRect[] spriteRects)
        {
            SetSpriteRects(spriteRects);
        }

        internal void SetSpriteRects(SpriteRect[] spriteRects)
        {
            System.Collections.Generic.List<SpriteMetaData> spriteImportData = GetSpriteImportData();
            if (spriteImportModeToUse == SpriteImportMode.Multiple)
            {
                spriteImportData.RemoveAll(data => spriteRects.FirstOrDefault(x => x.spriteID == data.spriteID) == null);
                foreach (SpriteRect sr in spriteRects)
                {
                    SpriteMetaData importData = spriteImportData.FirstOrDefault(x => x.spriteID == sr.spriteID);
                    if (importData == null)
                        spriteImportData.Add(new SpriteMetaData(sr));
                    else
                    {
                        importData.name = sr.name;
                        importData.alignment = sr.alignment;
                        importData.border = sr.border;
                        importData.pivot = sr.pivot;
                        importData.rect = sr.rect;
                    }
                }
            }
            else if (spriteRects.Length == 1 && (spriteImportModeToUse == SpriteImportMode.Single || spriteImportModeToUse == SpriteImportMode.Polygon))
            {
                if (spriteImportData[0].spriteID == spriteRects[0].spriteID)
                {
                    spriteImportData[0].name = spriteRects[0].name;
                    spriteImportData[0].alignment = spriteRects[0].alignment;
                    m_TextureImporterSettings.spriteAlignment = (int)spriteRects[0].alignment;
                    m_TextureImporterSettings.spriteBorder = spriteImportData[0].border = spriteRects[0].border;
                    m_TextureImporterSettings.spritePivot = spriteImportData[0].pivot = spriteRects[0].pivot;
                    spriteImportData[0].rect = spriteRects[0].rect;
                }
                else
                {
                    spriteImportData[0] = new SpriteMetaData(spriteRects[0]);
                }
            }
        }
    }
}
