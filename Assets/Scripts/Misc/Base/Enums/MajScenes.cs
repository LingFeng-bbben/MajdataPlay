using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    public enum MajScenes
    {
        Init,
        Title,
        Login,
        [ContainsCubismComponent] Calibrator,
        [ContainsCubismComponent] List,
        Game,
        [ContainsCubismComponent] Result,
        Setting,
        SortFind,
        TotalResult,
        [ContainsCubismComponent] Parctice,
        View,
        Test,
        Empty,
    }
}
