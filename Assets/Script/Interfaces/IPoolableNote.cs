using MajdataPlay.Types;

namespace MajdataPlay.Interfaces
{
    public interface IPoolableNote<TInfo,TMember> :IStatefulNote, INoteQueueMember<TMember>, IGameObjectProvider
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void Initialize(TInfo poolingInfo);
        public void End(bool forceEnd);
    }
}
