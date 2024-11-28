using MajdataPlay.Types;
using MajdataPlay.Utils;
using MajSimaiDecode;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MajdataPlay
{
    public class SkinManager : MonoBehaviour
    {
        public CustomSkin SelectedSkin { get; set; }
        public CustomSkin[] LoadedSkins => loadedSkins.ToArray();
        List<CustomSkin> loadedSkins = new();

        public Sprite HoldEnd;
        public Sprite HoldEachEnd;
        public Sprite HoldBreakEnd;

        public Texture2D test;

        public Sprite[] TapLines;
        public Sprite[] StarLines;
        public RuntimeAnimatorController JustBreak;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            MajInstances.SkinManager = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            var path = GameManager.SkinPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var selectedSkinName = MajInstances.Setting.Display.Skin;
            var dicts = Directory.GetDirectories(path);

            foreach (var skinPath in dicts)
                loadedSkins.Add(new CustomSkin(skinPath));

            var targetSkin = loadedSkins.Find(x => x.Name == selectedSkinName);
            if (targetSkin is null)
                targetSkin = new CustomSkin(Path.Combine(path, selectedSkinName));

            SelectedSkin = targetSkin;

            print(path);
            Debug.Log(test);
        }
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
        public TapSkin GetTapSkin()
        {
            return new()
            {
                Normal = SelectedSkin.Tap,
                Each = SelectedSkin.Tap_Each,
                Break = SelectedSkin.Tap_Break,
                Ex = SelectedSkin.Tap_Ex,

                NoteLines = TapLines,
                ExEffects = new Color[]
                {
                    new Color(255 / 255f,172 / 255f,225 / 255f), // Pink
                    new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
                    new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
                }
            };
        }
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

                NoteLines = StarLines,
                ExEffects = new Color[]
                {
                    new Color(1,1,1), //White
                    new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
                    new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
                }
            };
        }
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

                NoteLines = TapLines,
                Ends = new Sprite[3]
                {
                    HoldEnd,
                    HoldEachEnd,
                    HoldBreakEnd
                },
                ExEffects = new Color[]
                {
                    new Color(255 / 255f,172 / 255f,225 / 255f), // Pink
                    new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
                    new Color(255 / 255f,254 / 255f,119 / 255f), // Yellow
                }
            };
        }
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
        public TouchHoldSkin GetTouchHoldSkin()
        {
            return new TouchHoldSkin()
            {
                Fans = new Sprite[4]
                {
                    SelectedSkin.TouchHold[0],
                    SelectedSkin.TouchHold[1],
                    SelectedSkin.TouchHold[2],
                    SelectedSkin.TouchHold[3],
                },
                Fans_Break = new Sprite[4]
                {
                    SelectedSkin.TouchHold_Break[0],
                    SelectedSkin.TouchHold_Break[1],
                    SelectedSkin.TouchHold_Break[2],
                    SelectedSkin.TouchHold_Break[3],
                },
                Boader = SelectedSkin.TouchHold[4],
                Boader_Break = SelectedSkin.TouchHold_Break[4],
                Point = SelectedSkin.TouchPoint,
                Point_Break = SelectedSkin.TouchPoint_Break,
                Off = SelectedSkin.TouchHold_Off,
            };
        }
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
    }
}