
using MajdataPlay.Extensions;

namespace MajdataPlay.Types
{
    public class Area
    {
        public bool On = false;
        public bool Off = false;
        public SensorType Type;
        public bool IsLast = false;
        public bool IsFinished
        {
            get
            {
                if (IsLast)
                    return On;
                else
                    return On && Off;
            }
        }
        public void Judge(in SensorStatus status)
        {
            if (status == SensorStatus.Off)
            {
                if (On)
                    Off = true;
            }
            else
                On = true;
        }
        public void Reset()
        {
            On = false;
            Off = false;
        }
        public void Mirror(SensorType baseLine) => Type = Type.Mirror(baseLine);
        public void Diff(int diff) => Type = Type.Diff(diff);
    }
}
