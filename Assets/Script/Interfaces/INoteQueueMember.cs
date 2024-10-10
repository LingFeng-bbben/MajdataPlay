using MajdataPlay.Types;

namespace MajdataPlay.Interfaces
{
    public interface INoteQueueMember<TMember> where TMember: NoteQueueInfo
    {
        TMember QueueInfo { get; }
    }
}
