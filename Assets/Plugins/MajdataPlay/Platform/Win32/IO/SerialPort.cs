using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace MajdataPlay.Platform.Win32.IO
{
    public static class SerialPort
    {
        void Test()
        {
            System.IO.Ports.SerialPort
        }

        public static Stream Open(SerialPortOptions options)
        {
            if(string.IsNullOrEmpty(options.PortName))
            {
                throw new ArgumentException(nameof(options.PortName));
            }
        }
    }
}
