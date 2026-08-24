namespace MajdataPlay.Scenes.Game.Parsing.Slide
{
    /// <summary>
    /// <p>用来控制箭头对齐的标志</p>
    /// </summary>
    public enum SlideParseMarker
    {
        None = 0,

        /// <summary>
        /// 调整箭头间距，以保证本段结束时箭头位置恰好对齐本段终点
        /// </summary>
        SmoothAlign,

        /// <summary>
        /// 不调整箭头间距，但本段结束时把箭头位置强制设为本段终点
        /// </summary>
        ForceAlign,
    }
}
