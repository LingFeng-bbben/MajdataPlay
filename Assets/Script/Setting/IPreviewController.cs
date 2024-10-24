using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Setting
{
    public interface IPreviewController
    {
        bool Active { get; set; }
        void Refresh();
    }
}
