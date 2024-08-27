using MajdataPlay.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Types
{
    public class SlideTable
    {
        public string Name { get; init; }
        public JudgeArea[] JudgeQueue { get; init; }
        public float Const { get; init; }
        public void Mirror()
        {
            foreach(var item in JudgeQueue)
                item.Mirror(SensorType.A1);
        }
        public void SetDiff(int diff)
        {
            foreach (var item in JudgeQueue)
                item.SetDiff(diff);
        }
    }
}
