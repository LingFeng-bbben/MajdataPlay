using System;
using System.Collections.Generic;
using System.Linq;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Settings.SettingItems
{
    public static class SettingDefinitions
    {
        static GameSetting Settings => MajEnv.Settings;

        /// <summary>
        /// 获取所有设置菜单（不包括ChartSetting）
        /// </summary>
        public static IReadOnlyList<SettingMenu> GetAllMenus()
        {
            return new[]
            {
                CreateGameMenu(),
                CreateJudgeMenu(),
                CreateDisplayMenu(),
                CreateAudioVolumeMenu(),
                CreateModMenu(),
                CreateDebugMenu()
            };
        }

        /// <summary>
        /// 创建 Game 菜单
        /// </summary>
        static SettingMenu CreateGameMenu()
        {
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "TapSpeed",
                    () => Settings.Game.TapSpeed,
                    v => Settings.Game.TapSpeed = v,
                    step: 0.25m
                ),
                new NumericSettingItem<float>(
                    "TouchSpeed",
                    () => Settings.Game.TouchSpeed,
                    v => Settings.Game.TouchSpeed = v,
                    step: 0.25m
                ),
                new NumericSettingItem<float>(
                    "SlideFadeInOffset",
                    () => Settings.Game.SlideFadeInOffset,
                    v => Settings.Game.SlideFadeInOffset = v,
                    step: 0.001m,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "BackgroundDim",
                    () => Settings.Game.BackgroundDim,
                    v => Settings.Game.BackgroundDim = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new BoolSettingItem(
                    "StarRotation",
                    () => Settings.Game.StarRotation,
                    v => Settings.Game.StarRotation = v
                ),
                new EnumSettingItem<BGInfoOption>(
                    "BGInfo",
                    () => Settings.Game.BGInfo,
                    v => Settings.Game.BGInfo = v
                ),
                new EnumSettingItem<TopInfoDisplayOption>(
                    "TopInfo",
                    () => Settings.Game.TopInfo,
                    v => Settings.Game.TopInfo = v
                ),
                new BoolSettingItem(
                    "TrackSkip",
                    () => Settings.Game.TrackSkip,
                    v => Settings.Game.TrackSkip = v
                ),
                new BoolSettingItem(
                    "FastRetry",
                    () => Settings.Game.FastRetry,
                    v => Settings.Game.FastRetry = v
                ),
                new EnumSettingItem<MirrorOption>(
                    "Mirror",
                    () => Settings.Game.Mirror,
                    v => Settings.Game.Mirror = v
                ),
                new NumericSettingItem<int>(
                    "Rotation",
                    () => Settings.Game.Rotation,
                    v => Settings.Game.Rotation = v,
                    step: 1m,
                    minValue: -7,
                    maxValue: 7
                ),
                new EnumSettingItem<RandomModeOption>(
                    "Random",
                    () => Settings.Game.Random,
                    v => Settings.Game.Random = v
                ),
                new EnumSettingItem<RecordModeOption>(
                    "RecordMode",
                    () => Settings.Game.RecordMode,
                    v => Settings.Game.RecordMode = v
                )
            };

            return new SettingMenu("Game", items);
        }

        /// <summary>
        /// 创建 Judge 菜单
        /// </summary>
        static SettingMenu CreateJudgeMenu()
        {
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "AudioOffset",
                    () => Settings.Judge.AudioOffset,
                    v => Settings.Judge.AudioOffset = v,
                    step: 0.001m,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "JudgeOffset",
                    () => Settings.Judge.JudgeOffset,
                    v => Settings.Judge.JudgeOffset = v,
                    step: 0.001m,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "AnswerOffset",
                    () => Settings.Judge.AnswerOffset,
                    v => Settings.Judge.AnswerOffset = v,
                    step: 0.001m,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "TouchPanelOffset",
                    () => Settings.Judge.TouchPanelOffset,
                    v => Settings.Judge.TouchPanelOffset = v,
                    step: 0.001m,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new EnumSettingItem<JudgeModeOption>(
                    "Mode",
                    () => Settings.Judge.Mode,
                    v => Settings.Judge.Mode = v
                )
            };

            return new SettingMenu("Judge", items);
        }

        /// <summary>
        /// 创建 Display 菜单
        /// </summary>
        static SettingMenu CreateDisplayMenu()
        {
            var items = new List<ISettingItem>
            {
                new StringOptionSettingItem(
                    "Language",
                    () => Settings.Display.Language,
                    v => {
                        Settings.Display.Language = v;
                        Localization.SetLang(v);
                    },
                    () => {
                        var availableLangs = Localization.Available;
                        if (availableLangs.IsEmpty())
                            return new[] { "Unavailable" };
                        return availableLangs.Select(x => x.ToString()).ToArray();
                    }
                ),
                new StringOptionSettingItem(
                    "Skin",
                    () => Settings.Display.Skin,
                    v => {
                        Settings.Display.Skin = v;
                        var skinManager = MajInstances.SkinManager;
                        var newSkin = skinManager.LoadedSkins.Find(x => x.Name == v);
                        if (newSkin != null)
                            skinManager.SelectedSkin = newSkin;
                    },
                    () => {
                        var skinManager = MajInstances.SkinManager;
                        return skinManager.LoadedSkins.Select(x => x.Name).ToArray();
                    }
                ),
                new BoolSettingItem(
                    "DisplayCriticalPerfect",
                    () => Settings.Display.DisplayCriticalPerfect,
                    v => Settings.Display.DisplayCriticalPerfect = v
                ),
                new BoolSettingItem(
                    "DisplayBreakScore",
                    () => Settings.Display.DisplayBreakScore,
                    v => Settings.Display.DisplayBreakScore = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "FastLateType",
                    () => Settings.Display.FastLateType,
                    v => Settings.Display.FastLateType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "NoteJudgeType",
                    () => Settings.Display.NoteJudgeType,
                    v => Settings.Display.NoteJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "TouchJudgeType",
                    () => Settings.Display.TouchJudgeType,
                    v => Settings.Display.TouchJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "SlideJudgeType",
                    () => Settings.Display.SlideJudgeType,
                    v => Settings.Display.SlideJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "BreakJudgeType",
                    () => Settings.Display.BreakJudgeType,
                    v => Settings.Display.BreakJudgeType = v
                ),
                new EnumSettingItem<JudgeDisplayOption>(
                    "BreakFastLateType",
                    () => Settings.Display.BreakFastLateType,
                    v => Settings.Display.BreakFastLateType = v
                ),
                new EnumSettingItem<JudgeModeOption>(
                    "SlideSortOrder",
                    () => Settings.Display.SlideSortOrder,
                    v => Settings.Display.SlideSortOrder = v
                ),
                new NumericSettingItem<float>(
                    "OuterJudgeDistance",
                    () => Settings.Display.OuterJudgeDistance,
                    v => Settings.Display.OuterJudgeDistance = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new NumericSettingItem<float>(
                    "InnerJudgeDistance",
                    () => Settings.Display.InnerJudgeDistance,
                    v => Settings.Display.InnerJudgeDistance = v,
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new NumericSettingItem<float>(
                    "TapScale",
                    () => Settings.Display.TapScale,
                    v => Settings.Display.TapScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "HoldScale",
                    () => Settings.Display.HoldScale,
                    v => Settings.Display.HoldScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "TouchScale",
                    () => Settings.Display.TouchScale,
                    v => Settings.Display.TouchScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "SlideScale",
                    () => Settings.Display.SlideScale,
                    v => Settings.Display.SlideScale = v,
                    step: 0.01m,
                    minValue: 0,
                    maxValue: 2
                ),
                new EnumSettingItem<TouchFeedbackLevel>(
                    "TouchFeedback",
                    () => Settings.Display.TouchFeedback,
                    v => Settings.Display.TouchFeedback = v
                ),
                new EnumSettingItem<RenderQualityOption>(
                    "RenderQuality",
                    () => Settings.Display.RenderQuality,
                    v => {
                        Settings.Display.RenderQuality = v;
                        QualitySettings.SetQualityLevel((int)v, true);
                    }
                ),
                new NumericSettingItem<int>(
                    "FPSLimit",
                    () => Settings.Display.FPSLimit,
                    v => {
                        Settings.Display.FPSLimit = v;
                        Application.targetFrameRate = v;
                    },
                    step: 1m,
                    minValue: -1
                )
#if !(UNITY_ANDROID || UNITY_IOS)
                ,
                new BoolSettingItem(
                    "VSync",
                    () => Settings.Display.VSync,
                    v => {
                        Settings.Display.VSync = v;
                        QualitySettings.vSyncCount = v ? 1 : 0;
                    }
                )
#endif
            };

            return new SettingMenu("Display", items);
        }

        /// <summary>
        /// 创建 Audio Volume 菜单
        /// </summary>
        static SettingMenu CreateAudioVolumeMenu()
        {
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "Global",
                    () => Settings.Audio.Volume.Global,
                    v => {
                        Settings.Audio.Volume.Global = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 1
                ),
                new NumericSettingItem<float>(
                    "Answer",
                    () => Settings.Audio.Volume.Answer,
                    v => {
                        Settings.Audio.Volume.Answer = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "BGM",
                    () => Settings.Audio.Volume.BGM,
                    v => {
                        Settings.Audio.Volume.BGM = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Track",
                    () => Settings.Audio.Volume.Track,
                    v => {
                        Settings.Audio.Volume.Track = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Tap",
                    () => Settings.Audio.Volume.Tap,
                    v => {
                        Settings.Audio.Volume.Tap = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Slide",
                    () => Settings.Audio.Volume.Slide,
                    v => {
                        Settings.Audio.Volume.Slide = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Break",
                    () => Settings.Audio.Volume.Break,
                    v => {
                        Settings.Audio.Volume.Break = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Touch",
                    () => Settings.Audio.Volume.Touch,
                    v => {
                        Settings.Audio.Volume.Touch = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                ),
                new NumericSettingItem<float>(
                    "Voice",
                    () => Settings.Audio.Volume.Voice,
                    v => {
                        Settings.Audio.Volume.Voice = v;
                        MajInstances.AudioManager.ReadVolumeFromSettings();
                    },
                    step: 0.05m,
                    minValue: 0,
                    maxValue: 2
                )
            };

            return new SettingMenu("Volume", items);
        }

        /// <summary>
        /// 创建 Mod 菜单
        /// </summary>
        static SettingMenu CreateModMenu()
        {
            var items = new List<ISettingItem>
            {
                new NumericSettingItem<float>(
                    "PlaybackSpeed",
                    () => Settings.Mod.PlaybackSpeed,
                    v => Settings.Mod.PlaybackSpeed = v,
                    step: 0.05m,
                    minValue: 0
                ),
                new EnumSettingItem<AutoplayModeOption>(
                    "AutoPlay",
                    () => Settings.Mod.AutoPlay,
                    v => Settings.Mod.AutoPlay = v
                ),
                new EnumSettingItem<JudgeStyleOption>(
                    "JudgeStyle",
                    () => Settings.Mod.JudgeStyle,
                    v => Settings.Mod.JudgeStyle = v
                ),
                new BoolSettingItem(
                    "SubdivideSlideJudgeGrade",
                    () => Settings.Mod.SubdivideSlideJudgeGrade,
                    v => Settings.Mod.SubdivideSlideJudgeGrade = v
                ),
                new BoolSettingItem(
                    "AllBreak",
                    () => Settings.Mod.AllBreak,
                    v => Settings.Mod.AllBreak = v
                ),
                new BoolSettingItem(
                    "AllEx",
                    () => Settings.Mod.AllEx,
                    v => Settings.Mod.AllEx = v
                ),
                new BoolSettingItem(
                    "AllTouch",
                    () => Settings.Mod.AllTouch,
                    v => Settings.Mod.AllTouch = v
                ),
                new BoolSettingItem(
                    "SlideNoHead",
                    () => Settings.Mod.SlideNoHead,
                    v => Settings.Mod.SlideNoHead = v
                ),
                new BoolSettingItem(
                    "SlideNoTrack",
                    () => Settings.Mod.SlideNoTrack,
                    v => Settings.Mod.SlideNoTrack = v
                ),
#if !(UNITY_ANDROID || UNITY_IOS)
                new BoolSettingItem(
                    "ButtonRingForTouch",
                    () => Settings.Mod.ButtonRingForTouch,
                    v => Settings.Mod.ButtonRingForTouch = v
                ),
#endif
                new StringOptionSettingItem(
                    "NoteMask",
                    () => Settings.Mod.NoteMask,
                    v => Settings.Mod.NoteMask = v,
                    () => new[] { "Disable", "Inner", "Outer" }
                )
            };

            return new SettingMenu("Mod", items);
        }

        /// <summary>
        /// 创建 Debug 菜单
        /// </summary>
        static SettingMenu CreateDebugMenu()
        {
            var items = new List<ISettingItem>
            {
                new BoolSettingItem(
                    "DisplaySensor",
                    () => Settings.Debug.DisplaySensor,
                    v => Settings.Debug.DisplaySensor = v
                ),
                new NumericSettingItem<float>(
                    "TouchSimulationRadius",
                    () => Settings.Debug.TouchSimulationRadius,
                    v => Settings.Debug.TouchSimulationRadius = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new NumericSettingItem<float>(
                    "TouchAAreaExtraRadius",
                    () => Settings.Debug.TouchAAreaExtraRadius,
                    v => Settings.Debug.TouchAAreaExtraRadius = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new NumericSettingItem<float>(
                    "TouchBAreaExtraRadius",
                    () => Settings.Debug.TouchBAreaExtraRadius,
                    v => Settings.Debug.TouchBAreaExtraRadius = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new NumericSettingItem<float>(
                    "TouchCAreaExtraRadius",
                    () => Settings.Debug.TouchCAreaExtraRadius,
                    v => Settings.Debug.TouchCAreaExtraRadius = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new NumericSettingItem<float>(
                    "TouchDAreaExtraRadius",
                    () => Settings.Debug.TouchDAreaExtraRadius,
                    v => Settings.Debug.TouchDAreaExtraRadius = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new NumericSettingItem<float>(
                    "TouchEAreaExtraRadius",
                    () => Settings.Debug.TouchEAreaExtraRadius,
                    v => Settings.Debug.TouchEAreaExtraRadius = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new NumericSettingItem<float>(
                    "TouchRadiusAdjust",
                    () => Settings.Debug.TouchRadiusAdjust,
                    v => Settings.Debug.TouchRadiusAdjust = v,
                    step: 0.05m,
                    minValue: -5,
                    maxValue: 5
                ),
                new BoolSettingItem(
                    "DisplayFPS",
                    () => Settings.Debug.DisplayFPS,
                    v => Settings.Debug.DisplayFPS = v
                ),
                new NumericSettingItem<float>(
                    "DisplayOffset",
                    () => Settings.Debug.DisplayOffset,
                    v => Settings.Debug.DisplayOffset = v,
                    step: 0.001m,
                    minValue: 0,
                    stepProvider: () => MajEnv.Settings.Debug.OffsetUnit == OffsetUnitOption.Second ? 0.001m : 0.1m
                ),
                new NumericSettingItem<float>(
                    "NoteAppearRate",
                    () => Settings.Debug.NoteAppearRate,
                    v => Settings.Debug.NoteAppearRate = v,
                    step: 0.001m
                ),
                new EnumSettingItem<OffsetUnitOption>(
                    "OffsetUnit",
                    () => Settings.Debug.OffsetUnit,
                    v => Settings.Debug.OffsetUnit = v
                ),
                new EnumSettingItem<DJAutoPolicyOption>(
                    "DJAutoPolicy",
                    () => Settings.Debug.DJAutoPolicy,
                    v => Settings.Debug.DJAutoPolicy = v
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
