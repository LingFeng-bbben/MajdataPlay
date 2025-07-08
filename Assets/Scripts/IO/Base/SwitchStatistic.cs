namespace MajdataPlay.IO
{
    internal struct SwitchStatistic
    {
        public bool IsPressed { get; set; }
        public bool IsClicked { get; set; }
        public bool IsReleased { get; set; }
        public float PressTime { get; set; }
        public bool IsClickEventUsed { get; set; }
    }
}