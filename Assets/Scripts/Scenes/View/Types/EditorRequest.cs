using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.View.Types
{
    internal readonly struct EditorRequest
    {
        public PlaybackMode Mode { get; init; }
        public float PlaybackSpeed { get; init; }
        public float StartAt { get; init; }
    }
}
