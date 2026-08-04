using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.i18n;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class NoTranslateAttribute : Attribute
{
}
