using MajdataPlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public abstract class ComponentInfo<TObject>
    {
        public TObject? Object { get; init; }
        public abstract bool IsUpdatable { get; }
        public abstract bool IsFixedUpdatable { get; }
        public abstract bool IsLateUpdatable { get; }
        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void FixedUpdate();
    }
}
