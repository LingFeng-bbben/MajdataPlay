namespace MajdataPlay.Game.Buffers
{
    internal interface INoteQueueMember<TMember> where TMember : NoteQueueInfo
    {
        TMember QueueInfo { get; }
    }
}
