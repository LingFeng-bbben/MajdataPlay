using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace MajdataPlay.Platform.Win32.IO
{
    public struct SerialPortOptions
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
    }
}
