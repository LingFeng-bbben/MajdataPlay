using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Settings
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class SettingVisualizationIgnoreAttribute : PreserveAttribute
    {
    }
}
