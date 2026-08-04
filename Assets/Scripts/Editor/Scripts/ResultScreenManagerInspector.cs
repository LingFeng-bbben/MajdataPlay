using MajdataPlay.Scenes.Result;
using UnityEditor;
using UnityEngine;

namespace MajdataPlay.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ResultScreenManager))]
    public sealed class ResultScreenManagerInspector : Editor
    {
        double _previewRepaintUntil;
        bool _isPreviewing;

        void OnEnable()
        {
            EditorApplication.update += RepaintPreview;
        }

        void OnDisable()
        {
            StopEditModePreview();
            EditorApplication.update -= RepaintPreview;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            if (GUILayout.Button("Play Score Animation"))
            {
                var manager = (ResultScreenManager)target;
                if (!manager.PreviewAchievementRollAnimation())
                {
                    Debug.LogWarning("Unable to preview the score animation: accDX must contain a numeric percentage.");
                    return;
                }

                _previewRepaintUntil = EditorApplication.timeSinceStartup
                                     + ResultScreenManager.RESULT_ANIMATION_DURATION_SEC;
                _isPreviewing = true;
            }
        }

        void RepaintPreview()
        {
            if (!_isPreviewing)
            {
                return;
            }
            if (EditorApplication.timeSinceStartup > _previewRepaintUntil)
            {
                StopEditModePreview();
                return;
            }

            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
            Repaint();
        }

        void StopEditModePreview()
        {
            if (!_isPreviewing)
            {
                return;
            }
            if (!EditorApplication.isPlaying
             && target is ResultScreenManager manager
             && manager != null)
            {
                manager.StopResultAnimationPreview();
            }
            _isPreviewing = false;
        }
    }
}
