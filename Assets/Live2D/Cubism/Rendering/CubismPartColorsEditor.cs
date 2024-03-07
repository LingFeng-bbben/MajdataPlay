/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */

using System;
using System.Linq;
using Live2D.Cubism.Core;
using UnityEngine;


namespace Live2D.Cubism.Rendering
{
    [ExecuteInEditMode, RequireComponent(typeof(CubismPart))]
    public class CubismPartColorsEditor : MonoBehaviour
    {
        /// <summary>
        /// RenderController.
        /// </summary>
        private CubismRenderController _renderController;

        /// <summary>
        /// Renderer Array.
        /// </summary>
        private CubismRenderer[] _renderers;

        /// <summary>
        /// Part Array.
        /// </summary>
        private CubismPart _part;

        /// <summary>
        /// <see cref="ChildDrawableRenderers"/>s backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private CubismRenderer[] _childDrawableRenderers;

        /// <summary>
        /// Array of art meshes with parts as parents.
        /// </summary>
        public CubismRenderer[] ChildDrawableRenderers
        {
            get { return _childDrawableRenderers; }
            set { _childDrawableRenderers = value; }
        }

        /// <summary>
        /// <see cref="OverwriteColorForPartMultiplyColors"/>s backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private bool _isOverwrittenPartMultiplyColors;

        /// <summary>
        /// Whether to overwrite with multiply color from the model.
        /// </summary>
        public bool OverwriteColorForPartMultiplyColors
        {
            get { return _isOverwrittenPartMultiplyColors; }
            set {
                _isOverwrittenPartMultiplyColors = value;
                for (int i = 0; i < ChildDrawableRenderers.Length; i++)
                {
                    ChildDrawableRenderers[i].OverwriteFlagForDrawableMultiplyColors = OverwriteColorForPartMultiplyColors;
                    ChildDrawableRenderers[i].LastMultiplyColor = OverwriteColorForPartMultiplyColors ? MultiplyColor : ChildDrawableRenderers[i].LastMultiplyColor;
                    ChildDrawableRenderers[i].MultiplyColor = OverwriteColorForPartMultiplyColors ? MultiplyColor : ChildDrawableRenderers[i].MultiplyColor;
                    ChildDrawableRenderers[i].ApplyMultiplyColor();
                }
            }
        }

        /// <summary>
        /// <see cref="OverwriteColorForPartScreenColors"/>s backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private bool _isOverwrittenPartScreenColors;

        /// <summary>
        /// Whether to overwrite with screen color from the model.
        /// </summary>
        public bool OverwriteColorForPartScreenColors
        {
            get { return _isOverwrittenPartScreenColors; }
            set {
                _isOverwrittenPartScreenColors = value;
                for (int i = 0; i < ChildDrawableRenderers.Length; i++)
                {
                    ChildDrawableRenderers[i].OverwriteFlagForDrawableScreenColors = OverwriteColorForPartScreenColors;
                    ChildDrawableRenderers[i].LastScreenColor = OverwriteColorForPartScreenColors ? ScreenColor : ChildDrawableRenderers[i].LastScreenColor;
                    ChildDrawableRenderers[i].ScreenColor = OverwriteColorForPartScreenColors ? ScreenColor : ChildDrawableRenderers[i].ScreenColor;
                    ChildDrawableRenderers[i].ApplyScreenColor();
                }
            }
        }

        /// <summary>
        /// <see cref="MultiplyColor"/>s backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private Color _multiplyColor = Color.white;

        /// <summary>
        /// Multiply color.
        /// </summary>
        public Color MultiplyColor
        {
            get
            {
                return _multiplyColor;
            }
            set
            {
                // 同じ値が与えられた場合、早めに返す
                if (value == _multiplyColor)
                {
                    return;
                }

                // 値を保存
                _multiplyColor = (value != null)
                    ? value
                    : Color.white;

                for (int i = 0; i < ChildDrawableRenderers.Length; i++)
                {
                    ChildDrawableRenderers[i].MultiplyColor = OverwriteColorForPartMultiplyColors ? MultiplyColor : ChildDrawableRenderers[i].MultiplyColor;
                }
            }
        }

        /// <summary>
        /// <see cref="ScreenColor"/>s backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private Color _screenColor = Color.clear;

        /// <summary>
        /// Screen color.
        /// </summary>
        public Color ScreenColor
        {
            get
            {
                return _screenColor;
            }
            set
            {
                // 同じ値が与えられた場合、早めに返す
                if (value == _screenColor)
                {
                    return;
                }

                // 値を保存
                _screenColor = (value != null)
                    ? value
                    : Color.black;

                for (int i = 0; i < ChildDrawableRenderers.Length; i++)
                {
                    ChildDrawableRenderers[i].ScreenColor = OverwriteColorForPartScreenColors ? ScreenColor : ChildDrawableRenderers[i].ScreenColor;
                }
            }
        }

        public void TryInitialize(CubismRenderController cubismRenderController, CubismPart part, CubismDrawable[] drawables)
        {
            // Initialize.
            _renderController = cubismRenderController;
            _renderers = _renderController.Renderers;
            _part = part;

            // Initialize elements.
            ChildDrawableRenderers = Array.Empty<CubismRenderer>();

            for (var j = 0; j < _renderers.Length; j++)
            {
                // When this object is the parent part.
                if (drawables[j].ParentPartIndex == _part.UnmanagedIndex)
                {
                    // Register the corresponding renderers in the dictionary.
                    Array.Resize(ref _childDrawableRenderers, _childDrawableRenderers.Length + 1);
                    ChildDrawableRenderers[ChildDrawableRenderers.Length - 1] = _renderers[j];
                }
            }
        }

#region Unity Events

        private void OnEnable()
        {
            // Early return.
            if (ChildDrawableRenderers != null && !ChildDrawableRenderers.Contains(null))
            {
                return;
            }

            // Making cash.
            var model = gameObject.FindCubismModel(true);
            var drawables = model.Drawables;

            if (_renderController == null)
            {
                _renderController = model.GetComponent<CubismRenderController>();
                _renderers = _renderController.Renderers;
            }

            if (_part == null)
            {
                _part = GetComponent<CubismPart>();
            }

            // Initialize.
            TryInitialize(_renderController, _part, drawables);
        }

#endregion
    }
}
