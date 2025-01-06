using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Interfaces
{
    public interface IRendererContainer
    {
        public RendererStatus RendererState { get; set; }
    }
}
