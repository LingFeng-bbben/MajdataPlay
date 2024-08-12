using System;

namespace MajdataPlay.IO
{
    public class ButtonEventArgs : EventArgs
    {
        public ButtonEventArgs(int index)
        {
            ButtonIndex = index;
        }

        public int ButtonIndex { get; set; }
    }
}