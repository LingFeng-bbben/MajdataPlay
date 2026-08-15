using MajdataPlay.IO;
using MajdataPlay.Settings;
using MajdataPlay.Recording;
using Semver;
using System.Runtime.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    /// <summary>
    /// This class holds a reference to the only instance of a type.
    /// </summary>
    internal static class MajInstances
    {
#if UNITY_EDITOR || DEBUG
        public static SemVersion GameVersion { get; } = SemVersion.Parse("0.1.51", SemVersionStyles.Strict);
#else
        public static SemVersion GameVersion { get; } = SemVersion.Parse(Application.version,SemVersionStyles.Strict);
#endif
        public static GameManager GameManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<GameManager>.Instance!;
        }
        public static AudioManager AudioManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<AudioManager>.Instance!;
        }
        public static SkinManager SkinManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<SkinManager>.Instance!;
        }
        public static SceneSwitcher SceneSwitcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<SceneSwitcher>.Instance!;
        }
        public static BackgroundVideoController BackgroundVideo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<BackgroundVideoController>.Instance!;
        }
        internal static GameUpdater GameUpdater
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<GameUpdater>.Instance!;
        }
        internal static RuntimeInfoDisplayer RuntimeInfoDisplayer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Majdata<RuntimeInfoDisplayer>.Instance!;
        }
    }
}
