using System;

namespace MajdataPlay.IO
{
    public class TouchAreaEventArgs : EventArgs
    {
        public TouchAreaEventArgs(string area, int index)
        {
            AreaName = area;
            AreaIndex = index;
        }

        public string AreaName { get; set; }
        public int AreaIndex { get; set; }
    }
}