
namespace MajdataPlay.Scenes.View.Types
{
    internal readonly struct MajWsRequestLoad
    {
        /*public string TrackB64 { get; set; }
        public string ImageB64 { get; set; }
        public string VideoB64 { get; set; }*/
        public string TrackPath { get; init; }
        public string ImagePath { get; init; }
        public string VideoPath { get; init; }
    }
}