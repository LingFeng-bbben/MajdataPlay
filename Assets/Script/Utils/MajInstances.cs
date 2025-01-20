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
    public static class MajInstances
    {
#if UNITY_EDITOR
        public static SemVersion GameVersion { get; } = SemVersion.Parse("0.1.0", SemVersionStyles.Strict);
#else
        public static SemVersion GameVersion { get; } = SemVersion.Parse(Application.version,SemVersionStyles.Strict);
#endif
        public static GameManager GameManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<GameManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<GameManager>.Instance = value;
        }
        public static GameSetting Setting
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<GameSetting>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<GameSetting>.Instance = value;
        }
        public static AudioManager AudioManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<AudioManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<AudioManager>.Instance = value;
        }
        public static InputManager InputManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<InputManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<InputManager>.Instance = value;
        }
        public static ScoreManager ScoreManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<ScoreManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<ScoreManager>.Instance = value;

        }
        public static SkinManager SkinManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<SkinManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<SkinManager>.Instance = value;
        }
        public static SceneSwitcher SceneSwitcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<SceneSwitcher>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<SceneSwitcher>.Instance = value;
        }
        public static LightManager LightManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<LightManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<LightManager>.Instance = value;
        }
        public static OnlineManager OnlineManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<OnlineManager>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<OnlineManager>.Instance = value;
        }
        internal static GameUpdater GameUpdater
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MajInstanceHelper<GameUpdater>.Instance!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => MajInstanceHelper<GameUpdater>.Instance = value;
        }
    }
}
