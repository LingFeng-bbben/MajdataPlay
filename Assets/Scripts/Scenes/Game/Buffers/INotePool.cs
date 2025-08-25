using MajdataPlay.Buffers;
using System;

namespace MajdataPlay.Scenes.Game.Buffers
{
    internal interface INotePool<TInfo, TMember> : IObjectPool<IPoolableNote<TInfo, TMember>>, IDisposable
        where TInfo : NotePoolingInfo where TMember : NoteQueueInfo
    {
        public void OnPreUpdate(float currentSec);
    }
}
