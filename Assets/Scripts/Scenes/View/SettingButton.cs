using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.View
{
    internal class SettingButton: MajComponent
    {
        public void OnClick()
        {
            MajInstances.SceneSwitcher.SwitchScene("Setting");
        }
    }
}
