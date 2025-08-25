using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.Game
{
    public enum GamePlayStatus
    {
        Start,
        Loading,
        Running,
        Blocking,
        WaitForEnd,
        Ended
    }
}
