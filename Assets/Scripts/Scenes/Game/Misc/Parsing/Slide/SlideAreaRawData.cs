namespace MajdataPlay.Scenes.Game.Parsing.Slide
{
    public readonly struct SlideAreaRawData
    {
        /// <summary>
        /// 判定段激活以后 slide 完成的长度
        /// </summary>
        public readonly double LengthAfterPush;

        /// <summary>
        /// 判定段完成以后 slide 完成的长度
        /// </summary>
        public readonly double LengthAfterFinish;

        public readonly int SensorA;
        public readonly int SensorB;

        public SlideAreaRawData(double lengthAfterPush, double lengthAfterFinish, int sensorA, int sensorB = -1)
        {
            LengthAfterPush = lengthAfterPush;
            LengthAfterFinish = lengthAfterFinish;
            SensorA = sensorA;
            SensorB = sensorB;
        }
    }
}
