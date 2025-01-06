using MajdataPlay.Types;

namespace MajdataPlay.Game.Buffers
{
    public interface INoteQueueMember<TMember> where TMember : NoteQueueInfo
    {
        TMember QueueInfo { get; }
    }
}
