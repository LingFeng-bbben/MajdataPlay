using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.U2D.PSD
{
    internal class PSDGameObjectPreviewData : IDisposable
    {
        static int s_SliderHash = "PSDGameObjectPreviewData_Slider".GetHashCode();
        Texture m_Texture;
        bool m_Disposed;
        PreviewRenderUtility m_RenderUtility;
        Rect m_PreviewRect = new Rect();
        Vector2 m_PreviewDir = Vector2.zero;
        GameObject m_PreviewObject;
        string m_PrefabAssetPath;
        Bounds m_RenderableBounds;
        Vector2 m_GameObjectOffset;
        bool m_ShowPivot;
        GameObject m_PivotInstance;
        GameObject m_Root;
        Rect m_DocumentPivot;

        public PSDGameObjectPreviewData(GameObject assetPrefab, bool showPivot, Rect documentPivot)
        {
            m_RenderUtility = new PreviewRenderUtility();
            m_RenderUtility.camera.fieldOfView = 30f;
            m_ShowPivot = showPivot;
            m_Root = new GameObject();
            m_PreviewObject = GameObject.Instantiate(assetPrefab, Vector3.zero, Quaternion.identity);
            m_PreviewObject.transform.parent = m_Root.transform;
            Bounds renderableBounds = GetRenderableBounds(m_PreviewObject);
            float axisScale = Math.Max(renderableBounds.extents.x, m_RenderableBounds.extents.y) * 0.5f;
            GameObject pivotGO = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.unity.2d.psdimporter/Editor/Assets/pivot.fbx");
            m_PivotInstance = GameObject.Instantiate(pivotGO, Vector3.zero, Quaternion.identity);
            m_PivotInstance.transform.localScale = new Vector3(axisScale, axisScale, axisScale);
            m_PivotInstance.transform.parent = m_Root.transform;
            m_PivotInstance.SetActive(m_ShowPivot);
            m_DocumentPivot = documentPivot;
            m_RenderUtility.AddSingleGO(m_Root);
        }

        static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlId = GUIUtility.GetControlID(s_SliderHash, FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case UnityEngine.EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && (double)position.width > 50.0)
                    {
                        GUIUtility.hotControl = controlId;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        break;
                    }

                    break;
                case UnityEngine.EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                        GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case UnityEngine.EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        scrollPosition -= current.delta * (current.shift ? 3f : 1f) / Mathf.Min(position.width, position.height) * 140f;
                        current.Use();
                        GUI.changed = true;
                        break;
                    }
                    break;
            }
            return scrollPosition;
        }

        public void DrawPreview(Rect r, GUIStyle background, Vector2 offset, bool showPivot)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                EditorGUI.DropShadowLabel(new Rect(r.x, r.y, r.width, 40f), "Preview requires\nrender texture support");
            }
            else
            {

                Vector2 vector2 = Drag2D(m_PreviewDir, r);
                if (vector2 != m_PreviewDir)
                {
                    UnityEngine.Object.DestroyImmediate(m_Texture);
                    m_Texture = null;
                    m_PreviewDir = vector2;
                }


                if (m_GameObjectOffset != offset)
                {
                    UnityEngine.Object.DestroyImmediate(m_Texture);
                    m_Texture = null;
                    m_GameObjectOffset = offset;
                }

                if (m_ShowPivot != showPivot)
                {
                    m_ShowPivot = showPivot;
                    m_PivotInstance.SetActive(m_ShowPivot);
                    UnityEngine.Object.DestroyImmediate(m_Texture);
                    m_Texture = null;
                }

                if (Event.current.type != EventType.Repaint)
                    return;

                if (m_PreviewRect != r)
                {
                    UnityEngine.Object.DestroyImmediate(m_Texture);
                    m_Texture = null;
                    m_PreviewRect = r;
                }

                if (m_Texture == null)
                {
                    m_PreviewObject.transform.position = m_ShowPivot ? new Vector2(-m_DocumentPivot.x, -m_DocumentPivot.y) - m_GameObjectOffset : Vector2.zero;
                    m_RenderUtility.BeginPreview(r, background);
                    DoRenderPreview();
                    m_Texture = m_RenderUtility.EndPreview();
                }

                GUI.DrawTexture(r, m_Texture, ScaleMode.StretchToFill, false);
            }
        }

        void DoRenderPreview()
        {
            m_RenderableBounds = GetRenderableBounds(m_Root);
            float num1 = Mathf.Max(m_RenderableBounds.extents.magnitude, 0.0001f);
            float num2 = num1 * 3.8f;
            Quaternion quaternion = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0.0f);
            Vector3 vector3 = m_RenderableBounds.center - quaternion * (Vector3.forward * num2);
            m_RenderUtility.camera.transform.position = vector3;
            m_RenderUtility.camera.transform.rotation = quaternion;
            m_RenderUtility.camera.nearClipPlane = num2 - num1 * 1.1f;
            m_RenderUtility.camera.farClipPlane = num2 + num1 * 5.1f;
            m_RenderUtility.lights[0].intensity = 0.7f;
            m_RenderUtility.lights[0].transform.rotation = quaternion * Quaternion.Euler(40f, 40f, 0.0f);
            m_RenderUtility.lights[1].intensity = 0.7f;
            m_RenderUtility.lights[1].transform.rotation = quaternion * Quaternion.Euler(340f, 218f, 177f);
            m_RenderUtility.ambientColor = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            if (m_ShowPivot)
            {
                GL.PushMatrix();
                Camera.SetupCurrent(m_RenderUtility.camera);
                GL.LoadProjectionMatrix(m_RenderUtility.camera.projectionMatrix);
                Handles.color = Color.white;

                Vector2 p = (Vector2)m_PreviewObject.transform.position;
                Handles.DrawLine(m_DocumentPivot.min + p, new Vector2(m_DocumentPivot.min.x, m_DocumentPivot.max.y) + p);
                Handles.DrawLine(m_DocumentPivot.min + p, new Vector2(m_DocumentPivot.max.x, m_DocumentPivot.min.y) + p);
                Handles.DrawLine(m_DocumentPivot.max + p, new Vector2(m_DocumentPivot.min.x, m_DocumentPivot.max.y) + p);
                Handles.DrawLine(m_DocumentPivot.max + p, new Vector2(m_DocumentPivot.max.x, m_DocumentPivot.min.y) + p);

                GL.End();
                GL.PopMatrix();
            }

            m_RenderUtility.Render(true);
        }

        public static Bounds GetRenderableBounds(GameObject go)
        {
            Bounds bounds = new Bounds();
            if (go == null)
                return bounds;
            List<Renderer> renderers = new List<Renderer>();
            go.GetComponentsInChildren(renderers);
            foreach (Renderer rendererComponents in renderers)
            {
                if (bounds.extents == Vector3.zero)
                    bounds = rendererComponents.bounds;
                else if (rendererComponents.enabled)
                    bounds.Encapsulate(rendererComponents.bounds);
            }
            return bounds;
        }

        public void Dispose()
        {
            if (m_Disposed)
                return;
            m_RenderUtility.Cleanup();
            UnityEngine.Object.DestroyImmediate(m_PreviewObject);
            m_PreviewObject = null;
            m_Disposed = true;
        }
    }

}
