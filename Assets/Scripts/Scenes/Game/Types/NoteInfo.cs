using MajdataPlay.Buffers;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Game.Types
{
    public sealed class NoteInfo : ComponentInfo
    {
        public bool IsValid => _onUpdate is not null ||
                               _onFixedUpdate is not null ||
                               _onLateUpdate is not null;
        public NoteStatus State => _noteObj?.State ?? NoteStatus.End;

        IStateful<NoteStatus> _noteObj;
        IMajComponent? _component;

        public NoteInfo(IStateful<NoteStatus> noteObj) : base(noteObj)
        {
            _noteObj = noteObj;
            if(noteObj is IMajComponent component)
                _component = component;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnUpdate()
        {
            if (IsExecutable())
                base.OnUpdate();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnLateUpdate()
        {
            if (IsExecutable())
                base.OnLateUpdate();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnFixedUpdate()
        {
            if (IsExecutable())
                base.OnFixedUpdate();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExecutable()
        {
            return State is not (NoteStatus.Start or NoteStatus.End) &&
                   (_component?.Active ?? false);
        }
    }
}
