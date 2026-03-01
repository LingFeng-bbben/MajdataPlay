using MajdataPlay.Collections;
using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    /// <summary>
    /// 集中定义所有设置菜单和设置项的静态类
    /// </summary>
    internal static class SettingDefinitions
    {
        /// <summary>
        /// 获取所有设置菜单
        /// </summary>
        internal static IReadOnlyList<SettingMenu> GetAllMenus()
        {
            var settings = MajEnv.Settings;
            return new List<SettingMenu>
            {
                GetGameMenu(),
                GetJudgeMenu(),
                GetDisplayMenu(),
                GetVolumeMenu(),
                GetModMenu(),
                GetDebugMenu()
            };
        }

        /// <summary>
        /// 获取 Game 设置菜单
        /// </summary>
        static SettingMenu GetGameMenu()
        {
            var settings = MajEnv.Settings;
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "TapSpeed",
                    () => settings.Game.TapSpeed,
                    v => settings.Game.TapSpeed = v,
                    step: 0.25m
                ),
                new NumericSettingItem<float>(
                    "TouchSpeed",
                    () => settings.Game.TouchSpeed,
                    v => settings.Game.TouchSpeed = v,
                    step: 0.25m
                ),
                new NumericSettingItem<float>(
                    "SlideFadeInOffset",
                    () => settings.Game.SlideFadeInOffset,
                    v => settings.Game.SlideFadeInOffset = v,
                    step: 0.001m,
                    stepProvider: () => settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "BackgroundDim",
                    () => settings.Game.BackgroundDim,
                    v => settings.Game.BackgroundDim = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new BooleanSettingItem(
                    "StarRotation",
                    () => settings.Game.StarRotation,
                    v => settings.Game.StarRotation = v
                ),
                new EnumSettingItem<BGInfoOption>(
                    "BGInfo",
                    () => settings.Game.BGInfo,
                    v => settings.Game.BGInfo = v
                ),
                new EnumSettingItem<TopInfoDisplayOption>(
                    "TopInfo",
                    () => settings.Game.TopInfo,
                    v => settings.Game.TopInfo = v
                ),
                new BooleanSettingItem(
                    "TrackSkip",
                    () => settings.Game.TrackSkip,
                    v => settings.Game.TrackSkip = v
                ),
                new BooleanSettingItem(
                    "FastRetry",
                    () => settings.Game.FastRetry,
                    v => settings.Game.FastRetry = v
                ),
                new EnumSettingItem<MirrorOption>(
                    "Mirror",
                    () => settings.Game.Mirror,
                    v => settings.Game.Mirror = v
                ),
                new NumericSettingItem<int>(
                    "Rotation",
                    () => settings.Game.Rotation,
                    v => settings.Game.Rotation = v,
                    step: 1,
                    minValue: -7,
                    maxValue: 7
                ),
                new EnumSettingItem<RandomModeOption>(
                    "Random",
                    () => settings.Game.Random,
                    v => settings.Game.Random = v
                ),
                new EnumSettingItem<RecordModeOption>(
                    "RecordMode",
                    () => settings.Game.RecordMode,
                    v => settings.Game.RecordMode = v
                )
            };

            return new SettingMenu("Game", items);
        }

        /// <summary>
        /// 获取 Judge 设置菜单
        /// </summary>
        static SettingMenu GetJudgeMenu()
        {
            var settings = MajEnv.Settings;
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "AudioOffset",
                    () => settings.Judge.AudioOffset,
                    v => settings.Judge.AudioOffset = v,
                    step: 0.001m,
                    stepProvider: () => settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "JudgeOffset",
                    () => settings.Judge.JudgeOffset,
                    v => settings.Judge.JudgeOffset = v,
                    step: 0.001m,
                    stepProvider: () => settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "AnswerOffset",
                    () => settings.Judge.AnswerOffset,
                    v => settings.Judge.AnswerOffset = v,
                    step: 0.001m,
                    stepProvider: () => settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "TouchPanelOffset",
                    () => settings.Judge.TouchPanelOffset,
                    v => settings.Judge.TouchPanelOffset = v,
                    step: 0.001m,
                    stepProvider: () => settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new EnumSettingItem<JudgeModeOption>(
                    "Mode",
                    () => settings.Judge.Mode,
                    v => settings.Judge.Mode = v
                )
            };

            return new SettingMenu("Judge", items);
        }

        /// <summary>
        /// 获取 Display 设置菜单
        /// </summary>
        static SettingMenu GetDisplayMenu()
        {
            var settings = MajEnv.Settings;
            var items = new List<ISettingItem>
            {
                new StringOptionSettingItem(
                    "Language",
                    () => settings.Display.Language,
                    v => {
                        settings.Display.Language = v;
                        Localization.SetLang(v);
                    },
                    () => {
                        var availableLangs = Localization.Available;
                        if (availableLangs.IsEmpty())
                            return new string[] { "Unavailable" };
                        return availableLangs.Select(x => x.ToString()).ToArray();
                    },
                    isReadOnly: Localization.Available.IsEmpty()
                ),
                new StringOptionSettingItem(
                    "Skin",
                    () => settings.Display.Skin,
                    v => {
                        settings.Display.Skin = v;
                        var skinManager = MajInstances.SkinManager;
                        var newSkin = skinManager.LoadedSkins.Find(x => x.Name == v);
                        if (newSkin != null)
                            skinManager.SelectedSkin = newSkin;
                    },
                    () => MajInstances.SkinManager.LoadedSkins.Select(x => x.Name).ToArray()
                ),
                new BooleanSettingItem(
                    "DisplayCriticalPerfect",
                    () => settings.Display.DisplayCriticalPerfect,
                    v => settings.Display.DisplayCriticalPerfect = v
                ),
                new BooleanSettingItem(
                    "DisplayBreakScore",
                    () => settings.Display.DisplayBreakScore,
                    v => settings.Display.DisplayBreakScore = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "FastLateType",
                    () => settings.Display.FastLateType,
                    v => settings.Display.FastLateType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "NoteJudgeType",
                    () => settings.Display.NoteJudgeType,
                    v => settings.Display.NoteJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "TouchJudgeType",
                    () => settings.Display.TouchJudgeType,
                    v => settings.Display.TouchJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "SlideJudgeType",
                    () => settings.Display.SlideJudgeType,
                    v => settings.Display.SlideJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "BreakJudgeType",
                    () => settings.Display.BreakJudgeType,
                    v => settings.Display.BreakJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "BreakFastLateType",
                    () => settings.Display.BreakFastLateType,
                    v => settings.Display.BreakFastLateType = v
                ),
                new EnumSettingItem<JudgeModeOption>(
                    "SlideSortOrder",
                    () => settings.Display.SlideSortOrder,
                    v => settings.Display.SlideSortOrder = v
                ),
                new NumericSettingItem<float>(
                    "OuterJudgeDistance",
                    () => settings.Display.OuterJudgeDistance,
                    v => settings.Display.OuterJudgeDistance = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new NumericSettingItem<float>(
                    "InnerJudgeDistance",
                    () => settings.Display.InnerJudgeDistance,
                    v => settings.Display.InnerJudgeDistance = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new NumericSettingItem<float>(
                    "TapScale",
                    () => settings.Display.TapScale,
                    v => settings.Display.TapScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "HoldScale",
                    () => settings.Display.HoldScale,
                    v => settings.Display.HoldScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "TouchScale",
                    () => settings.Display.TouchScale,
                    v => settings.Display.TouchScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "SlideScale",
                    () => settings.Display.SlideScale,
                    v => settings.Display.SlideScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new EnumSettingItem<TouchFeedbackLevel>(
                    "TouchFeedback",
                    () => settings.Display.TouchFeedback,
                    v => settings.Display.TouchFeedback = v
                ),
                new StringOptionSettingItem(
                    "Resolution",
                    () => settings.Display.Resolution,
                    v => settings.Display.Resolution = v,
                    () => new string[] { settings.Display.Resolution },
                    isReadOnly: true
                ),
                new NumericSettingItem<float>(
                    "MainScreenPosition",
                    () => settings.Display.MainScreenPosition,
                    v => settings.Display.MainScreenPosition = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new EnumSettingItem<RenderQualityOption>(
                    "RenderQuality",
                    () => settings.Display.RenderQuality,
                    v => {
                        settings.Display.RenderQuality = v;
                        UnityEngine.QualitySettings.SetQualityLevel((int)v, true);
                    }
                ),
                new NumericSettingItem<int>(
                    "FPSLimit",
                    () => settings.Display.FPSLimit,
                    v => {
                        settings.Display.FPSLimit = v;
                        UnityEngine.Application.targetFrameRate = v;
                    },
                    step: 1,
                    minValue: -1
                ),
#if !(UNITY_ANDROID || UNITY_IOS)
                new BooleanSettingItem(
                    "VSync",
                    () => settings.Display.VSync,
                    v => {
                        settings.Display.VSync = v;
                        UnityEngine.QualitySettings.vSyncCount = v ? 1 : 0;
                    }
                )
#endif
            };

            return new SettingMenu("Display", items);
        }

        /// <summary>
        /// 获取 Volume 设置菜单
        /// </summary>
        static SettingMenu GetVolumeMenu()
        {
            var settings = MajEnv.Settings;
            var audioManager = MajInstances.AudioManager;
            
            Action<float> volumeChangedCallback = _ => audioManager.ReadVolumeFromSettings();

            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "Global",
                    () => settings.Audio.Volume.Global,
                    v => {
                        settings.Audio.Volume.Global = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new NumericSettingItem<float>(
                    "Answer",
                    () => settings.Audio.Volume.Answer,
                    v => {
                        settings.Audio.Volume.Answer = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "BGM",
                    () => settings.Audio.Volume.BGM,
                    v => {
                        settings.Audio.Volume.BGM = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Track",
                    () => settings.Audio.Volume.Track,
                    v => {
                        settings.Audio.Volume.Track = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Tap",
                    () => settings.Audio.Volume.Tap,
                    v => {
                        settings.Audio.Volume.Tap = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Judge",
                    () => settings.Audio.Volume.Tap,
                    v => {
                        settings.Audio.Volume.Tap = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Slide",
                    () => settings.Audio.Volume.Slide,
                    v => {
                        settings.Audio.Volume.Slide = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Break",
                    () => settings.Audio.Volume.Break,
                    v => {
                        settings.Audio.Volume.Break = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Touch",
                    () => settings.Audio.Volume.Touch,
                    v => {
                        settings.Audio.Volume.Touch = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Voice",
                    () => settings.Audio.Volume.Voice,
                    v => {
                        settings.Audio.Volume.Voice = v;
                        volumeChangedCallback(v);
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                )
            };

            return new SettingMenu("Volume", items);
        }

        /// <summary>
        /// 获取 Mod 设置菜单
        /// </summary>
        static SettingMenu GetModMenu()
        {
            var settings = MajEnv.Settings;
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "PlaybackSpeed",
                    () => settings.Mod.PlaybackSpeed,
                    v => settings.Mod.PlaybackSpeed = v,
                    step: 0.05m,
                    minValue: 0
                ),
                new EnumSettingItem<AutoplayModeOption>(
                    "AutoPlay",
                    () => settings.Mod.AutoPlay,
                    v => settings.Mod.AutoPlay = v
                ),
                new EnumSettingItem<JudgeStyleOption>(
                    "JudgeStyle",
                    () => settings.Mod.JudgeStyle,
                    v => settings.Mod.JudgeStyle = v
                ),
                new BooleanSettingItem(
                    "SubdivideSlideJudgeGrade",
                    () => settings.Mod.SubdivideSlideJudgeGrade,
                    v => settings.Mod.SubdivideSlideJudgeGrade = v
                ),
                new BooleanSettingItem(
                    "AllBreak",
                    () => settings.Mod.AllBreak,
                    v => settings.Mod.AllBreak = v
                ),
                new BooleanSettingItem(
                    "AllEx",
                    () => settings.Mod.AllEx,
                    v => settings.Mod.AllEx = v
                ),
                new BooleanSettingItem(
                    "AllTouch",
                    () => settings.Mod.AllTouch,
                    v => settings.Mod.AllTouch = v
                ),
                new BooleanSettingItem(
                    "SlideNoHead",
                    () => settings.Mod.SlideNoHead,
                    v => settings.Mod.SlideNoHead = v
                ),
                new BooleanSettingItem(
                    "SlideNoTrack",
                    () => settings.Mod.SlideNoTrack,
                    v => settings.Mod.SlideNoTrack = v
                ),
#if !(UNITY_ANDROID || UNITY_IOS)
                new BooleanSettingItem(
                    "ButtonRingForTouch",
                    () => settings.Mod.ButtonRingForTouch,
                    v => settings.Mod.ButtonRingForTouch = v
                ),
#endif
                new StringOptionSettingItem(
                    "NoteMask",
                    () => settings.Mod.NoteMask,
                    v => settings.Mod.NoteMask = v,
                    () => new string[] { "Disable", "Inner", "Outer" }
                )
            };

            return new SettingMenu("Mod", items);
        }

        /// <summary>
        /// 获取 Debug 设置菜单
        /// </summary>
        static SettingMenu GetDebugMenu()
        {
            var settings = MajEnv.Settings;
            var items = new List<ISettingItem>
            {
                new BooleanSettingItem(
                    "DisplaySensor",
                    () => settings.Debug.DisplaySensor,
                    v => settings.Debug.DisplaySensor = v
                ),
                new NumericSettingItem<float>(
                    "TouchSimulationRadius",
                    () => settings.Debug.TouchSimulationRadius,
                    v => settings.Debug.TouchSimulationRadius = v,
                    step: 0.05m
                ),
                new NumericSettingItem<float>(
                    "TouchAAreaExtraRadius",
                    () => settings.Debug.TouchAAreaExtraRadius,
                    v => settings.Debug.TouchAAreaExtraRadius = v,
                    step: 0.05m
                ),
                new NumericSettingItem<float>(
                    "TouchBAreaExtraRadius",
                    () => settings.Debug.TouchBAreaExtraRadius,
                    v => settings.Debug.TouchBAreaExtraRadius = v,
                    step: 0.05m
                ),
                new NumericSettingItem<float>(
                    "TouchCAreaExtraRadius",
                    () => settings.Debug.TouchCAreaExtraRadius,
                    v => settings.Debug.TouchCAreaExtraRadius = v,
                    step: 0.05m
                ),
                new NumericSettingItem<float>(
                    "TouchDAreaExtraRadius",
                    () => settings.Debug.TouchDAreaExtraRadius,
                    v => settings.Debug.TouchDAreaExtraRadius = v,
                    step: 0.05m
                ),
                new NumericSettingItem<float>(
                    "TouchEAreaExtraRadius",
                    () => settings.Debug.TouchEAreaExtraRadius,
                    v => settings.Debug.TouchEAreaExtraRadius = v,
                    step: 0.05m
                ),
                new NumericSettingItem<float>(
                    "TouchRadiusAdjust",
                    () => settings.Debug.TouchRadiusAdjust,
                    v => settings.Debug.TouchRadiusAdjust = v,
                    step: 0.05m
                ),
                new BooleanSettingItem(
                    "DisplayFPS",
                    () => settings.Debug.DisplayFPS,
                    v => settings.Debug.DisplayFPS = v
                ),
                new NumericSettingItem<float>(
                    "DisplayOffset",
                    () => settings.Debug.DisplayOffset,
                    v => settings.Debug.DisplayOffset = v,
                    step: 0.001m,
                    minValue: 0,
                    stepProvider: () => settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "NoteAppearRate",
                    () => settings.Debug.NoteAppearRate,
                    v => settings.Debug.NoteAppearRate = v,
                    step: 0.001m
                ),
                new EnumSettingItem<OffsetUnitOption>(
                    "OffsetUnit",
                    () => settings.Debug.OffsetUnit,
                    v => settings.Debug.OffsetUnit = v
                ),
                new EnumSettingItem<DJAutoPolicyOption>(
                    "DJAutoPolicy",
                    () => settings.Debug.DJAutoPolicy,
                    v => settings.Debug.DJAutoPolicy = v
                )
            };

            return new SettingMenu("Debug", items);
        }

        /// <summary>
        /// 获取 ChartSetting 的设置项列表
        /// </summary>
        internal static IReadOnlyList<ISettingItem> GetChartSettingItems(ChartSetting chartSetting)
        {
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "TrackVolumeOffset",
                    () => chartSetting.TrackVolumeOffset,
                    v => chartSetting.TrackVolumeOffset = v,
                    step: 0.05m,
                    minValue: -2,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "AudioOffset",
                    () => chartSetting.AudioOffset,
                    v => chartSetting.AudioOffset = v,
                    step: 0.001m,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
            };

            return items;
        }
    }
}
