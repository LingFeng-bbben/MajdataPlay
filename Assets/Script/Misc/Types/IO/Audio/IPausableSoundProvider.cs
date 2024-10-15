
namespace MajdataPlay.IO
{
    public interface IPausableSoundProvider : ISoundProvider
    {
        void Pause();
        void Stop();
    }
}
