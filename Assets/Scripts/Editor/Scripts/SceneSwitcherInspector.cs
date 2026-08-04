using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MajdataPlay.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(SceneSwitcher))]
    public sealed class SceneSwitcherInspector : Editor
    {
        double _repaintUntil;
        bool _isPreviewing;
        bool _previewClosedState;

        void OnEnable()
        {
            EditorApplication.update += RepaintPreview;
        }

        void OnDisable()
        {
            EditorApplication.update -= RepaintPreview;
            FinishPreview();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transition Preview", EditorStyles.boldLabel);

            DrawPreviewRow(
                "Triangle Fold",
                "Close：从外圈向圆心展开。Open：从圆心向外圈折回。");

            EditorGUILayout.HelpBox(
                "Triangle Fold is the runtime scene transition.",
                MessageType.Info);
        }

        void DrawPreviewRow(
            string label,
            string tooltip)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    new GUIContent(label, tooltip),
                    GUILayout.Width(150f));
                if (GUILayout.Button("Close"))
                {
                    Preview(true);
                }
                if (GUILayout.Button("Open"))
                {
                    Preview(false);
                }
            }
        }

        void Preview(bool close)
        {
            var switcher = (SceneSwitcher)target;
            switcher.PreviewTransition(close);
            BeginRepaint(switcher.GetEditorPreviewDuration(close), close);
        }

        void BeginRepaint(float duration, bool closedState)
        {
            _repaintUntil = EditorApplication.timeSinceStartup + duration + 0.1d;
            _previewClosedState = closedState;
            _isPreviewing = true;
        }

        void RepaintPreview()
        {
            if (!_isPreviewing)
            {
                return;
            }
            if (EditorApplication.timeSinceStartup > _repaintUntil)
            {
                FinishPreview();
                return;
            }

            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
            InternalEditorUtility.RepaintAllViews();
            Repaint();
        }

        void FinishPreview()
        {
            if (!_isPreviewing)
            {
                return;
            }

            _isPreviewing = false;
            if (target != null)
            {
                ((SceneSwitcher)target).FinishEditorPreview(_previewClosedState);
            }
            EditorApplication.QueuePlayerLoopUpdate();
            InternalEditorUtility.RepaintAllViews();
            Repaint();
        }
    }
}
