using MajdataPlay.IO;

namespace MajdataPlay.Scenes.Game.Notes.Slide
{
    public class SlideTable
    {
        public string Name { get; init; }
        public SlideArea[] JudgeQueue { get; init; }
        public float Const { get; init; }
        public void Mirror()
        {
            foreach (var item in JudgeQueue)
                item.Mirror(SensorArea.A1);
        }
        public void Diff(int diff)
        {
            foreach (var item in JudgeQueue)
                item.Diff(diff);
        }
    }
}
