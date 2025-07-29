using MajdataPlay.Scenes.Game.Notes;
using System;

namespace MajdataPlay.Scenes.Game.Buffers
{
    internal interface IPoolableNote<TInfo, TMember> : IStatefulNote, INoteQueueMember<TMember>, IGameObjectProvider
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void Initialize(TInfo poolingInfo);
    }
}
