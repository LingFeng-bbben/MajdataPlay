using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay.Net;
[Preserve]
public enum NetAuthMethodOption
{
    None,
    Plain,
    QRCode,
    OAuth,
    Cookie
}
