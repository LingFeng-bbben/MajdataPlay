using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;

namespace MajdataPlay.Buffers
{
    public interface IPoolableNote<TInfo, TMember> : IEndableNote ,IStatefulNote, INoteQueueMember<TMember>, IGameObjectProvider
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void Initialize(TInfo poolingInfo);
    }
}
