using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.Game
{
    public interface INoteTimeProvider
    {
        float ThisFrameSec { get; }
        float FakeThisFrameSec { get; }
        List<Tuple<float, float>> SVList { get; } //time, sveloc
    }
}
