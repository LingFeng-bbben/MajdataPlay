using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.View.Types
{
    internal struct ViewSummary
    {
        public ViewStatus State { get; init; }
        public string ErrMsg { get; init; }
        public float Timeline { get; init; }
    }
}
