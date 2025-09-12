using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.IO;
public interface ILedUpdateFunction
{
    Color CurrentColor { get; }

    void Update(float deltaMs);
    void Reset();
}
