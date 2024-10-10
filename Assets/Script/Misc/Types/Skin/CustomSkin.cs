using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Types
{
    public class CustomSkin
    {
        public string Name { get; private set; }
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
        
        public Sprite[] Just { get; private set; } = new Sprite[54];
        public Sprite[] JudgeText { get; private set; } = new Sprite[5];
        public Sprite CriticalPerfect_Break { get; private set; }
        public Sprite Perfect_Break { get; private set; }
        public Sprite FastText { get; private set; }
        public Sprite LateText { get; private set; }

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
        public CustomSkin(string skinCollectionPath)
        {
            if (!Directory.Exists(skinCollectionPath))
                Directory.CreateDirectory(skinCollectionPath);
            Name = new DirectoryInfo(skinCollectionPath).Name;

            Outline = SpriteLoader.Load(skinCollectionPath + "/outline.png");
            SubDisplay = SpriteLoader.Load(skinCollectionPath + "/SubBackgourd.png");

            Tap = SpriteLoader.Load(skinCollectionPath + "/tap.png");
            Tap_Each = SpriteLoader.Load(skinCollectionPath + "/tap_each.png");
            Tap_Break = SpriteLoader.Load(skinCollectionPath + "/tap_break.png");
            Tap_Ex = SpriteLoader.Load(skinCollectionPath + "/tap_ex.png");

            Slide = SpriteLoader.Load(skinCollectionPath + "/slide.png");
            Slide_Each = SpriteLoader.Load(skinCollectionPath + "/slide_each.png");
            Slide_Break = SpriteLoader.Load(skinCollectionPath + "/slide_break.png");
            for (var i = 0; i < 11; i++)
            {
                Wifi[i] = SpriteLoader.Load(skinCollectionPath + "/wifi_" + i + ".png");
                Wifi_Each[i] = SpriteLoader.Load(skinCollectionPath + "/wifi_each_" + i + ".png");
                Wifi_Break[i] = SpriteLoader.Load(skinCollectionPath + "/wifi_break_" + i + ".png");
            }

            Star = SpriteLoader.Load(skinCollectionPath + "/star.png");
            Star_Double = SpriteLoader.Load(skinCollectionPath + "/star_double.png");
            Star_Each = SpriteLoader.Load(skinCollectionPath + "/star_each.png");
            Star_Each_Double = SpriteLoader.Load(skinCollectionPath + "/star_each_double.png");
            Star_Break = SpriteLoader.Load(skinCollectionPath + "/star_break.png");
            Star_Break_Double = SpriteLoader.Load(skinCollectionPath + "/star_break_double.png");
            Star_Ex = SpriteLoader.Load(skinCollectionPath + "/star_ex.png");
            Star_Ex_Double = SpriteLoader.Load(skinCollectionPath + "/star_ex_double.png");

            var border = new Vector4(0, 58, 0, 58);
            Hold = SpriteLoader.Load(skinCollectionPath + "/hold.png", border);
            Hold_Each = SpriteLoader.Load(skinCollectionPath + "/hold_each.png", border);
            Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/hold_each_on.png", border);
            Hold_Ex = SpriteLoader.Load(skinCollectionPath + "/hold_ex.png", border);
            Hold_Break = SpriteLoader.Load(skinCollectionPath + "/hold_break.png", border);
            Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/hold_break_on.png", border);

            if (File.Exists(Path.Combine(skinCollectionPath, "hold_on.png")))
                Hold_On = SpriteLoader.Load(skinCollectionPath + "/hold_on.png", border);
            else
                Hold_On = Hold;
            Hold_Off = SpriteLoader.Load(skinCollectionPath + "/hold_off.png", border);
            if (File.Exists(Path.Combine(skinCollectionPath, "hold_each_on.png")))
                Hold_Each_On = SpriteLoader.Load(skinCollectionPath + "/hold_each_on.png", border);
            else
                Hold_Each_On = Hold_Each;

            if (File.Exists(Path.Combine(skinCollectionPath, "hold_break_on.png")))
                Hold_Break_On = SpriteLoader.Load(skinCollectionPath + "/hold_break_on.png", border);
            else
                Hold_Break_On = Hold_Break;

            // Critical Perfect

            Just[0] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r.png");
            Just[1] = SpriteLoader.Load(skinCollectionPath + "/just_str_r.png");
            Just[2] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u.png");
            Just[3] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l.png");
            Just[4] = SpriteLoader.Load(skinCollectionPath + "/just_str_l.png");
            Just[5] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d.png");

            // Perfect

            Just[6] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_p.png");
            Just[7] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_p.png");
            Just[8] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_p.png");
            Just[9] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_p.png");
            Just[10] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_p.png");
            Just[11] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_p.png");

            // Fast Perfect

            Just[12] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_fast_p.png");
            Just[13] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_fast_p.png");
            Just[14] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_fast_p.png");
            Just[15] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_fast_p.png");
            Just[16] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_fast_p.png");
            Just[17] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_fast_p.png");

            // Fast Great

            Just[18] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_fast_gr.png");
            Just[19] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_fast_gr.png");
            Just[20] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_fast_gr.png");
            Just[21] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_fast_gr.png");
            Just[22] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_fast_gr.png");
            Just[23] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_fast_gr.png");

            // Fast Good

            Just[24] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_fast_gd.png");
            Just[25] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_fast_gd.png");
            Just[26] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_fast_gd.png");
            Just[27] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_fast_gd.png");
            Just[28] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_fast_gd.png");
            Just[29] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_fast_gd.png");

            // Late Perfect

            Just[30] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_late_p.png");
            Just[31] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_late_p.png");
            Just[32] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_late_p.png");
            Just[33] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_late_p.png");
            Just[34] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_late_p.png");
            Just[35] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_late_p.png");

            // Late Great

            Just[36] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_late_gr.png");
            Just[37] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_late_gr.png");
            Just[38] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_late_gr.png");
            Just[39] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_late_gr.png");
            Just[40] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_late_gr.png");
            Just[41] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_late_gr.png");

            // Late Good

            Just[42] = SpriteLoader.Load(skinCollectionPath + "/just_curv_r_late_gd.png");
            Just[43] = SpriteLoader.Load(skinCollectionPath + "/just_str_r_late_gd.png");
            Just[44] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_u_late_gd.png");
            Just[45] = SpriteLoader.Load(skinCollectionPath + "/just_curv_l_late_gd.png");
            Just[46] = SpriteLoader.Load(skinCollectionPath + "/just_str_l_late_gd.png");
            Just[47] = SpriteLoader.Load(skinCollectionPath + "/just_wifi_d_late_gd.png");

            // Miss

            Just[48] = SpriteLoader.Load(skinCollectionPath + "/miss_curv_r.png");
            Just[49] = SpriteLoader.Load(skinCollectionPath + "/miss_str_r.png");
            Just[50] = SpriteLoader.Load(skinCollectionPath + "/miss_wifi_u.png");
            Just[51] = SpriteLoader.Load(skinCollectionPath + "/miss_curv_l.png");
            Just[52] = SpriteLoader.Load(skinCollectionPath + "/miss_str_l.png");
            Just[53] = SpriteLoader.Load(skinCollectionPath + "/miss_wifi_d.png");

            JudgeText[0] = SpriteLoader.Load(skinCollectionPath + "/judge_text_miss.png");
            JudgeText[1] = SpriteLoader.Load(skinCollectionPath + "/judge_text_good.png");
            JudgeText[2] = SpriteLoader.Load(skinCollectionPath + "/judge_text_great.png");
            JudgeText[3] = SpriteLoader.Load(skinCollectionPath + "/judge_text_perfect.png");
            JudgeText[4] = SpriteLoader.Load(skinCollectionPath + "/judge_text_cPerfect.png");
            CriticalPerfect_Break = SpriteLoader.Load(skinCollectionPath + "/judge_text_break.png");
            Perfect_Break = SpriteLoader.Load(skinCollectionPath + "/judge_text_perfect_break.png");

            FastText = SpriteLoader.Load(skinCollectionPath + "/fast.png");
            LateText = SpriteLoader.Load(skinCollectionPath + "/late.png");

            Touch = SpriteLoader.Load(skinCollectionPath + "/touch.png");
            Touch_Each = SpriteLoader.Load(skinCollectionPath + "/touch_each.png");
            Touch_Break = SpriteLoader.Load(skinCollectionPath + "/touch_break.png");
            TouchPoint = SpriteLoader.Load(skinCollectionPath + "/touch_point.png");
            TouchPoint_Each = SpriteLoader.Load(skinCollectionPath + "/touch_point_each.png");
            TouchPoint_Break = SpriteLoader.Load(skinCollectionPath + "/touch_break_point.png");

            TouchJust = SpriteLoader.Load(skinCollectionPath + "/touch_just.png");

            TouchBorder[0] = SpriteLoader.Load(skinCollectionPath + "/touch_border_2.png");
            TouchBorder[1] = SpriteLoader.Load(skinCollectionPath + "/touch_border_3.png");
            TouchBorder_Each[0] = SpriteLoader.Load(skinCollectionPath + "/touch_border_2_each.png");
            TouchBorder_Each[1] = SpriteLoader.Load(skinCollectionPath + "/touch_border_3_each.png");
            TouchBorder_Break[0] = SpriteLoader.Load(skinCollectionPath + "/touch_break_border_2.png");
            TouchBorder_Break[1] = SpriteLoader.Load(skinCollectionPath + "/touch_break_border_3.png");

            for (var i = 0; i < 4; i++)
            {
                TouchHold[i] = SpriteLoader.Load(skinCollectionPath + "/touchhold_" + i + ".png");
                TouchHold_Break[i] = SpriteLoader.Load(skinCollectionPath + "/touchhold_break_" + i + ".png");
            }
            TouchHold[4] = SpriteLoader.Load(skinCollectionPath + "/touchhold_border.png");
            TouchHold_Break[4] = SpriteLoader.Load(skinCollectionPath + "/touchhold_break_border.png");
            TouchHold_Off = SpriteLoader.Load(skinCollectionPath + "/touchhold_off.png");

            LoadingSplash = SpriteLoader.Load(skinCollectionPath + "/now_loading.png");
        }
    }
}
