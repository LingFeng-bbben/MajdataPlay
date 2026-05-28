using Cysharp.Threading.Tasks;
using MajdataPlay.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public class CustomSkin
    {
        public string Name { get; private set; }
        public bool IsLoaded { get; private set; }
        public bool IsOutlineAvailable { get; private set; } = false;
        public Sprite SubDisplay { get; private set; }

        public Sprite Tap { get; private set; }
        public Sprite Tap_Each { get; private set; }
        public Sprite Tap_Break { get; private set; }
        public Sprite Tap_Ex { get; private set; }
        public Sprite Tap_Mine { get; private set; }
        public Sprite Tap_Break_Mine { get; private set; }

        public Sprite Slide { get; private set; }
        public Sprite Slide_Each { get; private set; }
        public Sprite Slide_Break { get; private set; }
        public Sprite Slide_Mine { get; private set; }
        public Sprite Slide_Break_Mine { get; private set; }
        public Sprite[] Wifi { get; private set; } = new Sprite[11];
        public Sprite[] Wifi_Each { get; private set; } = new Sprite[11];
        public Sprite[] Wifi_Break { get; private set; } = new Sprite[11];
        public Sprite[] Wifi_Mine { get; private set; } = new Sprite[11];
        public Sprite[] Wifi_Break_Mine { get; private set; } = new Sprite[11];


        public Sprite Star { get; private set; }
        
        public Sprite Star_Double { get; private set; }
        public Sprite Star_Each { get; private set; }
        public Sprite Star_Each_Double { get; private set; }
        public Sprite Star_Break { get; private set; }
        public Sprite Star_Break_Double { get; private set; }
        public Sprite Star_Ex { get; private set; }
        public Sprite Star_Ex_Double { get; private set; }
        public Sprite Star_Mine { get; private set; }
        public Sprite Star_Double_Mine { get; private set; }
        public Sprite Star_Break_Mine { get; private set; }
        public Sprite Star_Break_Double_Mine { get; private set; }

        public Sprite Hold { get; private set; }
        public Sprite Hold_On { get; private set; }
        public Sprite Hold_Off { get; private set; }
        public Sprite Hold_Each { get; private set; }
        public Sprite Hold_Each_On { get; private set; }
        public Sprite Hold_Ex { get; private set; }
        public Sprite Hold_Mine { get; private set; }
        public Sprite Hold_Mine_On { get; private set; }
        public Sprite Hold_Break_Mine { get; private set; }
        public Sprite Hold_Break_Mine_On { get; private set; }
        public Sprite Hold_Break { get; private set; }
        public Sprite Hold_Break_On { get; private set; }

        public Sprite[] Just { get; private set; } = new Sprite[60];

        public Sprite CriticalPerfect_Shine { get; private set; }
        public Sprite Perfect_Shine { get; private set; }
        public Sprite Break_2600_Shine { get; private set; }

        public Sprite CriticalPerfect { get; private set; }
        public Sprite Perfect { get; private set; }
        public Sprite Great { get; private set; }
        public Sprite Good { get; private set; }
        public Sprite Miss { get; private set; }

        public Sprite Break_2600 { get; private set; }
        public Sprite Break_2550 { get; private set; }
        public Sprite Break_2500 { get; private set; }
        public Sprite Break_2000 { get; private set; }
        public Sprite Break_1500 { get; private set; }
        public Sprite Break_1250 { get; private set; }
        public Sprite Break_1000 { get; private set; }
        public Sprite Break_0 { get; private set; }
        public Sprite Fast { get; private set; }
        public Sprite Late { get; private set; }

        public Sprite? CriticalPerfect_Fast { get; private set; } = null;
        public Sprite? Perfect_Fast { get; private set; } = null;
        public Sprite? Great_Fast { get; private set; } = null;
        public Sprite? Good_Fast { get; private set; } = null;

        public Sprite? Break_2600_Fast { get; private set; } = null;
        public Sprite? Break_2550_Fast { get; private set; } = null;
        public Sprite? Break_2500_Fast { get; private set; } = null;
        public Sprite? Break_2000_Fast { get; private set; } = null;
        public Sprite? Break_1500_Fast { get; private set; } = null;
        public Sprite? Break_1250_Fast { get; private set; } = null;
        public Sprite? Break_1000_Fast { get; private set; } = null;

        public Sprite? CriticalPerfect_Late { get; private set; } = null;
        public Sprite? Perfect_Late { get; private set; } = null;
        public Sprite? Great_Late { get; private set; } = null;
        public Sprite? Good_Late { get; private set; } = null;

        public Sprite? Break_2600_Late { get; private set; } = null;
        public Sprite? Break_2550_Late { get; private set; } = null;
        public Sprite? Break_2500_Late { get; private set; } = null;
        public Sprite? Break_2000_Late { get; private set; } = null;
        public Sprite? Break_1500_Late { get; private set; } = null;
        public Sprite? Break_1250_Late { get; private set; } = null;
        public Sprite? Break_1000_Late { get; private set; } = null;

        public Sprite Touch { get; private set; }
        public Sprite Touch_Each { get; private set; }
        public Sprite Touch_Break { get; private set; }
        public Sprite Touch_Mine { get; private set; }
        public Sprite Touch_Break_Mine { get; private set; }
        public Sprite TouchPoint { get; private set; }
        public Sprite TouchPoint_Each { get; private set; }
        public Sprite TouchPoint_Break { get; private set; }
        public Sprite TouchPoint_Mine { get; private set; }
        public Sprite TouchPoint_Break_Mine { get; private set; }
        public Sprite TouchJust { get; private set; }
        public Sprite[] TouchBorder { get; private set; } = new Sprite[2];
        public Sprite[] TouchBorder_Each { get; private set; } = new Sprite[2];
        public Sprite[] TouchBorder_Break { get; private set; } = new Sprite[2];
        public Sprite[] TouchBorder_Mine { get; private set; } = new Sprite[2];
        public Sprite[] TouchBorder_Break_Mine { get; private set; } = new Sprite[2];

        public Sprite[] TouchHold { get; private set; } = new Sprite[5];
        public Sprite[] TouchHold_Break { get; private set; } = new Sprite[5];
        public Sprite[] TouchHold_Mine { get; private set; } = new Sprite[5];
        public Sprite[] TouchHold_Break_Mine { get; private set; } = new Sprite[5];
        public Sprite TouchHold_Off { get; private set; }

        public Sprite LoadingSplash { get; private set; }

        public Sprite Outline { get; private set; }

        public Sprite TapLine_Normal { get; private set; }
        public Sprite TapLine_Each { get; private set; }
        public Sprite TapLine_Slide { get; private set; }
        public Sprite TapLine_Break { get; private set; }

        public Sprite[] EachLines { get; private set; } = new Sprite[4];
        public Sprite HoldEndPoint_Normal { get; private set; }
        public Sprite HoldEndPoint_Each { get; private set; }
        public Sprite HoldEndPoint_Break { get; private set; }
        public Sprite HoldEndPoint_Mine { get; private set; }

        public static readonly CustomSkin Empty;
        static readonly Sprite _dummySprite = Sprite.Create(new Texture2D(0, 0), new Rect(0, 0, 0, 0), new Vector2(0.5f, 0.5f));

        bool _canUnload = true;
        readonly string _path;
        readonly SemaphoreSlim _loadSyncLock = new(1, 1);

        static CustomSkin()
        {
            var skin = new CustomSkin
            {
                Name = "NotAvailable",
                IsLoaded = true,
                IsOutlineAvailable = false,

                SubDisplay = _dummySprite,

                Tap = _dummySprite,
                Tap_Each = _dummySprite,
                Tap_Break = _dummySprite,
                Tap_Ex = _dummySprite,

                Slide = _dummySprite,
                Slide_Each = _dummySprite,
                Slide_Break = _dummySprite,

                Star = _dummySprite,
                Star_Double = _dummySprite,
                Star_Each = _dummySprite,
                Star_Each_Double = _dummySprite,
                Star_Break = _dummySprite,
                Star_Break_Double = _dummySprite,
                Star_Ex = _dummySprite,
                Star_Ex_Double = _dummySprite,

                Hold = _dummySprite,
                Hold_On = _dummySprite,
                Hold_Off = _dummySprite,
                Hold_Each = _dummySprite,
                Hold_Each_On = _dummySprite,
                Hold_Ex = _dummySprite,
                Hold_Break = _dummySprite,
                Hold_Break_On = _dummySprite,

                CriticalPerfect_Shine = _dummySprite,
                Perfect_Shine = _dummySprite,
                Break_2600_Shine = _dummySprite,

                CriticalPerfect = _dummySprite,
                Perfect = _dummySprite,
                Great = _dummySprite,
                Good = _dummySprite,
                Miss = _dummySprite,

                Break_2600 = _dummySprite,
                Break_2550 = _dummySprite,
                Break_2500 = _dummySprite,
                Break_2000 = _dummySprite,
                Break_1500 = _dummySprite,
                Break_1250 = _dummySprite,
                Break_1000 = _dummySprite,
                Break_0 = _dummySprite,

                Fast = _dummySprite,
                Late = _dummySprite,

                CriticalPerfect_Fast = _dummySprite,
                Perfect_Fast = _dummySprite,
                Great_Fast = _dummySprite,
                Good_Fast = _dummySprite,

                Break_2600_Fast = _dummySprite,
                Break_2550_Fast = _dummySprite,
                Break_2500_Fast = _dummySprite,
                Break_2000_Fast = _dummySprite,
                Break_1500_Fast = _dummySprite,
                Break_1250_Fast = _dummySprite,
                Break_1000_Fast = _dummySprite,

                CriticalPerfect_Late = _dummySprite,
                Perfect_Late = _dummySprite,
                Great_Late = _dummySprite,
                Good_Late = _dummySprite,

                Break_2600_Late = _dummySprite,
                Break_2550_Late = _dummySprite,
                Break_2500_Late = _dummySprite,
                Break_2000_Late = _dummySprite,
                Break_1500_Late = _dummySprite,
                Break_1250_Late = _dummySprite,
                Break_1000_Late = _dummySprite,

                Touch = _dummySprite,
                Touch_Each = _dummySprite,
                Touch_Break = _dummySprite,
                TouchPoint = _dummySprite,
                TouchPoint_Each = _dummySprite,
                TouchPoint_Break = _dummySprite,
                TouchJust = _dummySprite,

                TouchHold_Off = _dummySprite,

                LoadingSplash = _dummySprite,

                Outline = _dummySprite,

                TapLine_Normal = _dummySprite,
                TapLine_Each = _dummySprite,
                TapLine_Slide = _dummySprite,
                TapLine_Break = _dummySprite,

                HoldEndPoint_Normal = _dummySprite,
                HoldEndPoint_Each = _dummySprite,
                HoldEndPoint_Break = _dummySprite,
            };

            Array.Fill(skin.Wifi, _dummySprite);
            Array.Fill(skin.Wifi_Each, _dummySprite);
            Array.Fill(skin.Wifi_Break, _dummySprite);
            Array.Fill(skin.Just, _dummySprite);

            Array.Fill(skin.TouchBorder, _dummySprite);
            Array.Fill(skin.TouchBorder_Each, _dummySprite);
            Array.Fill(skin.TouchBorder_Break, _dummySprite);

            Array.Fill(skin.TouchHold, _dummySprite);
            Array.Fill(skin.TouchHold_Break, _dummySprite);

            Array.Fill(skin.EachLines, _dummySprite);
            skin._canUnload = false;
            Empty = skin;
        }
        CustomSkin()
        {
            
        }
        public async UniTask LoadAsync(CancellationToken token = default)
        {
            if(IsLoaded)
            {
                return;
            }
            await _loadSyncLock.WaitAsync(token);
            try
            {
                if (IsLoaded)
                {
                    return;
                }
                await UniTask.SwitchToMainThread();
                if (File.Exists(this._path + "/outline.png"))
                {
                    IsOutlineAvailable = true;
                }
                Outline = SpriteLoader.Load(this._path + "/outline.png");
                SubDisplay = SpriteLoader.Load(this._path + "/SubBackgourd.png");

                Tap = SpriteLoader.Load(this._path + "/TapSkins/tap.png");
                Tap_Each = SpriteLoader.Load(this._path + "/TapSkins/tap_each.png");
                Tap_Break = SpriteLoader.Load(this._path + "/TapSkins/tap_break.png");
                Tap_Ex = SpriteLoader.Load(this._path + "/TapSkins/tap_ex.png");

                Slide = SpriteLoader.Load(this._path + "/SlideSkins/slide.png");
                Slide_Each = SpriteLoader.Load(this._path + "/SlideSkins/slide_each.png");
                Slide_Break = SpriteLoader.Load(this._path + "/SlideSkins/slide_break.png");
                for (var i = 0; i < 11; i++)
                {
                    Wifi[i] = SpriteLoader.Load(this._path + "/WifiSkins/wifi_" + i + ".png");
                    Wifi_Each[i] = SpriteLoader.Load(this._path + "/WifiSkins/wifi_each_" + i + ".png");
                    Wifi_Break[i] = SpriteLoader.Load(this._path + "/WifiSkins/wifi_break_" + i + ".png");
                }

                Star = SpriteLoader.Load(this._path + "/StarSkins/star.png");
                Star_Double = SpriteLoader.Load(this._path + "/StarSkins/star_double.png");
                Star_Each = SpriteLoader.Load(this._path + "/StarSkins/star_each.png");
                Star_Each_Double = SpriteLoader.Load(this._path + "/StarSkins/star_each_double.png");
                Star_Break = SpriteLoader.Load(this._path + "/StarSkins/star_break.png");
                Star_Break_Double = SpriteLoader.Load(this._path + "/StarSkins/star_break_double.png");
                Star_Ex = SpriteLoader.Load(this._path + "/StarSkins/star_ex.png");
                Star_Ex_Double = SpriteLoader.Load(this._path + "/StarSkins/star_ex_double.png");

                var border = new Vector4(0, 58, 0, 58);
                Hold = SpriteLoader.Load(this._path + "/HoldSkins/hold.png", border);
                Hold_Each = SpriteLoader.Load(this._path + "/HoldSkins/hold_each.png", border);
                Hold_Each_On = SpriteLoader.Load(this._path + "/HoldSkins/hold_each_on.png", border);
                Hold_Ex = SpriteLoader.Load(this._path + "/HoldSkins/hold_ex.png", border);
                Hold_Break = SpriteLoader.Load(this._path + "/HoldSkins/hold_break.png", border);
                Hold_Break_On = SpriteLoader.Load(this._path + "/HoldSkins/hold_break_on.png", border);

                if (File.Exists(Path.Combine(this._path, "HoldSkins/hold_on.png")))
                {
                    Hold_On = SpriteLoader.Load(this._path + "/HoldSkins/hold_on.png", border);
                }
                else
                {
                    Hold_On = Hold;
                }
                Hold_Off = SpriteLoader.Load(this._path + "/HoldSkins/hold_off.png", border);
                if (File.Exists(Path.Combine(this._path, "HoldSkins/hold_each_on.png")))
                {
                    Hold_Each_On = SpriteLoader.Load(this._path + "/HoldSkins/hold_each_on.png", border);
                }
                else
                {
                    Hold_Each_On = Hold_Each;
                }

                if (File.Exists(Path.Combine(this._path, "HoldSkins/hold_break_on.png")))
                {
                    Hold_Break_On = SpriteLoader.Load(this._path + "/HoldSkins/hold_break_on.png", border);
                }
                else
                {
                    Hold_Break_On = Hold_Break;
                }

                // Critical Perfect

                Just[0] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r.png");
                Just[1] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r.png");
                Just[2] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u.png");
                Just[3] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l.png");
                Just[4] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l.png");
                Just[5] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d.png");

                // Perfect

                Just[6] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_p.png");
                Just[7] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_p.png");
                Just[8] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_p.png");
                Just[9] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_p.png");
                Just[10] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_p.png");
                Just[11] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_p.png");

                // Fast Perfect

                Just[12] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_fast_p.png");
                Just[13] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_fast_p.png");
                Just[14] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_fast_p.png");
                Just[15] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_fast_p.png");
                Just[16] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_fast_p.png");
                Just[17] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_fast_p.png");

                // Fast Great

                Just[18] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_fast_gr.png");
                Just[19] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_fast_gr.png");
                Just[20] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_fast_gr.png");
                Just[21] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_fast_gr.png");
                Just[22] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_fast_gr.png");
                Just[23] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_fast_gr.png");

                // Fast Good

                Just[24] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_fast_gd.png");
                Just[25] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_fast_gd.png");
                Just[26] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_fast_gd.png");
                Just[27] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_fast_gd.png");
                Just[28] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_fast_gd.png");
                Just[29] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_fast_gd.png");

                // Late Perfect

                Just[30] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_late_p.png");
                Just[31] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_late_p.png");
                Just[32] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_late_p.png");
                Just[33] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_late_p.png");
                Just[34] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_late_p.png");
                Just[35] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_late_p.png");

                // Late Great

                Just[36] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_late_gr.png");
                Just[37] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_late_gr.png");
                Just[38] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_late_gr.png");
                Just[39] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_late_gr.png");
                Just[40] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_late_gr.png");
                Just[41] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_late_gr.png");

                // Late Good

                Just[42] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_r_late_gd.png");
                Just[43] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_r_late_gd.png");
                Just[44] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_u_late_gd.png");
                Just[45] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_curv_l_late_gd.png");
                Just[46] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_str_l_late_gd.png");
                Just[47] = SpriteLoader.Load(this._path + "/SlideOKSkins/just_wifi_d_late_gd.png");

                // Miss

                Just[48] = SpriteLoader.Load(this._path + "/SlideOKSkins/miss_curv_r.png");
                Just[49] = SpriteLoader.Load(this._path + "/SlideOKSkins/miss_str_r.png");
                Just[50] = SpriteLoader.Load(this._path + "/SlideOKSkins/miss_wifi_u.png");
                Just[51] = SpriteLoader.Load(this._path + "/SlideOKSkins/miss_curv_l.png");
                Just[52] = SpriteLoader.Load(this._path + "/SlideOKSkins/miss_str_l.png");
                Just[53] = SpriteLoader.Load(this._path + "/SlideOKSkins/miss_wifi_d.png");

                // TooFast
                Just[54] = SpriteLoader.Load(this._path + "/SlideOKSkins/toofast_curv_r.png");
                Just[55] = SpriteLoader.Load(this._path + "/SlideOKSkins/toofast_str_r.png");
                Just[56] = SpriteLoader.Load(this._path + "/SlideOKSkins/toofast_wifi_u.png");
                Just[57] = SpriteLoader.Load(this._path + "/SlideOKSkins/toofast_curv_l.png");
                Just[58] = SpriteLoader.Load(this._path + "/SlideOKSkins/toofast_str_l.png");
                Just[59] = SpriteLoader.Load(this._path + "/SlideOKSkins/toofast_wifi_d.png");

                Miss = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_miss.png");
                Good = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_good.png");
                Great = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_great.png");
                Perfect = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_perfect.png");
                CriticalPerfect = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_cPerfect.png");

                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_cPerfect_fast.png"))
                {
                    CriticalPerfect_Fast = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_cPerfect_fast.png");
                }
                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_cPerfect_late.png"))
                {
                    CriticalPerfect_Late = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_cPerfect_late.png");
                }

                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_perfect_fast.png"))
                {
                    Perfect_Fast = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_perfect_fast.png");
                }
                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_perfect_late.png"))
                {
                    Perfect_Late = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_perfect_late.png");
                }

                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_great_fast.png"))
                {
                    Great_Fast = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_great_fast.png");
                }
                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_great_late.png"))
                {
                    Great_Late = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_great_late.png");
                }

                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_good_fast.png"))
                {
                    Good_Fast = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_good_fast.png");
                }
                if (File.Exists(this._path + "/JudgeTextSkins/judge_text_good_late.png"))
                {
                    Good_Late = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_good_late.png");
                }



                CriticalPerfect_Shine = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_cPerfect_break.png");
                Break_2600_Shine = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_break_2600_shine.png");
                Perfect_Shine = SpriteLoader.Load(this._path + "/JudgeTextSkins/judge_text_perfect_break.png");


                //Break_2600 = SpriteLoader.Load(this._path + "/judge_text_break_2600.png");
                //Break_2550 = SpriteLoader.Load(this._path + "/judge_text_break_2550.png");
                //Break_2500 = SpriteLoader.Load(this._path + "/judge_text_break_2500.png");
                //Break_2000 = SpriteLoader.Load(this._path + "/judge_text_break_2000.png");
                //Break_1500 = SpriteLoader.Load(this._path + "/judge_text_break_1500.png");
                //Break_1250 = SpriteLoader.Load(this._path + "/judge_text_break_1250.png");
                //Break_1000 = SpriteLoader.Load(this._path + "/judge_text_break_1000.png");
                //Break_0 = SpriteLoader.Load(this._path + "/judge_text_break_0.png");


                foreach (var value in new int[] { 2600, 2550, 2500, 2000, 1500, 1250, 1000, 0 })
                {
                    var path = $"{this._path}/JudgeTextSkins/judge_text_break_{value}.png";
                    var _path = $"{this._path}/JudgeTextSkins/judge_text_break_{value}_fast.png";
                    var __path = $"{this._path}/JudgeTextSkins/judge_text_break_{value}_late.png";
                    var type = typeof(CustomSkin);
                    type.GetProperty($"Break_{value}").SetValue(this, SpriteLoader.Load(path));
                    if (value == 0)
                    {
                        continue;
                    }
                    if (File.Exists(_path))
                    {
                        type.GetProperty($"Break_{value}_Fast").SetValue(this, SpriteLoader.Load(_path));
                    }
                    if (File.Exists(__path))
                    {
                        type.GetProperty($"Break_{value}_Late").SetValue(this, SpriteLoader.Load(__path));
                    }
                }

                Fast = SpriteLoader.Load(this._path + "/JudgeTextSkins/fast.png");
                Late = SpriteLoader.Load(this._path + "/JudgeTextSkins/late.png");

                Touch = SpriteLoader.Load(this._path + "/TouchSkins/touch.png");
                Touch_Each = SpriteLoader.Load(this._path + "/TouchSkins/touch_each.png");
                Touch_Break = SpriteLoader.Load(this._path + "/TouchSkins/touch_break.png");
                TouchPoint = SpriteLoader.Load(this._path + "/TouchSkins/touch_point.png");
                TouchPoint_Each = SpriteLoader.Load(this._path + "/TouchSkins/touch_point_each.png");
                TouchPoint_Break = SpriteLoader.Load(this._path + "/TouchSkins/touch_break_point.png");

                TouchJust = SpriteLoader.Load(this._path + "/TouchSkins/touch_just.png");

                TouchBorder[0] = SpriteLoader.Load(this._path + "/TouchSkins/touch_border_2.png");
                TouchBorder[1] = SpriteLoader.Load(this._path + "/TouchSkins/touch_border_3.png");
                TouchBorder_Each[0] = SpriteLoader.Load(this._path + "/TouchSkins/touch_border_2_each.png");
                TouchBorder_Each[1] = SpriteLoader.Load(this._path + "/TouchSkins/touch_border_3_each.png");
                TouchBorder_Break[0] = SpriteLoader.Load(this._path + "/TouchSkins/touch_break_border_2.png");
                TouchBorder_Break[1] = SpriteLoader.Load(this._path + "/TouchSkins/touch_break_border_3.png");

                for (var i = 0; i < 4; i++)
                {
                    TouchHold[i] = SpriteLoader.Load(this._path + "/TouchHoldSkins/touchhold_" + i + ".png");
                    TouchHold_Break[i] = SpriteLoader.Load(this._path + "/TouchHoldSkins/touchhold_break_" + i + ".png");
                }
                TouchHold[4] = SpriteLoader.Load(this._path + "/TouchHoldSkins/touchhold_border.png");
                TouchHold_Break[4] = SpriteLoader.Load(this._path + "/TouchHoldSkins/touchhold_break_border.png");
                TouchHold_Off = SpriteLoader.Load(this._path + "/TouchHoldSkins/touchhold_off.png");

                LoadingSplash = SpriteLoader.Load(this._path + "/now_loading.png");

                TapLine_Normal = SpriteLoader.Load(this._path + "/NoteGuideSkins/Normal.png");
                TapLine_Each = SpriteLoader.Load(this._path + "/NoteGuideSkins/Each.png");
                TapLine_Slide = SpriteLoader.Load(this._path + "/NoteGuideSkins/Slide.png");
                TapLine_Break = SpriteLoader.Load(this._path + "/NoteGuideSkins/Break.png");

                EachLines[0] = SpriteLoader.Load(this._path + "/NoteGuideSkins/EachLine1.png");
                EachLines[1] = SpriteLoader.Load(this._path + "/NoteGuideSkins/EachLine2.png");
                EachLines[2] = SpriteLoader.Load(this._path + "/NoteGuideSkins/EachLine3.png");
                EachLines[3] = SpriteLoader.Load(this._path + "/NoteGuideSkins/EachLine4.png");

                HoldEndPoint_Normal = SpriteLoader.Load(this._path + "/NoteGuideSkins/Hold_End.png");
                HoldEndPoint_Each = SpriteLoader.Load(this._path + "/NoteGuideSkins/Hold_Each_End.png");
                HoldEndPoint_Break = SpriteLoader.Load(this._path + "/NoteGuideSkins/Hold_Break_End.png");

                IsLoaded = true;
            }
            finally
            {
                _loadSyncLock.Release();
            }
        }
        public async UniTask UnloadAsync(CancellationToken token = default)
        {
            void DestroySprite(Sprite? sprite)
            {
                if (sprite != null)
                {
                    var tex = sprite.texture;
                    UnityEngine.Object.DestroyImmediate(sprite, true);
                    UnityEngine.Object.DestroyImmediate(tex, true);
                }
            }

            void DestroyArray(Sprite?[] arr)
            {
                if (arr == null)
                {
                    return;
                }
                for (int i = 0; i < arr.Length; i++)
                {
                    DestroySprite(arr[i]);
                }
            }

            if(!IsLoaded || !_canUnload)
            {
                return;
            }
            await _loadSyncLock.WaitAsync(token);
            try
            {
                await UniTask.SwitchToMainThread();
                DestroySprite(SubDisplay);

                DestroySprite(Tap);
                DestroySprite(Tap_Each);
                DestroySprite(Tap_Break);
                DestroySprite(Tap_Ex);

                DestroySprite(Slide);
                DestroySprite(Slide_Each);
                DestroySprite(Slide_Break);

                DestroyArray(Wifi);
                DestroyArray(Wifi_Each);
                DestroyArray(Wifi_Break);

                DestroySprite(Star);
                DestroySprite(Star_Double);
                DestroySprite(Star_Each);
                DestroySprite(Star_Each_Double);
                DestroySprite(Star_Break);
                DestroySprite(Star_Break_Double);
                DestroySprite(Star_Ex);
                DestroySprite(Star_Ex_Double);

                DestroySprite(Hold);
                DestroySprite(Hold_On);
                DestroySprite(Hold_Off);
                DestroySprite(Hold_Each);
                DestroySprite(Hold_Each_On);
                DestroySprite(Hold_Ex);
                DestroySprite(Hold_Break);
                DestroySprite(Hold_Break_On);

                DestroyArray(Just);

                DestroySprite(CriticalPerfect_Shine);
                DestroySprite(Perfect_Shine);
                DestroySprite(Break_2600_Shine);

                DestroySprite(CriticalPerfect);
                DestroySprite(Perfect);
                DestroySprite(Great);
                DestroySprite(Good);
                DestroySprite(Miss);

                DestroySprite(Break_2600);
                DestroySprite(Break_2550);
                DestroySprite(Break_2500);
                DestroySprite(Break_2000);
                DestroySprite(Break_1500);
                DestroySprite(Break_1250);
                DestroySprite(Break_1000);
                DestroySprite(Break_0);
                DestroySprite(Fast);
                DestroySprite(Late);

                DestroySprite(CriticalPerfect_Fast);
                DestroySprite(Perfect_Fast);
                DestroySprite(Great_Fast);
                DestroySprite(Good_Fast);

                DestroySprite(Break_2600_Fast);
                DestroySprite(Break_2550_Fast);
                DestroySprite(Break_2500_Fast);
                DestroySprite(Break_2000_Fast);
                DestroySprite(Break_1500_Fast);
                DestroySprite(Break_1250_Fast);
                DestroySprite(Break_1000_Fast);

                DestroySprite(CriticalPerfect_Late);
                DestroySprite(Perfect_Late);
                DestroySprite(Great_Late);
                DestroySprite(Good_Late);

                DestroySprite(Break_2600_Late);
                DestroySprite(Break_2550_Late);
                DestroySprite(Break_2500_Late);
                DestroySprite(Break_2000_Late);
                DestroySprite(Break_1500_Late);
                DestroySprite(Break_1250_Late);
                DestroySprite(Break_1000_Late);

                DestroySprite(Touch);
                DestroySprite(Touch_Each);
                DestroySprite(Touch_Break);
                DestroySprite(TouchPoint);
                DestroySprite(TouchPoint_Each);
                DestroySprite(TouchPoint_Break);
                DestroySprite(TouchJust);

                DestroyArray(TouchBorder);
                DestroyArray(TouchBorder_Each);
                DestroyArray(TouchBorder_Break);

                DestroyArray(TouchHold);
                DestroyArray(TouchHold_Break);

                DestroySprite(TouchHold_Off);

                DestroySprite(LoadingSplash);
                DestroySprite(Outline);

                DestroySprite(TapLine_Normal);
                DestroySprite(TapLine_Each);
                DestroySprite(TapLine_Slide);
                DestroySprite(TapLine_Break);

                DestroyArray(EachLines);

                DestroySprite(HoldEndPoint_Normal);
                DestroySprite(HoldEndPoint_Each);
                DestroySprite(HoldEndPoint_Break);

                SubDisplay = null;

                Tap = null;
                Tap_Each = null;
                Tap_Break = null;
                Tap_Ex = null;

                Slide = null;
                Slide_Each = null;
                Slide_Break = null;

                //Wifi = null;
                //Wifi_Each = null;
                //Wifi_Break = null;

                Star = null;
                Star_Double = null;
                Star_Each = null;
                Star_Each_Double = null;
                Star_Break = null;
                Star_Break_Double = null;
                Star_Ex = null;
                Star_Ex_Double = null;

                Hold = null;
                Hold_On = null;
                Hold_Off = null;
                Hold_Each = null;
                Hold_Each_On = null;
                Hold_Ex = null;
                Hold_Break = null;
                Hold_Break_On = null;

                //Just = null;

                CriticalPerfect_Shine = null;
                Perfect_Shine = null;
                Break_2600_Shine = null;

                CriticalPerfect = null;
                Perfect = null;
                Great = null;
                Good = null;
                Miss = null;

                Break_2600 = null;
                Break_2550 = null;
                Break_2500 = null;
                Break_2000 = null;
                Break_1500 = null;
                Break_1250 = null;
                Break_1000 = null;
                Break_0 = null;
                Fast = null;
                Late = null;

                CriticalPerfect_Fast = null;
                Perfect_Fast = null;
                Great_Fast = null;
                Good_Fast = null;

                Break_2600_Fast = null;
                Break_2550_Fast = null;
                Break_2500_Fast = null;
                Break_2000_Fast = null;
                Break_1500_Fast = null;
                Break_1250_Fast = null;
                Break_1000_Fast = null;

                CriticalPerfect_Late = null;
                Perfect_Late = null;
                Great_Late = null;
                Good_Late = null;

                Break_2600_Late = null;
                Break_2550_Late = null;
                Break_2500_Late = null;
                Break_2000_Late = null;
                Break_1500_Late = null;
                Break_1250_Late = null;
                Break_1000_Late = null;

                Touch = null;
                Touch_Each = null;
                Touch_Break = null;
                TouchPoint = null;
                TouchPoint_Each = null;
                TouchPoint_Break = null;
                TouchJust = null;

                //TouchBorder = null;
                //TouchBorder_Each = null;
                //TouchBorder_Break = null;

                //TouchHold = null;
                //TouchHold_Break = null;

                TouchHold_Off = null;

                LoadingSplash = null;

                Outline = null;

                TapLine_Normal = null;
                TapLine_Each = null;
                TapLine_Slide = null;
                TapLine_Break = null;

                //EachLines = null;

                HoldEndPoint_Normal = null;
                HoldEndPoint_Each = null;
                HoldEndPoint_Break = null;

                IsLoaded = false;
            }
            finally
            {
                _loadSyncLock.Release();
            }
        }
        public CustomSkin(string skinCollectionPath, bool loadIntoMemory)
        {
            Name = new DirectoryInfo(skinCollectionPath).Name;
            _path = skinCollectionPath;

            if(loadIntoMemory)
            {
                if (File.Exists(skinCollectionPath + "/outline.png"))
                {
                    IsOutlineAvailable = true;
                }
                Outline = SpriteLoader.Load(skinCollectionPath + "/outline.png");
                SubDisplay = SpriteLoader.Load(skinCollectionPath + "/SubBackgourd.png");

                Tap = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap.png");
                Tap_Each = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_each.png");
                Tap_Break = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_break.png");
                Tap_Break_Mine = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_break_mine.png");
                Tap_Mine = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_mine.png");
                Tap_Ex = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_ex.png");

                Slide = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide.png");
                Slide_Each = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_each.png");
                Slide_Break = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_break.png");
                Slide_Mine = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_mine.png");
                Slide_Break_Mine = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_break_mine.png");
                for (var i = 0; i < 11; i++)
                {
                    Wifi[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_" + i + ".png");
                    Wifi_Each[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_each_" + i + ".png");
                    Wifi_Break[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_break_" + i + ".png");
                    Wifi_Mine[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_mine_" + i + ".png");
                    Wifi_Break_Mine[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_break_mine_" + i + ".png");
                }

                Star = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star.png");
                Star_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_double.png");
                Star_Double_Mine = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_double_mine.png");
                Star_Each = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each.png");
                Star_Mine = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_mine.png");
                Star_Each_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each_double.png");
                Star_Break = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break.png");
                Star_Break_Mine = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break_mine.png");
                Star_Break_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break_double.png");
                Star_Break_Double_Mine = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break_double_mine.png");
                Star_Ex = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex.png");
                Star_Ex_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex_double.png");

                var border = new Vector4(0, 58, 0, 58);
                Hold = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold.png", border);
                Hold_Mine = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_mine.png", border);
                Hold_Each = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each.png", border);
                Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each_on.png", border);
                Hold_Ex = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_ex.png", border);
                Hold_Break = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break.png", border);
                Hold_Break_Mine = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_mine.png", border);
                Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_on.png", border);

                if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_on.png")))
                {
                    Hold_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_on.png", border);
                }
                else
                {
                    Hold_On = Hold;
                }
                Hold_Mine_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_mine_on.png", border);
                Hold_Off = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_off.png", border);
                if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_each_on.png")))
                {
                    Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each_on.png", border);
                }
                else
                {
                    Hold_Each_On = Hold_Each;
                }

                if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_break_on.png")))
                {
                    Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_on.png", border);
                }
                else
                {
                    Hold_Break_On = Hold_Break;
                }
                Hold_Break_Mine_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_mine_on.png", border);

                // Critical Perfect

                Just[0] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r.png");
                Just[1] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r.png");
                Just[2] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u.png");
                Just[3] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l.png");
                Just[4] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l.png");
                Just[5] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d.png");

                // Perfect

                Just[6] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_p.png");
                Just[7] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_p.png");
                Just[8] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_p.png");
                Just[9] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_p.png");
                Just[10] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_p.png");
                Just[11] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_p.png");

                // Fast Perfect

                Just[12] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_p.png");
                Just[13] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_p.png");
                Just[14] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_p.png");
                Just[15] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_p.png");
                Just[16] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_p.png");
                Just[17] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_p.png");

                // Fast Great

                Just[18] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_gr.png");
                Just[19] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_gr.png");
                Just[20] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_gr.png");
                Just[21] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_gr.png");
                Just[22] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_gr.png");
                Just[23] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_gr.png");

                // Fast Good

                Just[24] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_gd.png");
                Just[25] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_gd.png");
                Just[26] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_gd.png");
                Just[27] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_gd.png");
                Just[28] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_gd.png");
                Just[29] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_gd.png");

                // Late Perfect

                Just[30] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_p.png");
                Just[31] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_p.png");
                Just[32] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_p.png");
                Just[33] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_p.png");
                Just[34] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_p.png");
                Just[35] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_p.png");

                // Late Great

                Just[36] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_gr.png");
                Just[37] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_gr.png");
                Just[38] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_gr.png");
                Just[39] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_gr.png");
                Just[40] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_gr.png");
                Just[41] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_gr.png");

                // Late Good

                Just[42] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_gd.png");
                Just[43] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_gd.png");
                Just[44] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_gd.png");
                Just[45] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_gd.png");
                Just[46] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_gd.png");
                Just[47] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_gd.png");

                // Miss

                Just[48] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_curv_r.png");
                Just[49] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_str_r.png");
                Just[50] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_wifi_u.png");
                Just[51] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_curv_l.png");
                Just[52] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_str_l.png");
                Just[53] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_wifi_d.png");

                // TooFast
                Just[54] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_curv_r.png");
                Just[55] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_str_r.png");
                Just[56] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_wifi_u.png");
                Just[57] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_curv_l.png");
                Just[58] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_str_l.png");
                Just[59] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_wifi_d.png");

                Miss = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_miss.png");
                Good = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_good.png");
                Great = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_great.png");
                Perfect = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect.png");
                CriticalPerfect = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect.png");

                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_fast.png"))
                {
                    CriticalPerfect_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_fast.png");
                }
                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_late.png"))
                {
                    CriticalPerfect_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_late.png");
                }

                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_fast.png"))
                {
                    Perfect_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_fast.png");
                }
                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_late.png"))
                {
                    Perfect_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_late.png");
                }

                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_great_fast.png"))
                {
                    Great_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_great_fast.png");
                }
                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_great_late.png"))
                {
                    Great_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_great_late.png");
                }

                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_good_fast.png"))
                {
                    Good_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_good_fast.png");
                }
                if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_good_late.png"))
                {
                    Good_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_good_late.png");
                }



                CriticalPerfect_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_break.png");
                Break_2600_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_break_2600_shine.png");
                Perfect_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_break.png");


                //Break_2600 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_2600.png");
                //Break_2550 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_2550.png");
                //Break_2500 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_2500.png");
                //Break_2000 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_2000.png");
                //Break_1500 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_1500.png");
                //Break_1250 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_1250.png");
                //Break_1000 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_1000.png");
                //Break_0 = SpriteLoader.Load(skinCollectionPath + "/judge_text_break_0.png");


                foreach (var value in new int[] { 2600, 2550, 2500, 2000, 1500, 1250, 1000, 0 })
                {
                    var path = $"{skinCollectionPath}/JudgeTextSkins/judge_text_break_{value}.png";
                    var _path = $"{skinCollectionPath}/JudgeTextSkins/judge_text_break_{value}_fast.png";
                    var __path = $"{skinCollectionPath}/JudgeTextSkins/judge_text_break_{value}_late.png";
                    var type = typeof(CustomSkin);
                    type.GetProperty($"Break_{value}").SetValue(this, SpriteLoader.Load(path));
                    if (value == 0)
                    {
                        continue;
                    }
                    if (File.Exists(_path))
                    {
                        type.GetProperty($"Break_{value}_Fast").SetValue(this, SpriteLoader.Load(_path));
                    }
                    if (File.Exists(__path))
                    {
                        type.GetProperty($"Break_{value}_Late").SetValue(this, SpriteLoader.Load(__path));
                    }
                }

                Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/fast.png");
                Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/late.png");

                Touch = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch.png");
                Touch_Mine = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_mine.png");
                Touch_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_each.png");
                Touch_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break.png");
                Touch_Break_Mine = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_mine.png");
                TouchPoint = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point.png");
                TouchPoint_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point_each.png");
                TouchPoint_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_point.png");
                TouchPoint_Mine = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point_mine.png");
                TouchPoint_Break_Mine = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_point_mine.png");

                TouchJust = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_just.png");

                TouchBorder[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2.png");
                TouchBorder[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3.png");
                TouchBorder_Each[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2_each.png");
                TouchBorder_Each[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3_each.png");
                TouchBorder_Break[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_2.png");
                TouchBorder_Break[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_3.png");
                TouchBorder_Mine[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2_mine.png");
                TouchBorder_Mine[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3_mine.png");
                TouchBorder_Break_Mine[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_2_mine.png");
                TouchBorder_Break_Mine[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_3_mine.png");

                for (var i = 0; i < 4; i++)
                {
                    TouchHold[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_" + i + ".png");
                    TouchHold_Break[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_" + i + ".png");
                    TouchHold_Mine[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_mine_" + i + ".png");
                    TouchHold_Break_Mine[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_mine_" + i + ".png");
                }
                TouchHold[4] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_border.png");
                TouchHold_Break[4] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_border.png");
                TouchHold_Off = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_off.png");

                LoadingSplash = SpriteLoader.Load(skinCollectionPath + "/now_loading.png");

                TapLine_Normal = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Normal.png");
                TapLine_Each = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Each.png");
                TapLine_Slide = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Slide.png");
                TapLine_Break = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Break.png");

                EachLines[0] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine1.png");
                EachLines[1] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine2.png");
                EachLines[2] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine3.png");
                EachLines[3] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine4.png");

                HoldEndPoint_Normal = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_End.png");
                HoldEndPoint_Each = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_Each_End.png");
                HoldEndPoint_Break = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_Break_End.png");

                IsLoaded = true;
            }
        }
        public static CustomSkin Load(string skinCollectionPath)
        {
            var task = LoadAsync(skinCollectionPath);
            task.Wait();
            return task.Result;
        }
        public static CustomSkin Create(string skinCollectionPath)
        {
            var dirInfo = new DirectoryInfo(skinCollectionPath);
            if (!dirInfo.Exists)
            {
                throw new DirectoryNotFoundException($"The skin collection path '{skinCollectionPath}' does not exist.");
            }
            return new CustomSkin(skinCollectionPath, false)
            {
                Name = dirInfo.Name
            };
        }
        public static async Task<CustomSkin> LoadAsync(string skinCollectionPath)
        {
            await UniTask.SwitchToThreadPool();
            var dirInfo = new DirectoryInfo(skinCollectionPath);
            if(!dirInfo.Exists)
            {
                throw new DirectoryNotFoundException($"The skin collection path '{skinCollectionPath}' does not exist.");
            }
            var customSkin = new CustomSkin();

            customSkin.Name = dirInfo.Name;

            if (File.Exists(skinCollectionPath + "/outline.png"))
            {
                customSkin.IsOutlineAvailable = true;
            }
            await UniTask.Yield();
            customSkin.Outline = SpriteLoader.Load(skinCollectionPath + "/outline.png");
            customSkin.SubDisplay = SpriteLoader.Load(skinCollectionPath + "/SubBackgourd.png");

            customSkin.Tap = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap.png");
            customSkin.Tap_Each = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_each.png");
            customSkin.Tap_Break = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_break.png");
            customSkin.Tap_Ex = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_ex.png");

            await UniTask.Yield();
            customSkin.Slide = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide.png");
            customSkin.Slide_Each = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_each.png");
            customSkin.Slide_Break = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_break.png");
            
            for (var i = 0; i < 11; i++)
            {
                customSkin.Wifi[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_" + i + ".png");
                customSkin.Wifi_Each[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_each_" + i + ".png");
                customSkin.Wifi_Break[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_break_" + i + ".png");
                await UniTask.Yield();
            }
            await UniTask.Yield();
            customSkin.Star = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star.png");
            customSkin.Star_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_double.png");
            customSkin.Star_Each = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each.png");
            customSkin.Star_Each_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each_double.png");
            customSkin.Star_Break = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break.png");
            customSkin.Star_Break_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break_double.png");
            customSkin.Star_Ex = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex.png");
            customSkin.Star_Ex_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex_double.png");

            await UniTask.Yield();
            var border = new Vector4(0, 58, 0, 58);
            customSkin.Hold = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold.png", border);
            customSkin.Hold_Each = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each.png", border);
            customSkin.Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each_on.png", border);
            customSkin.Hold_Ex = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_ex.png", border);
            customSkin.Hold_Break = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break.png", border);
            customSkin.Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_on.png", border);
            await UniTask.Yield();

            if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_on.png")))
            {
                customSkin.Hold_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_on.png", border);
            }
            else
            {
                customSkin.Hold_On = customSkin.Hold;
            }
            customSkin.Hold_Off = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_off.png", border);
            if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_each_on.png")))
            {
                customSkin.Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each_on.png", border);
            }
            else
            {
                customSkin.Hold_Each_On = customSkin.Hold_Each;
            }

            if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_break_on.png")))
            {
                customSkin.Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_on.png", border);
            }
            else
            {
                customSkin.Hold_Break_On = customSkin.Hold_Break;
            }

            // Critical Perfect
            await UniTask.Yield();
            customSkin.Just[0] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r.png");
            customSkin.Just[1] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r.png");
            customSkin.Just[2] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u.png");
            customSkin.Just[3] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l.png");
            customSkin.Just[4] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l.png");
            customSkin.Just[5] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d.png");

            // Perfect
            await UniTask.Yield();
            customSkin.Just[6] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_p.png");
            customSkin.Just[7] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_p.png");
            customSkin.Just[8] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_p.png");
            customSkin.Just[9] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_p.png");
            customSkin.Just[10] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_p.png");
            customSkin.Just[11] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_p.png");

            // Fast Perfect
            await UniTask.Yield();
            customSkin.Just[12] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_p.png");
            customSkin.Just[13] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_p.png");
            customSkin.Just[14] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_p.png");
            customSkin.Just[15] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_p.png");
            customSkin.Just[16] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_p.png");
            customSkin.Just[17] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_p.png");

            // Fast Great
            await UniTask.Yield();
            customSkin.Just[18] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_gr.png");
            customSkin.Just[19] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_gr.png");
            customSkin.Just[20] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_gr.png");
            customSkin.Just[21] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_gr.png");
            customSkin.Just[22] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_gr.png");
            customSkin.Just[23] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_gr.png");

            // Fast Good
            await UniTask.Yield();
            customSkin.Just[24] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_gd.png");
            customSkin.Just[25] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_gd.png");
            customSkin.Just[26] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_gd.png");
            customSkin.Just[27] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_gd.png");
            customSkin.Just[28] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_gd.png");
            customSkin.Just[29] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_gd.png");

            // Late Perfect
            await UniTask.Yield();
            customSkin.Just[30] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_p.png");
            customSkin.Just[31] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_p.png");
            customSkin.Just[32] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_p.png");
            customSkin.Just[33] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_p.png");
            customSkin.Just[34] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_p.png");
            customSkin.Just[35] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_p.png");

            // Late Great
            await UniTask.Yield();
            customSkin.Just[36] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_gr.png");
            customSkin.Just[37] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_gr.png");
            customSkin.Just[38] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_gr.png");
            customSkin.Just[39] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_gr.png");
            customSkin.Just[40] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_gr.png");
            customSkin.Just[41] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_gr.png");

            // Late Good
            await UniTask.Yield();
            customSkin.Just[42] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_gd.png");
            customSkin.Just[43] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_gd.png");
            customSkin.Just[44] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_gd.png");
            customSkin.Just[45] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_gd.png");
            customSkin.Just[46] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_gd.png");
            customSkin.Just[47] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_gd.png");

            // Miss
            await UniTask.Yield();
            customSkin.Just[48] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_curv_r.png");
            customSkin.Just[49] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_str_r.png");
            customSkin.Just[50] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_wifi_u.png");
            customSkin.Just[51] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_curv_l.png");
            customSkin.Just[52] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_str_l.png");
            customSkin.Just[53] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_wifi_d.png");

            // TooFast
            await UniTask.Yield();
            customSkin.Just[54] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_curv_r.png");
            customSkin.Just[55] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_str_r.png");
            customSkin.Just[56] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_wifi_u.png");
            customSkin.Just[57] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_curv_l.png");
            customSkin.Just[58] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_str_l.png");
            customSkin.Just[59] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_wifi_d.png");

            await UniTask.Yield();
            customSkin.Miss = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_miss.png");
            customSkin.Good = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_good.png");
            customSkin.Great = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_great.png");
            customSkin.Perfect = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect.png");
            customSkin.CriticalPerfect = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect.png");

            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_fast.png"))
            {
                customSkin.CriticalPerfect_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_fast.png");
            }
            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_late.png"))
            {
                customSkin.CriticalPerfect_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_late.png");
            }

            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_fast.png"))
            {
                customSkin.Perfect_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_fast.png");
            }
            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_late.png"))
            {
                customSkin.Perfect_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_late.png");
            }
            await UniTask.Yield();
            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_great_fast.png"))
            {
                customSkin.Great_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_great_fast.png");
            }
            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_great_late.png"))
            {
                customSkin.Great_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_great_late.png");
            }

            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_good_fast.png"))
            {
                customSkin.Good_Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_good_fast.png");
            }
            if (File.Exists(skinCollectionPath + "/JudgeTextSkins/judge_text_good_late.png"))
            {
                customSkin.Good_Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_good_late.png");
            }


            await UniTask.Yield();
            customSkin.CriticalPerfect_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_break.png");
            customSkin.Break_2600_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_break_2600_shine.png");
            customSkin.Perfect_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_break.png");


            foreach (var value in new int[] { 2600, 2550, 2500, 2000, 1500, 1250, 1000, 0 })
            {
                var path = $"{skinCollectionPath}/JudgeTextSkins/judge_text_break_{value}.png";
                var _path = $"{skinCollectionPath}/JudgeTextSkins/judge_text_break_{value}_fast.png";
                var __path = $"{skinCollectionPath}/JudgeTextSkins/judge_text_break_{value}_late.png";
                var type = typeof(CustomSkin);
                type.GetProperty($"Break_{value}").SetValue(customSkin, SpriteLoader.Load(path));
                if (value == 0)
                {
                    continue;
                }
                if (File.Exists(_path))
                {
                    type.GetProperty($"Break_{value}_Fast").SetValue(customSkin, SpriteLoader.Load(_path));
                }
                if (File.Exists(__path))
                {
                    type.GetProperty($"Break_{value}_Late").SetValue(customSkin, SpriteLoader.Load(__path));
                }
                await UniTask.Yield();
            }

            customSkin.Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/fast.png");
            customSkin.Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/late.png");

            await UniTask.Yield();
            customSkin.Touch = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch.png");
            customSkin.Touch_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_each.png");
            customSkin.Touch_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break.png");
            customSkin.TouchPoint = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point.png");
            customSkin.TouchPoint_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point_each.png");
            customSkin.TouchPoint_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_point.png");

            customSkin.TouchJust = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_just.png");

            await UniTask.Yield();
            customSkin.TouchBorder[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2.png");
            customSkin.TouchBorder[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3.png");
            customSkin.TouchBorder_Each[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2_each.png");
            customSkin.TouchBorder_Each[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3_each.png");
            customSkin.TouchBorder_Break[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_2.png");
            customSkin.TouchBorder_Break[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_3.png");

            for (var i = 0; i < 4; i++)
            {
                customSkin.TouchHold[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_" + i + ".png");
                customSkin.TouchHold_Break[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_" + i + ".png");
                await UniTask.Yield();
            }
            await UniTask.Yield();
            customSkin.TouchHold[4] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_border.png");
            customSkin.TouchHold_Break[4] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_border.png");
            customSkin.TouchHold_Off = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_off.png");

            customSkin.LoadingSplash = SpriteLoader.Load(skinCollectionPath + "/now_loading.png");

            await UniTask.Yield();
            customSkin.TapLine_Normal = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Normal.png");
            customSkin.TapLine_Each = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Each.png");
            customSkin.TapLine_Slide = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Slide.png");
            customSkin.TapLine_Break = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Break.png");

            await UniTask.Yield();
            customSkin.EachLines[0] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine1.png");
            customSkin.EachLines[1] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine2.png");
            customSkin.EachLines[2] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine3.png");
            customSkin.EachLines[3] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine4.png");

            await UniTask.Yield();
            customSkin.HoldEndPoint_Normal = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_End.png");
            customSkin.HoldEndPoint_Each = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_Each_End.png");
            customSkin.HoldEndPoint_Break = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_Break_End.png");

            customSkin.IsLoaded = true;

            return customSkin;
        }
    }
}
