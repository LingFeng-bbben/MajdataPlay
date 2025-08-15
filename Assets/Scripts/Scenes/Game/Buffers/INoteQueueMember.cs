namespace MajdataPlay.Scenes.Game.Buffers
{
    internal interface INoteQueueMember<TMember> where TMember : NoteQueueInfo
    {
        TMember QueueInfo { get; }
    }
}
