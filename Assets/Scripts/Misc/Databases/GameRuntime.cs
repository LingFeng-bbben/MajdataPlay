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
    public sealed partial class GameRuntime : ScriptableObject
    {
        public static GameRuntime Instance
        {
            get => Majdata<GameRuntime>.Instance!;
        }
        /// <summary>
        /// Colors assigned to each difficulty level.
        /// <para>The array index represents the difficulty identifier.</para>
        /// </summary>
        public ReadOnlySpan<Color> DifficultyColors { get => _difficultyColors; }

        [field: SerializeField]
        public FontAssets LocalizedFonts { get; private set; }

        [field: SerializeField]
        public NoteAssets Note { get; private set; }

        [field: SerializeField]
        public SpriteAssets Sprite { get; private set; }


        [SerializeField]
        [FormerlySerializedAs("difficultyColors")]
        Color[] _difficultyColors;

        public static void Init()
        {
            if (!Majdata<GameRuntime>.IsNull)
            {
                return;
            }
            Majdata<GameRuntime>.SetAsSingleton(Resources.Load<GameRuntime>("Databases/RuntimeDatabase"));
        }
    }
    partial class GameRuntime
    {
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
        [Serializable]
        public class SpriteAssets
        {
            [field: SerializeField]
            public Sprite EmptySongCover { get; private set; }
        }
        [Serializable]
        public class NoteAssets
        {
            [field: SerializeField]
            public Material DefaultMaterial { get; private set; }
            [field: SerializeField]
            public Material BreakMaterial { get; private set; }
            [field: SerializeField]
            public Material HoldShineMaterial { get; private set; }
            [field: SerializeField]
            public NotePrefabAssets Prefab {  get; private set; }
        }
        [Serializable]
        public class NotePrefabAssets
        {
            [field: SerializeField]
            public GameObject SlideStar { get; private set; }
            [field: SerializeField]
            public GameObject SlideArrow { get; private set; }
        }
    }
}
