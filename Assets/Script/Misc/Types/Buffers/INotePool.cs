using MajdataPlay.Types;

namespace MajdataPlay.Buffers
{
    public interface INotePool<TInfo, TMember> where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public int Capacity { get; }
        public void Update(float currentSec);
        public void Collect(IPoolableNote<TInfo, TMember> endNote);
        public void Destroy();
    }
}
