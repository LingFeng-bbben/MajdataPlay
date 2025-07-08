using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Skins
{
    public class CustomSkin
    {
        public string Name { get; private set; }
        public bool IsOutlineAvailable { get; private set; } = false;
        public Sprite SubDisplay { get; private set; }

        public Sprite Tap { get; private set; }
        public Sprite Tap_Each { get; private set; }
        public Sprite Tap_Break { get; private set; }
        public Sprite Tap_Ex { get; private set; }

        public Sprite Slide { get; private set; }
        public Sprite Slide_Each { get; private set; }
        public Sprite Slide_Break { get; private set; }
        public Sprite[] Wifi { get; private set; } = new Sprite[11];
        public Sprite[] Wifi_Each { get; private set; } = new Sprite[11];
        public Sprite[] Wifi_Break { get; private set; } = new Sprite[11];

        public Sprite Star { get; private set; }
        public Sprite Star_Double { get; private set; }
        public Sprite Star_Each { get; private set; }
        public Sprite Star_Each_Double { get; private set; }
        public Sprite Star_Break { get; private set; }
        public Sprite Star_Break_Double { get; private set; }
        public Sprite Star_Ex { get; private set; }
        public Sprite Star_Ex_Double { get; private set; }

        public Sprite Hold { get; private set; }
        public Sprite Hold_On { get; private set; }
        public Sprite Hold_Off { get; private set; }
        public Sprite Hold_Each { get; private set; }
        public Sprite Hold_Each_On { get; private set; }
        public Sprite Hold_Ex { get; private set; }
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
        public Sprite TouchPoint { get; private set; }
        public Sprite TouchPoint_Each { get; private set; }
        public Sprite TouchPoint_Break { get; private set; }
        public Sprite TouchJust { get; private set; }
        public Sprite[] TouchBorder { get; private set; } = new Sprite[2];
        public Sprite[] TouchBorder_Each { get; private set; } = new Sprite[2];
        public Sprite[] TouchBorder_Break { get; private set; } = new Sprite[2];

        public Sprite[] TouchHold { get; private set; } = new Sprite[5];
        public Sprite[] TouchHold_Break { get; private set; } = new Sprite[5];
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
        CustomSkin()
        {

        }
        public CustomSkin(string skinCollectionPath)
        {
            if (!Directory.Exists(skinCollectionPath))
            {
                Directory.CreateDirectory(skinCollectionPath);
            }
            Name = new DirectoryInfo(skinCollectionPath).Name;

            if (File.Exists(skinCollectionPath + "/outline.png"))
            {
                IsOutlineAvailable = true;
            }
            Outline = SpriteLoader.Load(skinCollectionPath + "/outline.png");
            SubDisplay = SpriteLoader.Load(skinCollectionPath + "/SubBackgourd.png");

            Tap = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap.png");
            Tap_Each = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_each.png");
            Tap_Break = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_break.png");
            Tap_Ex = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_ex.png");

            Slide = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide.png");
            Slide_Each = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_each.png");
            Slide_Break = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_break.png");
            for (var i = 0; i < 11; i++)
            {
                Wifi[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_" + i + ".png");
                Wifi_Each[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_each_" + i + ".png");
                Wifi_Break[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_break_" + i + ".png");
            }

            Star = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star.png");
            Star_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_double.png");
            Star_Each = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each.png");
            Star_Each_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each_double.png");
            Star_Break = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break.png");
            Star_Break_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break_double.png");
            Star_Ex = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex.png");
            Star_Ex_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex_double.png");

            var border = new Vector4(0, 58, 0, 58);
            Hold = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold.png", border);
            Hold_Each = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each.png", border);
            Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each_on.png", border);
            Hold_Ex = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_ex.png", border);
            Hold_Break = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break.png", border);
            Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_on.png", border);

            if (File.Exists(Path.Combine(skinCollectionPath, "HoldSkins/hold_on.png")))
            {
                Hold_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_on.png", border);
            }
            else
            {
                Hold_On = Hold;
            }
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
            Touch_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_each.png");
            Touch_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break.png");
            TouchPoint = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point.png");
            TouchPoint_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point_each.png");
            TouchPoint_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_point.png");

            TouchJust = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_just.png");

            TouchBorder[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2.png");
            TouchBorder[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3.png");
            TouchBorder_Each[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_2_each.png");
            TouchBorder_Each[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_border_3_each.png");
            TouchBorder_Break[0] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_2.png");
            TouchBorder_Break[1] = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_border_3.png");

            for (var i = 0; i < 4; i++)
            {
                TouchHold[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_" + i + ".png");
                TouchHold_Break[i] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_" + i + ".png");
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
        }
        public static async Task<CustomSkin> LoadAsync(string skinCollectionPath)
        {
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
            customSkin.Outline = SpriteLoader.Load(skinCollectionPath + "/outline.png");
            customSkin.SubDisplay = SpriteLoader.Load(skinCollectionPath + "/SubBackgourd.png");

            customSkin.Tap = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap.png");
            customSkin.Tap_Each = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_each.png");
            customSkin.Tap_Break = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_break.png");
            customSkin.Tap_Ex = SpriteLoader.Load(skinCollectionPath + "/TapSkins/tap_ex.png");

            customSkin.Slide = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide.png");
            customSkin.Slide_Each = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_each.png");
            customSkin.Slide_Break = SpriteLoader.Load(skinCollectionPath + "/SlideSkins/slide_break.png");
            for (var i = 0; i < 11; i++)
            {
                customSkin.Wifi[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_" + i + ".png");
                customSkin.Wifi_Each[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_each_" + i + ".png");
                customSkin.Wifi_Break[i] = SpriteLoader.Load(skinCollectionPath + "/WifiSkins/wifi_break_" + i + ".png");
            }

            customSkin.Star = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star.png");
            customSkin.Star_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_double.png");
            customSkin.Star_Each = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each.png");
            customSkin.Star_Each_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_each_double.png");
            customSkin.Star_Break = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break.png");
            customSkin.Star_Break_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_break_double.png");
            customSkin.Star_Ex = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex.png");
            customSkin.Star_Ex_Double = SpriteLoader.Load(skinCollectionPath + "/StarSkins/star_ex_double.png");

            var border = new Vector4(0, 58, 0, 58);
            customSkin.Hold = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold.png", border);
            customSkin.Hold_Each = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each.png", border);
            customSkin.Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_each_on.png", border);
            customSkin.Hold_Ex = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_ex.png", border);
            customSkin.Hold_Break = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break.png", border);
            customSkin.Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/HoldSkins/hold_break_on.png", border);

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

            customSkin.Just[0] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r.png");
            customSkin.Just[1] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r.png");
            customSkin.Just[2] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u.png");
            customSkin.Just[3] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l.png");
            customSkin.Just[4] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l.png");
            customSkin.Just[5] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d.png");

            // Perfect

            customSkin.Just[6] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_p.png");
            customSkin.Just[7] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_p.png");
            customSkin.Just[8] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_p.png");
            customSkin.Just[9] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_p.png");
            customSkin.Just[10] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_p.png");
            customSkin.Just[11] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_p.png");

            // Fast Perfect

            customSkin.Just[12] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_p.png");
            customSkin.Just[13] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_p.png");
            customSkin.Just[14] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_p.png");
            customSkin.Just[15] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_p.png");
            customSkin.Just[16] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_p.png");
            customSkin.Just[17] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_p.png");

            // Fast Great

            customSkin.Just[18] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_gr.png");
            customSkin.Just[19] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_gr.png");
            customSkin.Just[20] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_gr.png");
            customSkin.Just[21] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_gr.png");
            customSkin.Just[22] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_gr.png");
            customSkin.Just[23] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_gr.png");

            // Fast Good

            customSkin.Just[24] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_fast_gd.png");
            customSkin.Just[25] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_fast_gd.png");
            customSkin.Just[26] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_fast_gd.png");
            customSkin.Just[27] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_fast_gd.png");
            customSkin.Just[28] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_fast_gd.png");
            customSkin.Just[29] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_fast_gd.png");

            // Late Perfect

            customSkin.Just[30] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_p.png");
            customSkin.Just[31] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_p.png");
            customSkin.Just[32] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_p.png");
            customSkin.Just[33] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_p.png");
            customSkin.Just[34] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_p.png");
            customSkin.Just[35] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_p.png");

            // Late Great

            customSkin.Just[36] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_gr.png");
            customSkin.Just[37] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_gr.png");
            customSkin.Just[38] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_gr.png");
            customSkin.Just[39] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_gr.png");
            customSkin.Just[40] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_gr.png");
            customSkin.Just[41] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_gr.png");

            // Late Good

            customSkin.Just[42] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_r_late_gd.png");
            customSkin.Just[43] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_r_late_gd.png");
            customSkin.Just[44] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_u_late_gd.png");
            customSkin.Just[45] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_curv_l_late_gd.png");
            customSkin.Just[46] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_str_l_late_gd.png");
            customSkin.Just[47] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/just_wifi_d_late_gd.png");

            // Miss

            customSkin.Just[48] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_curv_r.png");
            customSkin.Just[49] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_str_r.png");
            customSkin.Just[50] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_wifi_u.png");
            customSkin.Just[51] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_curv_l.png");
            customSkin.Just[52] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_str_l.png");
            customSkin.Just[53] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/miss_wifi_d.png");

            // TooFast
            customSkin.Just[54] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_curv_r.png");
            customSkin.Just[55] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_str_r.png");
            customSkin.Just[56] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_wifi_u.png");
            customSkin.Just[57] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_curv_l.png");
            customSkin.Just[58] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_str_l.png");
            customSkin.Just[59] = SpriteLoader.Load(skinCollectionPath + "/SlideOKSkins/toofast_wifi_d.png");

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



            customSkin.CriticalPerfect_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_cPerfect_break.png");
            customSkin.Break_2600_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_break_2600_shine.png");
            customSkin.Perfect_Shine = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/judge_text_perfect_break.png");


            foreach (var value in stackalloc int[] { 2600, 2550, 2500, 2000, 1500, 1250, 1000, 0 })
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
            }

            customSkin.Fast = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/fast.png");
            customSkin.Late = SpriteLoader.Load(skinCollectionPath + "/JudgeTextSkins/late.png");

            customSkin.Touch = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch.png");
            customSkin.Touch_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_each.png");
            customSkin.Touch_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break.png");
            customSkin.TouchPoint = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point.png");
            customSkin.TouchPoint_Each = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_point_each.png");
            customSkin.TouchPoint_Break = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_break_point.png");

            customSkin.TouchJust = SpriteLoader.Load(skinCollectionPath + "/TouchSkins/touch_just.png");

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
            }
            customSkin.TouchHold[4] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_border.png");
            customSkin.TouchHold_Break[4] = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_break_border.png");
            customSkin.TouchHold_Off = SpriteLoader.Load(skinCollectionPath + "/TouchHoldSkins/touchhold_off.png");

            customSkin.LoadingSplash = SpriteLoader.Load(skinCollectionPath + "/now_loading.png");

            customSkin.TapLine_Normal = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Normal.png");
            customSkin.TapLine_Each = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Each.png");
            customSkin.TapLine_Slide = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Slide.png");
            customSkin.TapLine_Break = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Break.png");

            customSkin.EachLines[0] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine1.png");
            customSkin.EachLines[1] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine2.png");
            customSkin.EachLines[2] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine3.png");
            customSkin.EachLines[3] = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/EachLine4.png");

            customSkin.HoldEndPoint_Normal = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_End.png");
            customSkin.HoldEndPoint_Each = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_Each_End.png");
            customSkin.HoldEndPoint_Break = SpriteLoader.Load(skinCollectionPath + "/NoteGuideSkins/Hold_Break_End.png");

            return customSkin;
        }
    }
}
