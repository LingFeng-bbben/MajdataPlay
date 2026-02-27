using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Touch;
public interface ITouchHoldGroupInfoProvider : ITouchGroupInfoProvider
{
    public TouchHoldGroup? TouchHoldGroupInfo { get; set; }
}
