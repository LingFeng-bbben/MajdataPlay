using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Types;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
#nullable enable
namespace MajdataPlay.Game.Utils
{
    public static class SlideTables
    {
        public static SlideTable[] SLIDE_TABLES => new SlideTable[]
        {
            new SlideTable()
            {
                Name = "circle2",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3,false),
                    BuildJudgeArea(SensorType.A2,7,true,true)
                },
                Const = 0.46526f
            },
            new SlideTable()
            {
                Name = "circle3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11,false),
                    BuildJudgeArea(SensorType.A3,15,true,true)
                },
                Const = 0.23263f
            },
            new SlideTable()
            {
                Name = "circle4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11),
                    BuildJudgeArea(SensorType.A3,19),
                    BuildJudgeArea(SensorType.A4,23,true,true)
                },
                Const = 0.15509f
            },
            new SlideTable()
            {
                Name = "circle5",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11),
                    BuildJudgeArea(SensorType.A3,19),
                    BuildJudgeArea(SensorType.A4,27),
                    BuildJudgeArea(SensorType.A5,31,true,true)
                },
                Const = 0.11631f
            },
            new SlideTable()
            {
                Name = "circle6",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11),
                    BuildJudgeArea(SensorType.A3,19),
                    BuildJudgeArea(SensorType.A4,27),
                    BuildJudgeArea(SensorType.A5,35),
                    BuildJudgeArea(SensorType.A6,39,true,true)
                },
                Const = 0.09305f
            },
            new SlideTable()
            {
                Name = "circle7",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11),
                    BuildJudgeArea(SensorType.A3,19),
                    BuildJudgeArea(SensorType.A4,27),
                    BuildJudgeArea(SensorType.A5,35),
                    BuildJudgeArea(SensorType.A6,43),
                    BuildJudgeArea(SensorType.A7,47,true,true)
                },
                Const = 0.07754f
            },
            new SlideTable()
            {
                Name = "circle8",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11),
                    BuildJudgeArea(SensorType.A3,19),
                    BuildJudgeArea(SensorType.A4,27),
                    BuildJudgeArea(SensorType.A5,35),
                    BuildJudgeArea(SensorType.A6,43),
                    BuildJudgeArea(SensorType.A7,50),
                    BuildJudgeArea(SensorType.A8,55,true,true)
                },
                Const = 0.06647f
            },
            new SlideTable()
            {
                Name = "circle1",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11),
                    BuildJudgeArea(SensorType.A3,19),
                    BuildJudgeArea(SensorType.A4,27),
                    BuildJudgeArea(SensorType.A5,35),
                    BuildJudgeArea(SensorType.A6,43),
                    BuildJudgeArea(SensorType.A7,50),
                    BuildJudgeArea(SensorType.A8,58),
                    BuildJudgeArea(SensorType.A1,63,true,true)
                },
                Const = 0.05816f
            },
            new SlideTable()
            {
                Name = "line3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[]{SensorType.A2,SensorType.B2 },8,false),
                    BuildJudgeArea(SensorType.A3,13,true,true)
                },
                Const = 0.19195f
            },
            new SlideTable()
            {
                Name = "line4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B2,8),
                    BuildJudgeArea(SensorType.B3,12),
                    BuildJudgeArea(SensorType.A4,18,true,true)
                },
                Const = 0.17929f
            },
            new SlideTable()
            {
                Name = "line5",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B5,15),
                    BuildJudgeArea(SensorType.A5,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "line6",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,12),
                    BuildJudgeArea(SensorType.A6,18,true,true)
                },
                Const = 0.17929f
            },
            new SlideTable()
            {
                Name = "line7",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[]{SensorType.A8,SensorType.B8 },8,false),
                    BuildJudgeArea(SensorType.A7,13,true,true)
                },
                Const = 0.19195f
            },
            new SlideTable()
            {
                Name = "v1",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B1,15),
                    BuildJudgeArea(SensorType.A1,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v2",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B2,15),
                    BuildJudgeArea(SensorType.A2,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B3,15),
                    BuildJudgeArea(SensorType.A3,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B4,15),
                    BuildJudgeArea(SensorType.A4,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v6",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B6,15),
                    BuildJudgeArea(SensorType.A6,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v7",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B7,15),
                    BuildJudgeArea(SensorType.A7,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "v8",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B8,15),
                    BuildJudgeArea(SensorType.A8,19,true,true)
                },
                Const = 0.16287f
            },
            new SlideTable()
            {
                Name = "ppqq1",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,13),
                    BuildJudgeArea(SensorType.B4,17),
                    BuildJudgeArea(SensorType.A3,26),
                    BuildJudgeArea(SensorType.A2,32),
                    BuildJudgeArea(SensorType.A1,35,true,true)
                },
                Const = 0.073445f

            },
            new SlideTable()
            {
                Name = "ppqq2",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,12),
                    BuildJudgeArea(SensorType.B4,16),
                    BuildJudgeArea(SensorType.A3,25),
                    BuildJudgeArea(SensorType.A2,28,true,true),
                },
                Const = 0.087213f
            },
            new SlideTable()
            {
                Name = "ppqq3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,12),
                    BuildJudgeArea(SensorType.B4,15),
                    BuildJudgeArea(SensorType.A3,22,true,true),
                },
                Const = 0.15091f
            },
            new SlideTable()
            {
                Name = "ppqq4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,12),
                    BuildJudgeArea(SensorType.B4,16),
                    BuildJudgeArea(SensorType.A3,25),
                    BuildJudgeArea(SensorType.A2,29),
                    BuildJudgeArea(SensorType.B1,35),
                    BuildJudgeArea(SensorType.C,40),
                    BuildJudgeArea(SensorType.B4,44),
                    BuildJudgeArea(SensorType.A4,49,true,true),
                },
                Const = 0.06976f
            },
            new SlideTable()
            {
                Name = "ppqq5",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,12),
                    BuildJudgeArea(SensorType.B4,16),
                    BuildJudgeArea(SensorType.A3,25),
                    BuildJudgeArea(SensorType.A2,29),
                    BuildJudgeArea(SensorType.B1,35),
                    BuildJudgeArea(SensorType.C,40),
                    BuildJudgeArea(SensorType.B5,44),
                    BuildJudgeArea(SensorType.A5,49,true,true),
                },
                Const = 0.06976f
            },
            new SlideTable()
            {
                Name = "ppqq6",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,12),
                    BuildJudgeArea(SensorType.B4,16),
                    BuildJudgeArea(SensorType.A3,25),
                    BuildJudgeArea(SensorType.A2,28),
                    BuildJudgeArea(SensorType.B1,34),
                    BuildJudgeArea(new SensorType[] { SensorType.C,SensorType.B8 },38),
                    BuildJudgeArea(new SensorType[] { SensorType.B7,SensorType.B6 },41),
                    BuildJudgeArea(SensorType.A6,48,true,true),
                },
                Const = 0.07107f
            },
            new SlideTable()
            {
                Name = "ppqq7",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,13),
                    BuildJudgeArea(SensorType.B4,17),
                    BuildJudgeArea(SensorType.A3,27),
                    BuildJudgeArea(SensorType.A2,31),
                    BuildJudgeArea(SensorType.B1,37),
                    BuildJudgeArea(SensorType.B8,41),
                    BuildJudgeArea(SensorType.A7,46,true,true),
                },
                Const = 0.08106f
            },
            new SlideTable()
            {
                Name = "ppqq8",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,7),
                    BuildJudgeArea(SensorType.C,12),
                    BuildJudgeArea(SensorType.B4,16),
                    BuildJudgeArea(SensorType.A3,25),
                    BuildJudgeArea(SensorType.A2,29),
                    BuildJudgeArea(new SensorType[] { SensorType.B1,SensorType.A1 },35),
                    BuildJudgeArea(SensorType.A8,41,true,true),
                },
                Const = 0.06027f
            },
            new SlideTable()
            {
                Name = "L2",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[] { SensorType.B8,SensorType.A8 },7,false),
                    BuildJudgeArea(SensorType.A7,15),
                    BuildJudgeArea(SensorType.B8,21),
                    BuildJudgeArea(SensorType.A1,26),
                    BuildJudgeArea(SensorType.A2,32,true,true),
                },
                Const = 0.09482f
            },
            new SlideTable()
            {
                Name = "L3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[] { SensorType.B8,SensorType.A8 },8,false),
                    BuildJudgeArea(SensorType.A7,17),
                    BuildJudgeArea(SensorType.B7,22),
                    BuildJudgeArea(SensorType.C,26),
                    BuildJudgeArea(SensorType.B3,29),
                    BuildJudgeArea(SensorType.A3,34,true,true),
                },
                Const = 0.09546f
            },
            new SlideTable()
            {
                Name = "L4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[] { SensorType.B8,SensorType.A8 },8,false),
                    BuildJudgeArea(SensorType.A7,17),
                    BuildJudgeArea(SensorType.B6,22),
                    BuildJudgeArea(SensorType.B5,26),
                    BuildJudgeArea(SensorType.A4,32,true,true),
                },
                Const = 0.10176f
            },
            new SlideTable()
            {
                Name = "L5",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[] { SensorType.B8,SensorType.A8 },8,false),
                    BuildJudgeArea(SensorType.A7,16),
                    BuildJudgeArea(new SensorType[] { SensorType.B6,SensorType.A6 },22,false),
                    BuildJudgeArea(SensorType.A5,28,true,true),
                },
                Const = 0.09598f
            },
            new SlideTable()
            {
                Name = "s",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,11),
                    BuildJudgeArea(SensorType.C,17),
                    BuildJudgeArea(SensorType.B3,21),
                    BuildJudgeArea(SensorType.B4,24),
                    BuildJudgeArea(SensorType.A5,30,true,true),
                },
                Const = 0.10546f
            },
            new SlideTable()
            {
                Name = "pq1",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,11),
                    BuildJudgeArea(SensorType.B6,14),
                    BuildJudgeArea(SensorType.B5,17),
                    BuildJudgeArea(SensorType.B4,21),
                    BuildJudgeArea(SensorType.B3,24),
                    BuildJudgeArea(SensorType.B2,27),
                    BuildJudgeArea(SensorType.A1,33,true,true),
                },
                Const = 0.09215f
            },
            new SlideTable()
            {
                Name = "pq2",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,11),
                    BuildJudgeArea(SensorType.B6,14),
                    BuildJudgeArea(SensorType.B5,18),
                    BuildJudgeArea(SensorType.B4,21),
                    BuildJudgeArea(SensorType.B3,24),
                    BuildJudgeArea(SensorType.A2,30,true,true),
                },
                Const = 0.10208f
            },
            new SlideTable()
            {
                Name = "pq3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,9),
                    BuildJudgeArea(SensorType.B7,12),
                    BuildJudgeArea(SensorType.B6,16),
                    BuildJudgeArea(SensorType.B5,19),
                    BuildJudgeArea(SensorType.B4,23),
                    BuildJudgeArea(SensorType.A3,27,true,true),
                },
                Const = 0.12468f
            },
            new SlideTable()
            {
                Name = "pq4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,9),
                    BuildJudgeArea(SensorType.B7,13),
                    BuildJudgeArea(SensorType.B6,16),
                    BuildJudgeArea(SensorType.B5,20),
                    BuildJudgeArea(SensorType.A4,24,true,true),
                },
                Const = 0.14359f
            },
            new SlideTable()
            {
                Name = "pq5",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,9),
                    BuildJudgeArea(SensorType.B7,13),
                    BuildJudgeArea(SensorType.B6,17),
                    BuildJudgeArea(SensorType.A5,21,true,true),
                },
                Const = 0.16925f
            },
            new SlideTable()
            {
                Name = "pq6",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,11),
                    BuildJudgeArea(SensorType.B6,15),
                    BuildJudgeArea(SensorType.B5,18),
                    BuildJudgeArea(SensorType.B4,21),
                    BuildJudgeArea(SensorType.B3,25),
                    BuildJudgeArea(SensorType.B2,28),
                    BuildJudgeArea(SensorType.B1,31),
                    BuildJudgeArea(SensorType.B8,35),
                    BuildJudgeArea(SensorType.B7,38),
                    BuildJudgeArea(SensorType.A6,42,true,true),
                },
                Const = 0.07518f
            },
            new SlideTable()
            {
                Name = "pq7",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,12),
                    BuildJudgeArea(SensorType.B6,15),
                    BuildJudgeArea(SensorType.B5,18),
                    BuildJudgeArea(SensorType.B4,22),
                    BuildJudgeArea(SensorType.B3,25),
                    BuildJudgeArea(SensorType.B2,28),
                    BuildJudgeArea(SensorType.B1,32),
                    BuildJudgeArea(SensorType.B8,35),
                    BuildJudgeArea(SensorType.A7,40,true,true),
                },
                Const = 0.08167f
            },
            new SlideTable()
            {
                Name = "pq8",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,11),
                    BuildJudgeArea(SensorType.B6,14),
                    BuildJudgeArea(SensorType.B5,17),
                    BuildJudgeArea(SensorType.B4,21),
                    BuildJudgeArea(SensorType.B3,24),
                    BuildJudgeArea(SensorType.B2,27),
                    BuildJudgeArea(SensorType.B1,30),
                    BuildJudgeArea(SensorType.A8,36,true,true),
                },
                Const = 0.08398f
            },
        };
        public static JudgeArea[][] WIFISLIDE_JUDGE_QUEUE => new JudgeArea[][]
        {
            new JudgeArea[] // L
            {
                BuildJudgeArea(SensorType.A1,0),
                BuildJudgeArea(SensorType.B8,2),
                BuildJudgeArea(SensorType.B7,4),
                BuildJudgeArea(new SensorType[]{ SensorType.A6 , SensorType.D6 },7,true,true)
            },
            new JudgeArea[] // Center
            {
                BuildJudgeArea(SensorType.A1,0),
                BuildJudgeArea(SensorType.B1,2),
                BuildJudgeArea(SensorType.C,4),
                BuildJudgeArea(new SensorType[]{ SensorType.A5 , SensorType.B5 },7,true,true)
            },
            new JudgeArea[] // R
            {
                BuildJudgeArea(SensorType.A1,0),
                BuildJudgeArea(SensorType.B2,2),
                BuildJudgeArea(SensorType.B3,4),
                BuildJudgeArea(new SensorType[]{ SensorType.A4 , SensorType.D5 },7,true,true)
            }
        };
        public static JudgeArea[][] WIFISLIDE_JUDGE_QUEUE_CLASSIC => new JudgeArea[][]
        {
            new JudgeArea[] // L
            {
                BuildJudgeArea(SensorType.A1,0),
                BuildJudgeArea(SensorType.B8,2),
                BuildJudgeArea(SensorType.B7,4),
                BuildJudgeArea(new SensorType[]{ SensorType.A6 , SensorType.D6 },7,true,true)
            },
            new JudgeArea[] // Center
            {
                BuildJudgeArea(SensorType.A1,0),
                BuildJudgeArea(SensorType.B1,2),
                BuildJudgeArea(SensorType.C,7,true,false),
            },
            new JudgeArea[] // R
            {
                BuildJudgeArea(SensorType.A1,0),
                BuildJudgeArea(SensorType.B2,2),
                BuildJudgeArea(SensorType.B3,4),
                BuildJudgeArea(new SensorType[]{ SensorType.A4 , SensorType.D5 },7,true,true)
            }
        };
        public static SlideTable? FindTableByName(string prefabName)
        {
            var result = SLIDE_TABLES.Find(x => x.Name == prefabName);
            var clone = result.Clone();
            return clone;
        }
        public static JudgeArea[][] GetWifiTable(int startPos)
        {
            List<JudgeArea[]> queue = new();
            var raw = MajInstances.Setting.Judge.Mode == JudgeMode.Modern ? WIFISLIDE_JUDGE_QUEUE : WIFISLIDE_JUDGE_QUEUE_CLASSIC;
            foreach (var line in raw)
            {
                List<JudgeArea> rows = new();
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
        static JudgeArea BuildJudgeArea(SensorType type, int barIndex, bool canSkip = true, bool isLast = false)
        {
            var obj = new JudgeArea(new Dictionary<SensorType, bool>
                      {
                          { type, isLast}
                      }, barIndex);
            obj.CanSkip = canSkip;
            return obj;
        }
        static JudgeArea BuildJudgeArea(SensorType[] type, int barIndex, bool canSkip = true, bool isLast = false)
        {
            var table = new Dictionary<SensorType, bool>();
            foreach (var sensorType in type)
                table.Add(sensorType, isLast);

            var obj = new JudgeArea(table, barIndex);
            obj.CanSkip = canSkip;
            return obj;
        }
    }
}
