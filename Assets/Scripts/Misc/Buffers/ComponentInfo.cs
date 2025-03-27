using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Buffers
{
    public abstract class ComponentInfo<TComponent>: ComponentInfo
    {
        public new TComponent? Component => (TComponent?)base.Component;
        public ComponentInfo(TComponent? component): base(component)
        {
            
        }
    }
    public abstract class ComponentInfo
    {
        public object? Component { get; init; }
        public bool IsUpdatable { get; init; }
        public bool IsFixedUpdatable { get; init; }
        public bool IsLateUpdatable { get; init; }

        protected readonly PlayerLoopEventMethod? _onUpdate = null;
        protected readonly PlayerLoopEventMethod? _onFixedUpdate = null;
        protected readonly PlayerLoopEventMethod? _onLateUpdate = null;

        protected delegate void PlayerLoopEventMethod();
        public ComponentInfo(object? component)
        {
            if (component is null)
            {
                Component = default;
                IsUpdatable = false;
                IsFixedUpdatable = false;
                IsLateUpdatable = false;
                return;
            }
            var componentType = component.GetType();
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var methods = componentType.GetMethods(flags);
            var delegateType = typeof(PlayerLoopEventMethod);
            foreach (var method in methods)
            {
                if (method.GetParameters().Length != 0)
                    continue;
                switch(method.Name)
                {
                    case "OnUpdate":
                        _onUpdate ??= (PlayerLoopEventMethod?)method.CreateDelegate(delegateType, component);
                        break;
                    case "OnFixedUpdate":
                        _onFixedUpdate ??= (PlayerLoopEventMethod?)method.CreateDelegate(delegateType, component);
                        break;
                    case "OnLateUpdate":
                        _onLateUpdate ??= (PlayerLoopEventMethod?)method.CreateDelegate(delegateType, component);
                        break;
                }
            }
            IsUpdatable = _onUpdate is not null;
            IsFixedUpdatable = _onFixedUpdate is not null;
            IsLateUpdatable = _onLateUpdate is not null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void OnUpdate();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void OnLateUpdate();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void OnFixedUpdate();
    }
}
