using MajdataPlay.Buffers;
using MajdataPlay.Scenes.Game.Notes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Scenes.Game.Buffers
{
    public sealed class NoteInfo
    {
        public IStateful<NoteStatus>? Component { get; init; }
        public bool IsPreUpdatable { get; init; }
        public bool IsUpdatable { get; init; }
        public bool IsFixedUpdatable { get; init; }
        public bool IsLateUpdatable { get; init; }
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _onUpdateFunctions is not null ||
                       _onFixedUpdateFunctions is not null ||
                       _onLateUpdateFunctions is not null;
            }
        }
        public NoteStatus State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Component?.State ?? NoteStatus.End;
            }
        }

        IMajComponent? _component;

        readonly PlayerLoopFunction[] _onPreUpdateFunctions = Array.Empty<PlayerLoopFunction>();
        readonly PlayerLoopFunction[] _onUpdateFunctions = Array.Empty<PlayerLoopFunction>();
        readonly PlayerLoopFunction[] _onFixedUpdateFunctions = Array.Empty<PlayerLoopFunction>();
        readonly PlayerLoopFunction[] _onLateUpdateFunctions = Array.Empty<PlayerLoopFunction>();

        delegate void PlayerLoopFunction();
        public NoteInfo(IStateful<NoteStatus> component)
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
            var delegateType = typeof(PlayerLoopFunction);
            var onPreUpdateFunctions = ArrayPool<PlayerLoopFunction>.Shared.Rent(methods.Length);
            var onUpdateFunctions = ArrayPool<PlayerLoopFunction>.Shared.Rent(methods.Length);
            var onLateUpdateFunctions = ArrayPool<PlayerLoopFunction>.Shared.Rent(methods.Length);
            var onFixedUpdateFunctions = ArrayPool<PlayerLoopFunction>.Shared.Rent(methods.Length);

            var onPreUpdateFuncCount = 0;
            var onUpdateFuncCount = 0;
            var onLateUpdateFuncCount = 0;
            var onFixedUpdateFuncCount = 0;

            foreach (var method in methods)
            {
                if (method.GetParameters().Length != 0)
                    continue;
                var attributes = method.GetCustomAttributes<PlayerLoopFunctionAttribute>();
                if (attributes.Count() == 0)
                {
                    continue;
                }
                var timing = attributes.Max(x => x.Timing);

                if ((timing & LoopTiming.PreUpdate) == LoopTiming.PreUpdate)
                {
                    var func = (PlayerLoopFunction?)method.CreateDelegate(delegateType, component);
                    if (func is not null)
                    {
                        onPreUpdateFunctions[onPreUpdateFuncCount++] = func;
                    }
                }
                if ((timing & LoopTiming.Update) == LoopTiming.Update)
                {
                    var func = (PlayerLoopFunction?)method.CreateDelegate(delegateType, component);
                    if (func is not null)
                    {
                        onUpdateFunctions[onUpdateFuncCount++] = func;
                    }
                }
                if ((timing & LoopTiming.LateUpdate) == LoopTiming.LateUpdate)
                {
                    var func = (PlayerLoopFunction?)method.CreateDelegate(delegateType, component);
                    if (func is not null)
                    {
                        onLateUpdateFunctions[onLateUpdateFuncCount++] = func;
                    }
                }
                if ((timing & LoopTiming.FixedUpdate) == LoopTiming.FixedUpdate)
                {
                    var func = (PlayerLoopFunction?)method.CreateDelegate(delegateType, component);
                    if (func is not null)
                    {
                        onFixedUpdateFunctions[onFixedUpdateFuncCount++] = func;
                    }
                }

            }
            IsPreUpdatable = onPreUpdateFuncCount != 0;
            IsUpdatable = onUpdateFuncCount != 0;
            IsFixedUpdatable = onFixedUpdateFuncCount != 0;
            IsLateUpdatable = onLateUpdateFuncCount != 0;

            if (IsPreUpdatable)
            {
                _onPreUpdateFunctions = new PlayerLoopFunction[onPreUpdateFuncCount];
                Array.Copy(onPreUpdateFunctions, _onPreUpdateFunctions, onPreUpdateFuncCount);
            }
            if (IsUpdatable)
            {
                _onUpdateFunctions = new PlayerLoopFunction[onUpdateFuncCount];
                Array.Copy(onUpdateFunctions, _onUpdateFunctions, onUpdateFuncCount);
            }
            if (IsFixedUpdatable)
            {
                _onFixedUpdateFunctions = new PlayerLoopFunction[onFixedUpdateFuncCount];
                Array.Copy(onFixedUpdateFunctions, _onFixedUpdateFunctions, onFixedUpdateFuncCount);
            }
            if (IsLateUpdatable)
            {
                _onLateUpdateFunctions = new PlayerLoopFunction[onLateUpdateFuncCount];
                Array.Copy(onLateUpdateFunctions, _onLateUpdateFunctions, onLateUpdateFuncCount);
            }
            ArrayPool<PlayerLoopFunction>.Shared.Return(onPreUpdateFunctions);
            ArrayPool<PlayerLoopFunction>.Shared.Return(onUpdateFunctions);
            ArrayPool<PlayerLoopFunction>.Shared.Return(onFixedUpdateFunctions);
            ArrayPool<PlayerLoopFunction>.Shared.Return(onLateUpdateFunctions);

            _component = component as IMajComponent;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnPreUpdate()
        {
            var funcCount = _onPreUpdateFunctions.Length;
            if (funcCount == 0)
            {
                return;
            }
            if (IsExecutable())
            {
                for (var i = 0; i < funcCount; i++)
                {
                    var func = _onPreUpdateFunctions[i];
                    func();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate()
        {
            var funcCount = _onUpdateFunctions.Length;
            if (funcCount == 0)
            {
                return;
            }
            if (IsExecutable())
            {
                for (var i = 0; i < funcCount; i++)
                {
                    var func = _onUpdateFunctions[i];
                    func();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnLateUpdate()
        {
            var funcCount = _onLateUpdateFunctions.Length;
            if (funcCount == 0)
            {
                return;
            }
            if (IsExecutable())
            {
                for (var i = 0; i < funcCount; i++)
                {
                    var func = _onLateUpdateFunctions[i];
                    func();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnFixedUpdate()
        {
            var funcCount = _onFixedUpdateFunctions.Length;
            if (funcCount == 0)
            {
                return;
            }
            if (IsExecutable())
            {
                for (var i = 0; i < funcCount; i++)
                {
                    var func = _onFixedUpdateFunctions[i];
                    func();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExecutable()
        {
            return State is not (NoteStatus.Start or NoteStatus.End) &&
                   (_component?.Active ?? false);
        }
    }
}
