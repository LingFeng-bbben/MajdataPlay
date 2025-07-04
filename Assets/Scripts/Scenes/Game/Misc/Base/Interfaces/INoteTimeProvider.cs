using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Game
{
    public interface INoteTimeProvider
    {
        float ThisFrameSec { get; }
    }
}
