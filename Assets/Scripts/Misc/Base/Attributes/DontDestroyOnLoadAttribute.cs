using System;
using System.Collections.Generic;
using System.Text;

namespace MajdataPlay
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class DontDestroyOnLoadAttribute : Attribute
    {

    }
}
