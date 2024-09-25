using MajdataPlay.Types;

namespace MajdataPlay.Interfaces
{
    public interface INotePool<TInfo, TMember> where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public int Capacity { get; }
        public void Update(float currentSec);
        public void Collect(IPoolableNote<TInfo, TMember> endNote);
        public void Destroy();
    }
}
