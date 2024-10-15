using MajdataPlay.Types;

namespace MajdataPlay.Buffers
{
    public interface INoteQueueMember<TMember> where TMember : NoteQueueInfo
    {
        TMember QueueInfo { get; }
    }
}
