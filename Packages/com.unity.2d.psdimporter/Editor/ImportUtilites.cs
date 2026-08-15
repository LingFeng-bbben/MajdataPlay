using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PDNWrapper;
using Unity.Collections;
using UnityEngine;
using UnityEditor.AssetImporters;


#if ENABLE_2D_ANIMATION
using UnityEditor.U2D.Animation;
#endif

namespace UnityEditor.U2D.PSD
{
    class UniqueNameGenerator
    {
        HashSet<int> m_NameHash = new HashSet<int>();

        public bool ContainHash(int i)
        {
            return m_NameHash.Contains(i);
        }

        public void AddHash(int i)
        {
            m_NameHash.Add(i);
        }

        public void AddHash(string name)
        {
            m_NameHash.Add(GetStringHash(name));
        }

        public string GetUniqueName(string name, bool logNewNameGenerated = false, UnityEngine.Object context = null)
        {
            return GetUniqueName(name, m_NameHash);
        }

        static string GetUniqueName(string name, HashSet<int> stringHash, bool logNewNameGenerated = false, UnityEngine.Object context = null)
        {
            string sanitizedName = string.Copy(SanitizeName(name));
            string uniqueName = sanitizedName;
            int index = 1;
            while (true)
            {
                int hash = GetStringHash(uniqueName);
                if (!stringHash.Contains(hash))
                {
                    stringHash.Add(hash);
                    if (logNewNameGenerated && sanitizedName != uniqueName)
                        Debug.Log($"Asset name {name} is changed to {uniqueName} to ensure uniqueness", context);
                    return uniqueName;
                }
                uniqueName = $"{sanitizedName}_{index}";
                ++index;
            }
        }

        static int GetStringHash(string str)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToInt32(hashed, 0);
        }

        public static string SanitizeName(string name)
        {
            name = name.Replace('\0', ' ');
            string newName = null;
            // We can't create asset name with these name.
            if ((name.Length == 2 && name[0] == '.' && name[1] == '.')
                || (name.Length == 1 && name[0] == '.')
                || (name.Length == 1 && name[0] == '/'))
                newName += name + "_";

            if (!string.IsNullOrEmpty(newName))
            {
                Debug.LogWarning($"File contains layer with invalid name for generating asset. {name} is renamed to {newName}");
                return newName;
            }
            return name;
        }
    }

    class GameObjectCreationFactory : UniqueNameGenerator
    {
        public GameObjectCreationFactory(IList<string> names)
        {
            if (names != null)
            {
                foreach (string name in names)
                    GetUniqueName(name);
            }
        }

        public GameObject CreateGameObject(string name, params System.Type[] components)
        {
            string newName = GetUniqueName(name);
            return new GameObject(newName, components);
        }
    }

    internal static class ImportUtilities
    {
        public static string SaveToPng(NativeArray<Color32> buffer, int width, int height)
        {
            if (!buffer.IsCreated ||
                buffer.Length == 0 ||
                width == 0 ||
                height == 0)
                return "No .png generated.";

            Texture2D texture2D = new Texture2D(width, height);
            texture2D.SetPixels32(buffer.ToArray());
            byte[] png = texture2D.EncodeToPNG();
            string path = Application.dataPath + $"/tex_{System.Guid.NewGuid().ToString()}.png";
            System.IO.FileStream fileStream = System.IO.File.Create(path);
            fileStream.Write(png);
            fileStream.Close();

            UnityEngine.Object.DestroyImmediate(texture2D);

            return path;
        }

        public static void ValidatePSDLayerId(IEnumerable<PSDLayer> oldPsdLayer, IEnumerable<BitmapLayer> layers, UniqueNameGenerator uniqueNameGenerator, ScriptedImporter importer)
        {
            // first check if all layers are unique. If not, we use back the previous layer id based on name match
            HashSet<int> uniqueIdSet = new HashSet<int>();
            bool useOldID = false;
            foreach (BitmapLayer layer in layers)
            {
                if (uniqueIdSet.Contains(layer.LayerID))
                {
                    useOldID = true;
                    break;
                }
                uniqueIdSet.Add(layer.LayerID);
            }

            for (int i = 0; i < layers.Count(); ++i)
            {
                BitmapLayer childBitmapLayer = layers.ElementAt(i);
                // fix case 1291323
                if (useOldID)
                {
                    IEnumerable<PSDLayer> oldLayers = oldPsdLayer.Where(x => x.name == childBitmapLayer.Name);
                    if (oldLayers.Count() == 0)
                        oldLayers = oldPsdLayer.Where(x => x.layerID == childBitmapLayer.Name.GetHashCode());
                    // pick one that is not already on the list
                    foreach (PSDLayer ol in oldLayers)
                    {
                        if (!uniqueNameGenerator.ContainHash(ol.layerID))
                        {
                            childBitmapLayer.LayerID = ol.layerID;
                            break;
                        }
                    }
                }

                if (uniqueNameGenerator.ContainHash(childBitmapLayer.LayerID))
                {
                    string layerName = UniqueNameGenerator.SanitizeName(childBitmapLayer.Name);
                    string importWarning = $"Layer {layerName}: LayerId is not unique. Mapping will be done by Layer's name.";
                    layerName = uniqueNameGenerator.GetUniqueName(layerName);
                    if (layerName != childBitmapLayer.Name)
                        importWarning += "\nLayer names are not unique. Please ensure they are unique to for SpriteRect to be mapped back correctly.";
                    childBitmapLayer.LayerID = layerName.GetHashCode();
                    Debug.LogWarning(importWarning, importer);
                }
                else
                    uniqueNameGenerator.AddHash(childBitmapLayer.LayerID);
                if (childBitmapLayer.ChildLayer != null)
                {
                    ValidatePSDLayerId(oldPsdLayer, childBitmapLayer.ChildLayer, uniqueNameGenerator, importer);
                }
            }
        }

        public static void TranslatePivotPoint(Vector2 pivot, Rect rect, out SpriteAlignment alignment, out Vector2 customPivot)
        {
            customPivot = pivot;
            if (new Vector2(rect.xMin, rect.yMax) == pivot)
                alignment = SpriteAlignment.TopLeft;
            else if (new Vector2(rect.center.x, rect.yMax) == pivot)
                alignment = SpriteAlignment.TopCenter;
            else if (new Vector2(rect.xMax, rect.yMax) == pivot)
                alignment = SpriteAlignment.TopRight;
            else if (new Vector2(rect.xMin, rect.center.y) == pivot)
                alignment = SpriteAlignment.LeftCenter;
            else if (new Vector2(rect.center.x, rect.center.y) == pivot)
                alignment = SpriteAlignment.Center;
            else if (new Vector2(rect.xMax, rect.center.y) == pivot)
                alignment = SpriteAlignment.RightCenter;
            else if (new Vector2(rect.xMin, rect.yMin) == pivot)
                alignment = SpriteAlignment.BottomLeft;
            else if (new Vector2(rect.center.x, rect.yMin) == pivot)
                alignment = SpriteAlignment.BottomCenter;
            else if (new Vector2(rect.xMax, rect.yMin) == pivot)
                alignment = SpriteAlignment.BottomRight;
            else
                alignment = SpriteAlignment.Custom;
        }

        public static Vector2 GetPivotPoint(Rect rect, SpriteAlignment alignment, Vector2 customPivot)
        {
            switch (alignment)
            {
                case SpriteAlignment.TopLeft:
                    return new Vector2(rect.xMin, rect.yMax);

                case SpriteAlignment.TopCenter:
                    return new Vector2(rect.center.x, rect.yMax);

                case SpriteAlignment.TopRight:
                    return new Vector2(rect.xMax, rect.yMax);

                case SpriteAlignment.LeftCenter:
                    return new Vector2(rect.xMin, rect.center.y);

                case SpriteAlignment.Center:
                    return new Vector2(rect.center.x, rect.center.y);

                case SpriteAlignment.RightCenter:
                    return new Vector2(rect.xMax, rect.center.y);

                case SpriteAlignment.BottomLeft:
                    return new Vector2(rect.xMin, rect.yMin);

                case SpriteAlignment.BottomCenter:
                    return new Vector2(rect.center.x, rect.yMin);

                case SpriteAlignment.BottomRight:
                    return new Vector2(rect.xMax, rect.yMin);

                case SpriteAlignment.Custom:
                    return new Vector2(customPivot.x * rect.width, customPivot.y * rect.height);
            }
            return Vector2.zero;
        }

        public static string GetUniqueSpriteName(string name, UniqueNameGenerator generator, bool keepDupilcateSpriteName)
        {
            if (keepDupilcateSpriteName)
                return name;
            return generator.GetUniqueName(name);
        }

        public static bool VisibleInHierarchy(List<PSDLayer> psdGroup, int index)
        {
            PSDLayer psdLayer = psdGroup[index];
            bool parentVisible = true;
            if (psdLayer.parentIndex >= 0)
                parentVisible = VisibleInHierarchy(psdGroup, psdLayer.parentIndex);
            return parentVisible && psdLayer.isVisible;
        }

        public static bool IsSpriteMetaDataDefault(SpriteMetaData metaData)
        {
            return metaData.spriteID == default ||
                   metaData.rect == Rect.zero;
        }

#if ENABLE_2D_ANIMATION        
        public static bool SpriteIsMainFromSpriteLib(List<SpriteCategory> categories, string spriteId, out string categoryName)
        {
            categoryName = "";
            if (categories != null)
            {
                foreach (SpriteCategory category in categories)
                {
                    int index = category.labels.FindIndex(x => x.spriteId == spriteId);
                    if (index == 0)
                    {
                        categoryName = category.name;
                        return true;
                    }
                    if (index > 0)
                        return false;
                }
            }
            return true;
        }
#endif
    }
}
