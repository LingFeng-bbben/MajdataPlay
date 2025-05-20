using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public interface IMainCameraProvider
    {
        Camera MainCamera { get; }
    }
}
