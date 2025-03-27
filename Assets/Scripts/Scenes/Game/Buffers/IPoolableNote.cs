using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using System;

namespace MajdataPlay.Game.Buffers
{
    internal interface IPoolableNote<TInfo, TMember> : IStatefulNote, INoteQueueMember<TMember>, IGameObjectProvider
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void Initialize(TInfo poolingInfo);
    }
}
