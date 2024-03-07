/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace Live2D.Cubism.Rendering.Masking
{
    /// <summary>
    /// Singleton buffer for Cubism mask related draw commands.
    /// </summary>
    [ExecuteInEditMode]
    public sealed class CubismMaskCommandBuffer : MonoBehaviour
    {
        /// <summary>
        /// Draw command sources.
        /// </summary>
        private static List<ICubismMaskCommandSource> Sources { get; set; }

        /// <summary>
        /// Command buffer.
        /// </summary>
        private static CommandBuffer Buffer { get; set; }

        /// <summary>
        /// Command buffers.
        /// </summary>
        private static CommandBuffer[] Buffers { get; set; }

        /// <summary>
        /// True if <see cref="Sources"/> are empty.
        /// </summary>
        private static bool ContainsSources
        {
            get { return Sources != null && Sources.Count > 0; }
        }


        /// <summary>
        /// Makes sure class is initialized for static usage.
        /// </summary>
        private static void Initialize()
        {
            // Initialize containers.
            if (Sources == null)
            {
                Sources = new List<ICubismMaskCommandSource>();
            }


            if (Buffer == null)
            {
                Buffer = new CommandBuffer
                {
                    name = "cubism_MaskCommandBuffer"
                };
            }


            if (Buffers == null)
            {
                Buffers = new CommandBuffer[0];
            }

            // Spawn update proxy.
            const string _proxyName = "cubism_MaskCommandBuffer";
            var proxy = GameObject.Find(_proxyName);


            if (proxy == null)
            {
                proxy = new GameObject(_proxyName)
                {
                     hideFlags = HideFlags.HideAndDontSave
                };


                if (!Application.isEditor || Application.isPlaying)
                {
                    DontDestroyOnLoad(proxy);
                }


                proxy.AddComponent<CubismMaskCommandBuffer>();
            }
        }


        /// <summary>
        /// Registers a new draw command source.
        /// </summary>
        /// <param name="source">Source to add.</param>
        internal static void AddSource(ICubismMaskCommandSource source)
        {
            // Make sure singleton is initialized.
            Initialize();


            // Prevent same source from being added twice.
            if (Sources.Contains(source))
            {
                return;
            }


            // Add source and force refresh.
            Sources.Add(source);

            if (source.CountOfCommandBuffers > Buffers.Length)
            {
                for (int bufferIndex = 0; bufferIndex < Buffers.Length; bufferIndex++)
                {
                    Buffers[bufferIndex].Clear();
                }

                Buffers = new CommandBuffer[source.CountOfCommandBuffers];

                for (int bufferIndex = 0; bufferIndex < Buffers.Length; bufferIndex++)
                {
                    Buffers[bufferIndex] = new CommandBuffer
                    {
                        name = "cubism_MaskCommandBuffer" + bufferIndex
                    };
                }
            }
        }

        /// <summary>
        /// Deregisters a draw command source.
        /// </summary>
        /// <param name="source">Source to remove.</param>
        internal static void RemoveSource(ICubismMaskCommandSource source)
        {
            // Make sure singleton is initialized.
            Initialize();


            // Remove source and force refresh.
            Sources.RemoveAll(s => s == source);
        }


        /// <summary>
        /// Forces the command buffer to be refreshed.
        /// </summary>
        private static void RefreshCommandBuffer()
        {
            // Clear buffer.
            Buffer.Clear();


            // Enqueue sources.
            for (var i = 0; i < Sources.Count; ++i)
            {
                Sources[i].AddToCommandBuffer(Buffer, false, -1);
            }
        }

        /// <summary>
        /// Forces command buffer in <see cref="Buffers"/> refresh and executes it.
        /// </summary>
        private static void RefreshCommandBuffers()
        {
            for (int bufferIndex = 0; bufferIndex < Buffers.Length; bufferIndex++)
            {
                // Clear buffer.
                Buffers[bufferIndex].Clear();

                // Enqueue sources.
                for (var i = 0; i < Sources.Count; ++i)
                {
                    Sources[i].AddToCommandBuffer(Buffers[bufferIndex], true, bufferIndex);
                }

                // Executes buffer.
                Graphics.ExecuteCommandBuffer(Buffers[bufferIndex]);
            }
        }

        #region Unity Event Handling

        /// <summary>
        /// Executes <see cref="Buffer"/> or <see cref="Buffers"/>.
        /// </summary>
        private void LateUpdate()
        {
            if (!ContainsSources)
            {
                return;
            }


            // Refresh and execute buffer.
            RefreshCommandBuffer();
            Graphics.ExecuteCommandBuffer(Buffer);
            RefreshCommandBuffers();
        }

        #endregion
    }
}
