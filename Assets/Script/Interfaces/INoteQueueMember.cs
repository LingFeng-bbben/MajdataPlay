using MajdataPlay.Types;

namespace MajdataPlay.Interfaces
{
    public interface INoteQueueMember<T> where T: NoteQueueInfo
    {
        T QueueInfo { get; }
    }
}
