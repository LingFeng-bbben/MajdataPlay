using MajdataPlay.IO;
using MajdataPlay.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Game
{
    internal readonly struct GameInputEventArgs
    {
        public SensorArea Area { get; init; }
        public SwitchStatus OldState { get; init; }
        public SwitchStatus State { get; init; }
        public bool IsButton { get; init; }
        public bool IsClick => OldState == SwitchStatus.Off && State == SwitchStatus.On;
        public Ref<bool> IsUsed { get; init; }
    }
}
