using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Rendering
{
    // Shared GPU resources are immutable while in use. Keep references across
    // OnDisable so pooled notes can reuse them, and release on replacement/destroy.
    internal static class RawSpriteResources
    {
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly Dictionary<(Sprite, bool, bool), MeshEntry> Meshes = new();
        static readonly Dictionary<(Material, Texture), MaterialEntry> Materials = new();

        internal sealed class MeshEntry
        {
            internal readonly (Sprite, bool, bool) Key;
            internal readonly Mesh Mesh;
            internal int References;

            internal MeshEntry((Sprite, bool, bool) key, Mesh mesh)
            {
                Key = key;
                Mesh = mesh;
            }
        }

        internal sealed class MaterialEntry
        {
            internal readonly (Material, Texture) Key;
            internal readonly Material Material;
            internal int References;

            internal MaterialEntry((Material, Texture) key, Material material)
            {
                Key = key;
                Material = material;
            }
        }

        internal static MeshEntry? AcquireMesh(Sprite? sprite, bool flipX, bool flipY)
        {
            if (sprite == null)
                return null;

            var key = (sprite, flipX, flipY);
            if (!Meshes.TryGetValue(key, out var entry))
            {
                var sourceVertices = sprite.vertices;
                var sourceIndices = sprite.triangles;
                var vertices = new Vector3[sourceVertices.Length];
                var colors = new Color32[sourceVertices.Length];
                var indices = new int[sourceIndices.Length];
                var scale = new Vector3(flipX ? -1f : 1f, flipY ? -1f : 1f, 1f);

                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = Vector3.Scale(sourceVertices[i], scale);
                    colors[i] = new Color32(255, 255, 255, 255);
                }
                for (var i = 0; i < indices.Length; i++)
                    indices[i] = sourceIndices[i];

                var bounds = sprite.bounds;
                bounds.center = Vector3.Scale(bounds.center, scale);
                var mesh = new Mesh
                {
                    name = $"RawSpriteRenderer {sprite.name} ({flipX}, {flipY})",
                    hideFlags = HideFlags.HideAndDontSave,
                    vertices = vertices,
                    uv = sprite.uv,
                    colors32 = colors,
                    triangles = indices,
                    bounds = bounds
                };
                entry = new MeshEntry(key, mesh);
                Meshes.Add(key, entry);
            }

            entry.References++;
            return entry;
        }

        internal static MaterialEntry? AcquireMaterial(Material? source, Texture? texture)
        {
            if (source == null || texture == null)
                return null;

            var key = (source, texture);
            if (!Materials.TryGetValue(key, out var entry))
            {
                var material = new Material(source)
                {
                    name = $"{source.name} ({texture.name})",
                    hideFlags = HideFlags.HideAndDontSave
                };
                material.SetTexture(MainTexId, texture);
                entry = new MaterialEntry(key, material);
                Materials.Add(key, entry);
            }

            entry.References++;
            return entry;
        }

        internal static void Release(MeshEntry? entry)
        {
            if (entry == null || --entry.References != 0)
                return;

            Meshes.Remove(entry.Key);
            DestroyResource(entry.Mesh);
        }

        internal static void Release(MaterialEntry? entry)
        {
            if (entry == null || --entry.References != 0)
                return;

            Materials.Remove(entry.Key);
            DestroyResource(entry.Material);
        }

        static void DestroyResource(Object resource)
        {
            if (Application.isPlaying)
                Object.Destroy(resource);
            else
                Object.DestroyImmediate(resource);
        }
    }
}
