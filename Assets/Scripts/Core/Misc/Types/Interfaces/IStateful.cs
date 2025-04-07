﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Interfaces
{
    public interface IStateful<T>
    {
        T State { get; }
    }
}
