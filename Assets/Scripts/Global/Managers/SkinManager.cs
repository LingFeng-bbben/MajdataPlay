using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MajdataPlay
{
    internal sealed class SkinManager : MajSingleton
    {
        public CustomSkin SelectedSkin
        {
            get
            {
                return _selectedSkin;
            }
            set
            {
                _tapLines[0] = value.TapLine_Normal;
                _tapLines[1] = value.TapLine_Each;
                _tapLines[2] = value.TapLine_Break;

                _starLines[0] = value.TapLine_Slide;
                _starLines[1] = value.TapLine_Each;
                _starLines[2] = value.TapLine_Break;

                _holdEnds[0] = value.HoldEndPoint_Normal;
                _holdEnds[1] = value.HoldEndPoint_Each;
                _holdEnds[2] = value.HoldEndPoint_Break;

                _touchHoldFans[0] = value.TouchHold[0];
                _touchHoldFans[1] = value.TouchHold[1];
                _touchHoldFans[2] = value.TouchHold[2];
                _touchHoldFans[3] = value.TouchHold[3];

                _touchHoldBreakFans[0] = value.TouchHold_Break[0];
                _touchHoldBreakFans[1] = value.TouchHold_Break[1];
                _touchHoldBreakFans[2] = value.TouchHold_Break[2];
                _touchHoldBreakFans[3] = value.TouchHold_Break[3];
                _selectedSkin = value;
            }
        }
        public CustomSkin[] LoadedSkins => loadedSkins.ToArray();
        List<CustomSkin> loadedSkins = new();

        CustomSkin _selectedSkin;
        public Texture2D test;

        readonly Sprite[] _tapLines = new Sprite[3];
        readonly Sprite[] _starLines = new Sprite[3];
        readonly Sprite[] _holdEnds = new Sprite[3];
        readonly Sprite[] _touchHoldFans = new Sprite[4];
        readonly Sprite[] _touchHoldBreakFans = new Sprite[4];

        readonly ReadOnlyMemory<Color> _tapAndHoldExEffects = new Color[3]
        {
            new Color(255 / 255f,172 / 255f,225 / 255f), // Pink
            new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
            new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
        };

        protected override void Awake()
        {
            base.Awake();

            var path = MajEnv.SkinPath;
            var selectedSkinName = MajInstances.Settings.Display.Skin;
            var dicts = Directory.GetDirectories(path);

            foreach (var skinPath in dicts)
                loadedSkins.Add(new CustomSkin(skinPath));

            var targetSkin = loadedSkins.Find(x => x.Name == selectedSkinName);
            if (targetSkin is null)
                targetSkin = new CustomSkin(Path.Combine(path, selectedSkinName));

            SelectedSkin = targetSkin;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JudgeTextSkin GetJudgeTextSkin()
        {
            return new()
            {
                CP_Shine = SelectedSkin.CriticalPerfect_Shine,
                P_Shine = SelectedSkin.Perfect_Shine,
                Break_2600_Shine = SelectedSkin.Break_2600_Shine,
                Break_2600 = new()
                {
                    Fast = SelectedSkin.Break_2600_Fast,
                    Normal = SelectedSkin.Break_2600,
                    Late = SelectedSkin.Break_2600_Late
                },
                Break_2550 = new()
                {
                    Fast = SelectedSkin.Break_2550_Fast,
                    Normal = SelectedSkin.Break_2550,
                    Late = SelectedSkin.Break_2550_Late
                },
                Break_2500 = new()
                {
                    Fast = SelectedSkin.Break_2500_Fast,
                    Normal = SelectedSkin.Break_2500,
                    Late = SelectedSkin.Break_2500_Late
                },
                Break_2000 = new()
                {
                    Fast = SelectedSkin.Break_2000_Fast,
                    Normal = SelectedSkin.Break_2000,
                    Late = SelectedSkin.Break_2000_Late
                },
                Break_1500 = new()
                {
                    Fast = SelectedSkin.Break_1500_Fast,
                    Normal = SelectedSkin.Break_1500,
                    Late = SelectedSkin.Break_1500_Late
                },
                Break_1250 = new()
                {
                    Fast = SelectedSkin.Break_1250_Fast,
                    Normal = SelectedSkin.Break_1250,
                    Late = SelectedSkin.Break_1250_Late
                },
                Break_1000 = new()
                {
                    Fast = SelectedSkin.Break_1000_Fast,
                    Normal = SelectedSkin.Break_1000,
                    Late = SelectedSkin.Break_1000_Late
                },
                Break_0 = SelectedSkin.Break_0,
                CriticalPerfect = new()
                {
                    Fast = SelectedSkin.CriticalPerfect_Fast,
                    Normal = SelectedSkin.CriticalPerfect,
                    Late = SelectedSkin.CriticalPerfect_Late
                },
                Perfect = new()
                {
                    Fast = SelectedSkin.Perfect_Fast,
                    Normal = SelectedSkin.Perfect,
                    Late = SelectedSkin.Perfect_Late
                },
                Great = new()
                {
                    Fast = SelectedSkin.Great_Fast,
                    Normal = SelectedSkin.Great,
                    Late = SelectedSkin.Great_Late
                },
                Good = new()
                {
                    Fast = SelectedSkin.Good_Fast,
                    Normal = SelectedSkin.Good,
                    Late = SelectedSkin.Good_Late
                },
                Miss = SelectedSkin.Miss,

                Fast = SelectedSkin.Fast,
                Late = SelectedSkin.Late
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TapSkin GetTapSkin()
        {
            return new()
            {
                Normal = SelectedSkin.Tap,
                Each = SelectedSkin.Tap_Each,
                Break = SelectedSkin.Tap_Break,
                Ex = SelectedSkin.Tap_Ex,

                GuideLines = _tapLines,
                ExEffects = _tapAndHoldExEffects.Span
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StarSkin GetStarSkin()
        {
            return new()
            {
                Normal = SelectedSkin.Star,
                Double = SelectedSkin.Star_Double,
                Each = SelectedSkin.Star_Each,
                EachDouble = SelectedSkin.Star_Each_Double,
                Break = SelectedSkin.Star_Break,
                BreakDouble = SelectedSkin.Star_Break_Double,
                Ex = SelectedSkin.Star_Ex,
                ExDouble = SelectedSkin.Star_Ex_Double,

                GuideLines = _starLines,
                ExEffects = _tapAndHoldExEffects.Span
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HoldSkin GetHoldSkin()
        {
            return new()
            {
                Normal = SelectedSkin.Hold,
                Off = SelectedSkin.Hold_Off,
                Normal_On = SelectedSkin.Hold_On,
                Each = SelectedSkin.Hold_Each,
                Each_On = SelectedSkin.Hold_Each_On,
                Break = SelectedSkin.Hold_Break,
                Break_On = SelectedSkin.Hold_Break_On,
                Ex = SelectedSkin.Hold_Ex,

                GuideLines = _tapLines,
                Ends = _holdEnds,
                ExEffects = _tapAndHoldExEffects.Span
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SlideSkin GetSlideSkin()
        {
            return new SlideSkin()
            {
                Star = GetStarSkin(),
                Normal = SelectedSkin.Slide,
                Each = SelectedSkin.Slide_Each,
                Break = SelectedSkin.Slide_Break,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WifiSkin GetWifiSkin()
        {
            return new WifiSkin()
            {
                Star = GetStarSkin(),
                Normal = SelectedSkin.Wifi,
                Each = SelectedSkin.Wifi_Each,
                Break = SelectedSkin.Wifi_Break,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TouchHoldSkin GetTouchHoldSkin()
        {
            return new TouchHoldSkin()
            {
                Fans = _touchHoldFans,
                Fans_Break = _touchHoldBreakFans,
                Boader = SelectedSkin.TouchHold[4],
                Boader_Break = SelectedSkin.TouchHold_Break[4],
                Point = SelectedSkin.TouchPoint,
                Point_Break = SelectedSkin.TouchPoint_Break,
                Off = SelectedSkin.TouchHold_Off,
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TouchSkin GetTouchSkin()
        {
            return new TouchSkin()
            {
                Normal = SelectedSkin.Touch,
                Each = SelectedSkin.Touch_Each,
                Break = SelectedSkin.Touch_Break,
                Point_Normal = SelectedSkin.TouchPoint,
                Point_Each = SelectedSkin.TouchPoint_Each,
                Point_Break = SelectedSkin.TouchPoint_Break,
                Border_Each = SelectedSkin.TouchBorder_Each,
                Border_Normal = SelectedSkin.TouchBorder,
                Border_Break = SelectedSkin.TouchBorder_Break,
                JustBorder = SelectedSkin.TouchJust
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EachLineSkin GetEachLineSkin()
        {
            return new EachLineSkin()
            {
                EachGuideLines = SelectedSkin.EachLines
            };
        }
    }
}