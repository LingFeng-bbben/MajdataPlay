using MajdataPlay.IO;
using MajdataPlay.Types;
using Semver;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class MajInstances
    {
        public static SemVersion GameVersion { get; } = SemVersion.Parse(Application.version,SemVersionStyles.Strict);
        public static GameManager GameManager
        {
            get => MajInstanceHelper<GameManager>.Instance!;
            set => MajInstanceHelper<GameManager>.Instance = value;
        }
        public static GameSetting Setting
        {
            get => MajInstanceHelper<GameSetting>.Instance!;
            set => MajInstanceHelper<GameSetting>.Instance = value;
        }
        public static AudioManager AudioManager
        {
            get => MajInstanceHelper<AudioManager>.Instance!;
            set => MajInstanceHelper<AudioManager>.Instance = value;
        }
        public static InputManager InputManager
        {
            get => MajInstanceHelper<InputManager>.Instance!;
            set => MajInstanceHelper<InputManager>.Instance = value;
        }
        public static ScoreManager ScoreManager
        {
            get => MajInstanceHelper<ScoreManager>.Instance!;
            set => MajInstanceHelper<ScoreManager>.Instance = value;

        }
        public static SkinManager SkinManager
        {
            get => MajInstanceHelper<SkinManager>.Instance!;
            set => MajInstanceHelper<SkinManager>.Instance = value;
        }
        public static SceneSwitcher SceneSwitcher
        {
            get => MajInstanceHelper<SceneSwitcher>.Instance!;
            set => MajInstanceHelper<SceneSwitcher>.Instance = value;
        }
        public static LightManager LightManager
        {
            get => MajInstanceHelper<LightManager>.Instance!;
            set => MajInstanceHelper<LightManager>.Instance = value;
        }
    }
}
