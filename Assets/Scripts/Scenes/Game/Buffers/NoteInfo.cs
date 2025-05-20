using MajdataPlay.Buffers;
using MajdataPlay.Game.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Game.Buffers
{
    public sealed class NoteInfo : ComponentInfo
    {
        public bool IsValid => _onUpdateFunctions is not null ||
                               _onFixedUpdateFunctions is not null ||
                               _onLateUpdateFunctions is not null;
        public NoteStatus State => _noteObj?.State ?? NoteStatus.End;

        IStateful<NoteStatus> _noteObj;
        IMajComponent? _component;

        public NoteInfo(IStateful<NoteStatus> noteObj) : base(noteObj)
        {
            _noteObj = noteObj;
            if (noteObj is IMajComponent component)
                _component = component;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnPreUpdate()
        {
            var funcCount = _onPreUpdateFunctions.Length;
            if (funcCount == 0)
                return;
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
        public override void OnUpdate()
        {
            var funcCount = _onUpdateFunctions.Length;
            if (funcCount == 0)
                return;
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
        public override void OnLateUpdate()
        {
            var funcCount = _onLateUpdateFunctions.Length;
            if (funcCount == 0)
                return;
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
        public override void OnFixedUpdate()
        {
            var funcCount = _onFixedUpdateFunctions.Length;
            if (funcCount == 0)
                return;
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
