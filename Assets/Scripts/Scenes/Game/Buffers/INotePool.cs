using MajdataPlay.Buffers;

namespace MajdataPlay.Game.Buffers
{
    internal interface INotePool<TInfo, TMember> : IObjectPool<IPoolableNote<TInfo, TMember>>
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void OnPreUpdate(float currentSec);
        public void Destroy();
    }
}
