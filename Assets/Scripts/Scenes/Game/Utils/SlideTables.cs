using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes.Slide;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
#nullable enable
namespace MajdataPlay.Game.Notes.Slide.Utils
{
    public static class SlideTables
    {
        public static SlideTable[] SLIDE_TABLES => new SlideTable[]
        {
            new SlideTable()
            {
                Name = "circle2",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3,false),
                    BuildSlideArea(SensorArea.A2,5,7,true,true)
                },
                Const = 0.46526f
            },
            new SlideTable()
            {
                Name = "circle3",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11,false),
                    BuildSlideArea(SensorArea.A3,13,15,true,true)
                },
                Const = 0.23263f
            },
            new SlideTable()
            {
                Name = "circle4",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11),
                    BuildSlideArea(SensorArea.A3,14,19),
                    BuildSlideArea(SensorArea.A4,21,23,true,true)
                },
                Const = 0.15509f
            },
            new SlideTable()
            {
                Name = "circle5",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11),
                    BuildSlideArea(SensorArea.A3,14,19),
                    BuildSlideArea(SensorArea.A4,23,27),
                    BuildSlideArea(SensorArea.A5,29,31,true,true)
                },
                Const = 0.11631f
            },
            new SlideTable()
            {
                Name = "circle6",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11),
                    BuildSlideArea(SensorArea.A3,14,19),
                    BuildSlideArea(SensorArea.A4,23,27),
                    BuildSlideArea(SensorArea.A5,31,35),
                    BuildSlideArea(SensorArea.A6,37,39,true,true)
                },
                Const = 0.09305f
            },
            new SlideTable()
            {
                Name = "circle7",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11),
                    BuildSlideArea(SensorArea.A3,14,19),
                    BuildSlideArea(SensorArea.A4,23,27),
                    BuildSlideArea(SensorArea.A5,31,35),
                    BuildSlideArea(SensorArea.A6,39,43),
                    BuildSlideArea(SensorArea.A7,45,47,true,true)
                },
                Const = 0.07754f
            },
            new SlideTable()
            {
                Name = "circle8",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11),
                    BuildSlideArea(SensorArea.A3,14,19),
                    BuildSlideArea(SensorArea.A4,23,27),
                    BuildSlideArea(SensorArea.A5,31,35),
                    BuildSlideArea(SensorArea.A6,39,43),
                    BuildSlideArea(SensorArea.A7,46,50),
                    BuildSlideArea(SensorArea.A8,53,55,true,true)
                },
                Const = 0.06647f
            },
            new SlideTable()
            {
                Name = "circle1",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.A2,7,11),
                    BuildSlideArea(SensorArea.A3,14,19),
                    BuildSlideArea(SensorArea.A4,23,27),
                    BuildSlideArea(SensorArea.A5,31,35),
                    BuildSlideArea(SensorArea.A6,39,43),
                    BuildSlideArea(SensorArea.A7,46,50),
                    BuildSlideArea(SensorArea.A8,54,58),
                    BuildSlideArea(SensorArea.A1,61,63,true,true)
                },
                Const = 0.05816f
            },
            new SlideTable()
            {
                Name = "line3",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,2),
                    BuildSlideArea(new SensorArea[]{SensorArea.A2,SensorArea.B2 },5,8,false),
                    BuildSlideArea(SensorArea.A3,10,13,true,true)
                },
                Const = 0.19195f
            },
            new SlideTable()
            {
                Name = "line4",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B2,5,8),
                    BuildSlideArea(SensorArea.B3,10,12),
                    BuildSlideArea(SensorArea.A4,15,18,true,true)
                },
                Const = 0.17929f
            },
            new SlideTable()
            {
                Name = "line5",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,9,11),
                    BuildSlideArea(SensorArea.B5,13,15),
                    BuildSlideArea(SensorArea.A5,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "line6",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5,8),
                    BuildSlideArea(SensorArea.B7,10,12),
                    BuildSlideArea(SensorArea.A6,15,18,true,true)
                },
                Const = 0.17929f
            },
            new SlideTable()
            {
                Name = "line7",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,2),
                    BuildSlideArea(new SensorArea[]{SensorArea.A8,SensorArea.B8 },5,8,false),
                    BuildSlideArea(SensorArea.A7,10,13,true,true)
                },
                Const = 0.19195f
            },
            new SlideTable()
            {
                Name = "v1",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B1,13,15),
                    BuildSlideArea(SensorArea.A1,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v2",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B2,13,15),
                    BuildSlideArea(SensorArea.A2,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v3",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B3,13,15),
                    BuildSlideArea(SensorArea.A3,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v4",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B4,13,15),
                    BuildSlideArea(SensorArea.A4,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v6",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B6,13,15),
                    BuildSlideArea(SensorArea.A6,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v7",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B7,13,15),
                    BuildSlideArea(SensorArea.A7,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v8",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,8,11),
                    BuildSlideArea(SensorArea.B8,13,15),
                    BuildSlideArea(SensorArea.A8,17,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "ppqq1",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,10,13),
                    BuildSlideArea(SensorArea.B4,15,17),
                    BuildSlideArea(SensorArea.A3,21,26),
                    BuildSlideArea(SensorArea.A2,29,32),
                    BuildSlideArea(SensorArea.A1,33,35,true,true)
                },
                Const = 0.073445f

            },
            new SlideTable()
            {
                Name = "ppqq2",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,9,12),
                    BuildSlideArea(SensorArea.B4,14,16),
                    BuildSlideArea(SensorArea.A3,20,25),
                    BuildSlideArea(SensorArea.A2,26,28,true,true),
                },
                Const = 0.087213f
            },
            new SlideTable()
            {
                Name = "ppqq3",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,4,6),
                    BuildSlideArea(SensorArea.C,9,12),
                    BuildSlideArea(SensorArea.B4,13,15),
                    BuildSlideArea(SensorArea.A3,19,22,true,true),
                },
                Const = 0.15091f
            },
            new SlideTable()
            {
                Name = "ppqq4",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,9,12),
                    BuildSlideArea(SensorArea.B4,14,16),
                    BuildSlideArea(SensorArea.A3,20,25),
                    BuildSlideArea(SensorArea.A2,27,29),
                    BuildSlideArea(SensorArea.B1,32,35),
                    BuildSlideArea(SensorArea.C,37,40),
                    BuildSlideArea(SensorArea.B4,42,44),
                    BuildSlideArea(SensorArea.A4,46,49,true,true),
                },
                Const = 0.06976f
            },
            new SlideTable()
            {
                Name = "ppqq5",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,9,12),
                    BuildSlideArea(SensorArea.B4,14,16),
                    BuildSlideArea(SensorArea.A3,20,25),
                    BuildSlideArea(SensorArea.A2,27,29),
                    BuildSlideArea(SensorArea.B1,32,35),
                    BuildSlideArea(SensorArea.C,37,40),
                    BuildSlideArea(SensorArea.B5,42,44),
                    BuildSlideArea(SensorArea.A5,46,49,true,true),
                },
                Const = 0.06976f
            },
            new SlideTable()
            {
                Name = "ppqq6",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,9,12),
                    BuildSlideArea(SensorArea.B4,14,16),
                    BuildSlideArea(SensorArea.A3,20,25),
                    BuildSlideArea(SensorArea.A2,26,28),
                    BuildSlideArea(SensorArea.B1,31,34),
                    BuildSlideArea(new SensorArea[] { SensorArea.C,SensorArea.B8 },36,38),
                    BuildSlideArea(new SensorArea[] { SensorArea.B7,SensorArea.B6 },39,41),
                    BuildSlideArea(SensorArea.A6,45,48,true,true),
                },
                Const = 0.07107f
            },
            new SlideTable()
            {
                Name = "ppqq7",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,10,13),
                    BuildSlideArea(SensorArea.B4,15,17),
                    BuildSlideArea(SensorArea.A3,22,27),
                    BuildSlideArea(SensorArea.A2,29,31),
                    BuildSlideArea(SensorArea.B1,34,37),
                    BuildSlideArea(SensorArea.B8,39,41),
                    BuildSlideArea(SensorArea.A7,43,46,true,true),
                },
                Const = 0.08106f
            },
            new SlideTable()
            {
                Name = "ppqq8",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B1,5,7),
                    BuildSlideArea(SensorArea.C,9,12),
                    BuildSlideArea(SensorArea.B4,14,16),
                    BuildSlideArea(SensorArea.A3,20,25),
                    BuildSlideArea(SensorArea.A2,27,29),
                    BuildSlideArea(new SensorArea[] { SensorArea.B1,SensorArea.A1 },32,35),
                    BuildSlideArea(SensorArea.A8,38,41,true,true),
                },
                Const = 0.06027f
            },
            new SlideTable()
            {
                Name = "L2",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,2),
                    BuildSlideArea(new SensorArea[] { SensorArea.B8,SensorArea.A8 },4,7,false),
                    BuildSlideArea(SensorArea.A7,11,15),
                    BuildSlideArea(SensorArea.B8,18,21),
                    BuildSlideArea(SensorArea.B1,23,26),
                    BuildSlideArea(SensorArea.A2,29,32,true,true),
                },
                Const = 0.09482f
            },
            new SlideTable()
            {
                Name = "L3",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,2),
                    BuildSlideArea(new SensorArea[] { SensorArea.B8,SensorArea.A8 },5,8,false),
                    BuildSlideArea(SensorArea.A7,12,17),
                    BuildSlideArea(SensorArea.B7,19,22),
                    BuildSlideArea(SensorArea.C,24,26),
                    BuildSlideArea(SensorArea.B3,27,29),
                    BuildSlideArea(SensorArea.A3,31,34,true,true),
                },
                Const = 0.09546f
            },
            new SlideTable()
            {
                Name = "L4",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,2),
                    BuildSlideArea(new SensorArea[] { SensorArea.B8,SensorArea.A8 },5,8,false),
                    BuildSlideArea(SensorArea.A7,12,17),
                    BuildSlideArea(SensorArea.B6,19,22),
                    BuildSlideArea(SensorArea.B5,24,26),
                    BuildSlideArea(SensorArea.A4,29,32,true,true),
                },
                Const = 0.10176f
            },
            new SlideTable()
            {
                Name = "L5",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,2),
                    BuildSlideArea(new SensorArea[] { SensorArea.B8,SensorArea.A8 },5,8,false),
                    BuildSlideArea(SensorArea.A7,12,16),
                    BuildSlideArea(new SensorArea[] { SensorArea.B6,SensorArea.A6 },19,22,false),
                    BuildSlideArea(SensorArea.A5,25,28,true,true),
                },
                Const = 0.09598f
            },
            new SlideTable()
            {
                Name = "s",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5,8),
                    BuildSlideArea(SensorArea.B7,9,11),
                    BuildSlideArea(SensorArea.C,14,17),
                    BuildSlideArea(SensorArea.B3,19,21),
                    BuildSlideArea(SensorArea.B4,22,24),
                    BuildSlideArea(SensorArea.A5,27,30,true,true),
                },
                Const = 0.10546f
            },
            new SlideTable()
            {
                Name = "pq1",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5, 8),
                    BuildSlideArea(SensorArea.B7,9,11),
                    BuildSlideArea(SensorArea.B6,12,14),
                    BuildSlideArea(SensorArea.B5,15,17),
                    BuildSlideArea(SensorArea.B4,19,21),
                    BuildSlideArea(SensorArea.B3,22,24),
                    BuildSlideArea(SensorArea.B2,25,27),
                    BuildSlideArea(SensorArea.A1,30,33,true,true),
                },
                Const = 0.09215f
            },
            new SlideTable()
            {
                Name = "pq2",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5,8),
                    BuildSlideArea(SensorArea.B7,9,11),
                    BuildSlideArea(SensorArea.B6,12,14),
                    BuildSlideArea(SensorArea.B5,16,18),
                    BuildSlideArea(SensorArea.B4,19,21),
                    BuildSlideArea(SensorArea.B3,22,24),
                    BuildSlideArea(SensorArea.A2,27,30,true,true),
                },
                Const = 0.10208f
            },
            new SlideTable()
            {
                Name = "pq3",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,6,9),
                    BuildSlideArea(SensorArea.B7,10,12),
                    BuildSlideArea(SensorArea.B6,14,16),
                    BuildSlideArea(SensorArea.B5,17,19),
                    BuildSlideArea(SensorArea.B4,21,23),
                    BuildSlideArea(SensorArea.A3,25,27,true,true),
                },
                Const = 0.12468f
            },
            new SlideTable()
            {
                Name = "pq4",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,6,9),
                    BuildSlideArea(SensorArea.B7,11,13),
                    BuildSlideArea(SensorArea.B6,14,16),
                    BuildSlideArea(SensorArea.B5,18,20),
                    BuildSlideArea(SensorArea.A4,22,24,true,true),
                },
                Const = 0.14359f
            },
            new SlideTable()
            {
                Name = "pq5",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,6,9),
                    BuildSlideArea(SensorArea.B7,11,13),
                    BuildSlideArea(SensorArea.B6,15,17),
                    BuildSlideArea(SensorArea.A5,19,21,true,true),
                },
                Const = 0.16925f
            },
            new SlideTable()
            {
                Name = "pq6",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5,8),
                    BuildSlideArea(SensorArea.B7,9,11),
                    BuildSlideArea(SensorArea.B6,13,15),
                    BuildSlideArea(SensorArea.B5,16,18),
                    BuildSlideArea(SensorArea.B4,19,21),
                    BuildSlideArea(SensorArea.B3,23,25),
                    BuildSlideArea(SensorArea.B2,26,28),
                    BuildSlideArea(SensorArea.B1,29,31),
                    BuildSlideArea(SensorArea.B8,33,35),
                    BuildSlideArea(SensorArea.B7,36,38),
                    BuildSlideArea(SensorArea.A6,40,42,true,true),
                },
                Const = 0.07518f
            },
            new SlideTable()
            {
                Name = "pq7",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5,8),
                    BuildSlideArea(SensorArea.B7,10,12),
                    BuildSlideArea(SensorArea.B6,13,15),
                    BuildSlideArea(SensorArea.B5,16,18),
                    BuildSlideArea(SensorArea.B4,20,22),
                    BuildSlideArea(SensorArea.B3,23,25),
                    BuildSlideArea(SensorArea.B2,26,28),
                    BuildSlideArea(SensorArea.B1,30,32),
                    BuildSlideArea(SensorArea.B8,33,35),
                    BuildSlideArea(SensorArea.A7,37,40,true,true),
                },
                Const = 0.08167f
            },
            new SlideTable()
            {
                Name = "pq8",
                JudgeQueue = new SlideArea[]
                {
                    BuildSlideArea(SensorArea.A1,0,3),
                    BuildSlideArea(SensorArea.B8,5,8),
                    BuildSlideArea(SensorArea.B7,9,11),
                    BuildSlideArea(SensorArea.B6,12,14),
                    BuildSlideArea(SensorArea.B5,15,17),
                    BuildSlideArea(SensorArea.B4,19,21),
                    BuildSlideArea(SensorArea.B3,22,24),
                    BuildSlideArea(SensorArea.B2,25,27),
                    BuildSlideArea(SensorArea.B1,28,30),
                    BuildSlideArea(SensorArea.A8,33,36,true,true),
                },
                Const = 0.08398f
            },
        };
        public static SlideArea[][] WIFISLIDE_JUDGE_QUEUE => new SlideArea[][]
        {
            new SlideArea[] // L
            {
                BuildSlideArea(SensorArea.A1,0),
                BuildSlideArea(SensorArea.B8,2),
                BuildSlideArea(SensorArea.B7,4),
                BuildSlideArea(new SensorArea[]{ SensorArea.A6 , SensorArea.D6 },7,true,true)
            },
            new SlideArea[] // Center
            {
                BuildSlideArea(SensorArea.A1,0),
                BuildSlideArea(SensorArea.B1,2),
                BuildSlideArea(SensorArea.C,4),
                BuildSlideArea(new SensorArea[]{ SensorArea.A5 , SensorArea.B5 },7,true,true)
            },
            new SlideArea[] // R
            {
                BuildSlideArea(SensorArea.A1,0),
                BuildSlideArea(SensorArea.B2,2),
                BuildSlideArea(SensorArea.B3,4),
                BuildSlideArea(new SensorArea[]{ SensorArea.A4 , SensorArea.D5 },7,true,true)
            }
        };
        public static SlideArea[][] WIFISLIDE_JUDGE_QUEUE_CLASSIC => new SlideArea[][]
        {
            new SlideArea[] // L
            {
                BuildSlideArea(SensorArea.A1,0),
                BuildSlideArea(SensorArea.B8,2),
                BuildSlideArea(SensorArea.B7,4),
                BuildSlideArea(new SensorArea[]{ SensorArea.A6 , SensorArea.D6 },7,true,true)
            },
            new SlideArea[] // Center
            {
                BuildSlideArea(SensorArea.A1,0),
                BuildSlideArea(SensorArea.B1,2),
                BuildSlideArea(SensorArea.C,7,true,false),
            },
            new SlideArea[] // R
            {
                BuildSlideArea(SensorArea.A1,0),
                BuildSlideArea(SensorArea.B2,2),
                BuildSlideArea(SensorArea.B3,4),
                BuildSlideArea(new SensorArea[]{ SensorArea.A4 , SensorArea.D5 },7,true,true)
            }
        };
        public static SlideTable? FindTableByName(string prefabName)
        {
            var result = SLIDE_TABLES.Find(x => x.Name == prefabName);
            var clone = result.Clone();
            return clone;
        }
        public static SlideArea[][] GetWifiTable(int startPos)
        {
            List<SlideArea[]> queue = new();
            var raw = MajInstances.Setting.Judge.Mode == JudgeMode.Modern ? WIFISLIDE_JUDGE_QUEUE : WIFISLIDE_JUDGE_QUEUE_CLASSIC;
            foreach (var line in raw)
            {
                List<SlideArea> rows = new();
                foreach (var row in line)
                    rows.Add(row.Clone()!);
                queue.Add(rows.ToArray());
            }
            var _queue = queue.ToArray();
            var diff = Math.Abs(1 - startPos);

            if (diff != 0)
            {
                foreach (var line in _queue)
                    foreach (var area in line)
                        area.Diff(diff);
            }

            return _queue;
        }
        static SlideArea BuildSlideArea(SensorArea type, int arrowProgress, bool isSkippable = true, bool isLast = false)
        {
            var obj = new SlideArea(new Dictionary<SensorArea, bool>
                      {
                          { type, isLast}
                      }, arrowProgress);
            obj.IsSkippable = isSkippable;
            return obj;
        }
        static SlideArea BuildSlideArea(SensorArea type, int progressWhenOn, int progressWhenFinished, bool isSkippable = true, bool isLast = false)
        {
            var obj = new SlideArea(new Dictionary<SensorArea, bool>
                      {
                          { type, isLast}
                      }, progressWhenOn, progressWhenFinished);
            obj.IsSkippable = isSkippable;
            return obj;
        }
        static SlideArea BuildSlideArea(SensorArea[] type, int barIndex, bool isSkippable = true, bool isLast = false)
        {
            var table = new Dictionary<SensorArea, bool>();
            foreach (var sensorType in type)
                table.Add(sensorType, isLast);

            var obj = new SlideArea(table, barIndex);
            obj.IsSkippable = isSkippable;
            return obj;
        }
        static SlideArea BuildSlideArea(SensorArea[] type, int progressWhenOn, int progressWhenFinished, bool isSkippable = true, bool isLast = false)
        {
            var table = new Dictionary<SensorArea, bool>();
            foreach (var sensorType in type)
                table.Add(sensorType, isLast);

            var obj = new SlideArea(table, progressWhenOn, progressWhenFinished);
            obj.IsSkippable = isSkippable;
            return obj;
        }
    }
}
