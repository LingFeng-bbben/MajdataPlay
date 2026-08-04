/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using Live2D.Cubism.Rendering.Masking;
using UnityEditor;
using UnityEngine;


namespace Live2D.Cubism.Editor.Inspectors
{
    /// <summary>
    /// Inspector for <see cref="CubismMaskTexture"/>s.
    /// </summary>
    [CustomEditor(typeof(CubismMaskTexture))]
    internal sealed class CubismMaskTextureInspector : UnityEditor.Editor
    {
        #region Editor

        private bool _foldoutStatus = true;

        /// <summary>
        /// Draws inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var texture = target as CubismMaskTexture;


            // Fail silently.
            if (texture == null)
            {
                return;
            }


            // Show settings.
            EditorGUI.BeginChangeCheck();

            var message = "Current using system: ";
            message += texture.RenderTextureCount < 1 ? "Subdivisions (Legacy)" : "Multiple RenderTexture";
            EditorGUILayout.HelpBox(message, MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Subdivisions (Legacy)", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            texture.Size = EditorGUILayout.IntField("Size (In Pixels)", texture.Size);
            texture.Subdivisions = EditorGUILayout.IntSlider("Subdivisions", texture.Subdivisions, 1, 5);
            EditorGUILayout.ObjectField("Render Texture (Read-only)", (RenderTexture) texture, typeof(RenderTexture), false);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Multiple RenderTexture", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            texture.RenderTextureCount = EditorGUILayout.IntSlider("RenderTextureCount", texture.RenderTextureCount, 0, 5);
            EditorGUILayout.Space();

            _foldoutStatus = EditorGUILayout.Foldout(_foldoutStatus, "Render Textures (Read-only)");
            if (_foldoutStatus)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                // Make it practically ReadOnly.
                GUI.enabled = false;
                for (int renderTextureIndex = 0; renderTextureIndex < texture.RenderTextures.Length; renderTextureIndex++)
                {
                    EditorGUILayout.ObjectField($"element {renderTextureIndex} (Read-only)", texture.RenderTextures[renderTextureIndex], typeof(RenderTexture), false);
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;


            // Save any changes.
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(texture);
            }
        }

        #endregion
    }
}
