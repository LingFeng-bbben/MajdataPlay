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
                    new JudgeArea(new Dictionary<SensorType, bool>
                    {
                        { SensorType.A1, false}
                    },3),
                    new JudgeArea(new Dictionary<SensorType, bool>
                    {
                        { SensorType.A2, true}
                    },7)
                }
            },
            new SlideTable()
            {
                Name = "circle3",
                JudgeQueue = new JudgeArea[]
                {
                    new JudgeArea(new Dictionary<SensorType, bool>
                    {
                        { SensorType.A1, false}
                    },3),
                    new JudgeArea(new Dictionary<SensorType, bool>
                    {
                        { SensorType.A2, true}
                    },11)
                }
            },
            new SlideTable()
            {
                Name = "circle3",
                JudgeQueue = Test()
            }
        };
        static JudgeArea[] BuildJudgeArea()
        {

        }
    }
}
