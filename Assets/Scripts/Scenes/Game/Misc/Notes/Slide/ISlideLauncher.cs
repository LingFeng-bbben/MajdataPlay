using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Slide
{
    public interface ISlideLauncher
    {
        public float RotateSpeed { get; set; }
        public bool IsDouble { get; set; }
        public bool IsNoHead { get; set; }
        public bool IsFakeStar { get; set; }
        public bool IsForceRotate { get; set; }
        public GameObject? SlideObject { get; set; }
    }
}
