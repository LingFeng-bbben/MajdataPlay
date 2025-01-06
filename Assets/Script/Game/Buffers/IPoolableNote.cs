using MajdataPlay.Interfaces;
using System;

namespace MajdataPlay.Game.Buffers
{
    public interface IPoolableNote<TInfo, TMember> : IEndableNote, IStatefulNote, INoteQueueMember<TMember>, IGameObjectProvider
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void Initialize(TInfo poolingInfo);
    }
}
