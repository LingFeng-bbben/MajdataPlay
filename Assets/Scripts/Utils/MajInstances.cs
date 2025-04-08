using MajdataPlay.IO;
using MajdataPlay.Types;
using Semver;
using System.Runtime.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    /// <summary>
    /// This class holds a reference to the only instance of a type.
    /// </summary>
    internal static class MajInstances
    {
#if UNITY_EDITOR || DEBUG
        public static SemVersion GameVersion { get; } = SemVersion.Parse("0.1.0", SemVersionStyles.Strict);
#else
        public static SemVersion GameVersion { get; } = SemVersion.Parse(Application.version,SemVersionStyles.Strict);
#endif
        public static GameManager GameManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<GameManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<GameManager>.Instance = value;
        }
        public static GameSetting Settings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajEnv.UserSettings;
        }
        public static AudioManager AudioManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<AudioManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<AudioManager>.Instance = value;
        }
        public static ScoreManager ScoreManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<ScoreManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<ScoreManager>.Instance = value;

        }
        public static SkinManager SkinManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<SkinManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<SkinManager>.Instance = value;
        }
        public static SceneSwitcher SceneSwitcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<SceneSwitcher>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<SceneSwitcher>.Instance = value;
        }
        public static LightManager LightManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<LightManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<LightManager>.Instance = value;
        }
        internal static GameUpdater GameUpdater
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<GameUpdater>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Majdata<GameUpdater>.Instance = value;
        }
    }
}
