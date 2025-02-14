using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Buffers
{
    public class ComponentInfo<TComponent>
    {
        public TComponent? Component { get; init; }
        public bool IsUpdatable { get; init; }
        public bool IsFixedUpdatable { get; init; }
        public bool IsLateUpdatable { get; init; }

        protected readonly Action? _onUpdate = null;
        protected readonly Action? _onFixedUpdate = null;
        protected readonly Action? _onLateUpdate = null;
        public ComponentInfo(TComponent component)
        {
            if(component is null)
            {
                Component = default;
                IsUpdatable = false;
                IsFixedUpdatable = false;
                IsLateUpdatable = false;
                return;
            }
            var methods = Reflection<TComponent>.Methods.Span;
            var actionType = typeof(Action);
            var onUpdateMethod = methods.Find(x => x.Name == "OnUpdate" && x.GetParameters().Length == 0 && x.IsStatic == false);
            var onFixedUpdateMethod = methods.Find(x => x.Name == "OnFixedUpdate" && x.GetParameters().Length == 0 && x.IsStatic == false);
            var onLateUpdateMethod = methods.Find(x => x.Name == "OnLateUpdate" && x.GetParameters().Length == 0 && x.IsStatic == false);

            _onUpdate = (Action?)onUpdateMethod?.CreateDelegate(actionType, component);
            _onFixedUpdate = (Action?)onFixedUpdateMethod?.CreateDelegate(actionType, component);
            _onLateUpdate = (Action?)onLateUpdateMethod?.CreateDelegate(actionType, component);

            IsUpdatable = _onUpdate is not null;
            IsFixedUpdatable = _onFixedUpdate is not null;
            IsLateUpdatable = _onLateUpdate is not null;
        }
        public virtual void OnUpdate()
        {
            if (_onUpdate is null)
                return;
            _onUpdate();
        }
        public virtual void OnLateUpdate()
        {
            if (_onLateUpdate is null)
                return;
            _onLateUpdate();
        }
        public virtual void OnFixedUpdate()
        {
            if (_onFixedUpdate is null)
                return;
            _onFixedUpdate();
        }
    }
    public class ComponentInfo
    {
        public object? Component { get; init; }
        public bool IsUpdatable { get; init; }
        public bool IsFixedUpdatable { get; init; }
        public bool IsLateUpdatable { get; init; }

        protected readonly Action? _onUpdate = null;
        protected readonly Action? _onFixedUpdate = null;
        protected readonly Action? _onLateUpdate = null;
        public ComponentInfo(object component)
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
            var actionType = typeof(Action);
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var onUpdateMethod = componentType.GetMethod("OnUpdate", flags);
            var onFixedUpdateMethod = componentType.GetMethod("OnFixedUpdate", flags);
            var onLateUpdateMethod = componentType.GetMethod("OnLateUpdate", flags);

            _onUpdate = (Action?)onUpdateMethod?.CreateDelegate(actionType, component);
            _onFixedUpdate = (Action?)onFixedUpdateMethod?.CreateDelegate(actionType, component);
            _onLateUpdate = (Action?)onLateUpdateMethod?.CreateDelegate(actionType, component);

            IsUpdatable = _onUpdate is not null;
            IsFixedUpdatable = _onFixedUpdate is not null;
            IsLateUpdatable = _onLateUpdate is not null;
        }
        public virtual void OnUpdate()
        {
            if (_onUpdate is null)
                return;
            _onUpdate();
        }
        public virtual void OnLateUpdate()
        {
            if (_onLateUpdate is null)
                return;
            _onLateUpdate();
        }
        public virtual void OnFixedUpdate()
        {
            if (_onFixedUpdate is null)
                return;
            _onFixedUpdate();
        }
    }
}
