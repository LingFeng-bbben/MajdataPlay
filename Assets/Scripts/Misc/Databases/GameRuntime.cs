using MajdataPlay.i18n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MajdataPlay.Databases
{
    /// <summary>
    /// Stores runtime configuration data used throughout the application.
    /// <para>This asset is created and managed as a ScriptableObject.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "RuntimeDatabase")]
    public sealed class GameRuntime : ScriptableObject
    {
        /// <summary>
        /// Colors assigned to each difficulty level.
        /// <para>The array index represents the difficulty identifier.</para>
        /// </summary>
        public ReadOnlySpan<Color> DifficultyColors { get => _difficultyColors; }
        [field: SerializeField]
        public FontAssets LocalizedFonts { get; private set; }


        [SerializeField]
        [FormerlySerializedAs("difficultyColors")]
        Color[] _difficultyColors;

        [Serializable]
        public class FontAssets
        {
            [FontLCID("zh-CN")]
            [field: SerializeField]
            public TMP_FontAsset SimplifiedChinese { get; private set; }

            [FontLCID("zh-HK")]
            [field: SerializeField]
            public TMP_FontAsset TraditionalChinese { get; private set; }

            [FontLCID("zh-TW")]
            [field: SerializeField]
            public TMP_FontAsset TraditionalChinese_zh_TW { get; private set; }

            [FontLCID("")]
            [field: SerializeField]
            public TMP_FontAsset Default { get; private set; }
        }
    }
}
