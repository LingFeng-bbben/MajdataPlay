using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public static class SlideTables
    {
        public static SlideTable[] SLIDE_TABLES = new SlideTable[]
        {
            new SlideTable()
            {
                Name = "circle2",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,7,true,true)
                }
            },
            new SlideTable()
            {
                Name = "circle3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.A2,11,false),
                    BuildJudgeArea(SensorType.A3,15,true,true)
                }
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
                }
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
                }
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
                }
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
                }
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
                }
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
                }
            },
            new SlideTable()
            {
                Name = "Line3",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[]{SensorType.A2,SensorType.B2 },8,false),
                    BuildJudgeArea(SensorType.A3,13,true,true)
                }
            },
            new SlideTable()
            {
                Name = "Line4",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B2,8),
                    BuildJudgeArea(SensorType.B3,12),
                    BuildJudgeArea(SensorType.A4,18,true,true)
                }
            },
            new SlideTable()
            {
                Name = "Line5",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B1,6),
                    BuildJudgeArea(SensorType.C,11),
                    BuildJudgeArea(SensorType.B5,15),
                    BuildJudgeArea(SensorType.A5,19,true,true)
                }
            },
            new SlideTable()
            {
                Name = "Line6",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,3),
                    BuildJudgeArea(SensorType.B8,8),
                    BuildJudgeArea(SensorType.B7,12),
                    BuildJudgeArea(SensorType.A6,18,true,true)
                }
            },
            new SlideTable()
            {
                Name = "Line7",
                JudgeQueue = new JudgeArea[]
                {
                    BuildJudgeArea(SensorType.A1,2),
                    BuildJudgeArea(new SensorType[]{SensorType.A8,SensorType.B8 },8,false),
                    BuildJudgeArea(SensorType.A7,13,true,true)
                }
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
                }
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
                }
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
                }
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
                }
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
                }
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
                }
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
                }
            },
        };
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
