using MajdataPlay.Game.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay
{
    public interface IRendererContainer
    {
        public RendererStatus RendererState { get; set; }
    }
}
