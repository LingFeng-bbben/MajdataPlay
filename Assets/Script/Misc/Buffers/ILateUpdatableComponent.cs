using MajdataPlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Buffers
{
    public interface ILateUpdatableComponent<TState> : IStateful<TState>
    {
        bool Active { get; }
        void ComponentLateUpdate();
    }
}
