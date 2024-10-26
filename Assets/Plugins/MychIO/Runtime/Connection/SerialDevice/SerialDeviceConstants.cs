namespace MychIO.Connection.SerialDevice
{
    public enum BaudRate
    {

        Bd300 = 300,
        Bd600 = 600,
        Bd1200 = 1200,
        Bd2400 = 2400,
        Bd4800 = 4800,
        Bd9600 = 9600,
        Bd14400 = 14400,
        Bd19200 = 19200,
        Bd28800 = 28800,
        Bd38400 = 38400,
        Bd57600 = 57600,
        Bd115200 = 115200,
        Bd230400 = 230400,
        Bd460800 = 460800,
        Bd921600 = 921600
    }

    public enum Parity
    {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4
    }

    // None is not included it is not supported
    public enum StopBits
    {
        One = 1,
        Two = 2,
        OnePointFive = 3
    }

    public enum Handshake
    {
        None = 0,
        XOnXOff = 1,
        RequestToSend = 2,
        RequestToSendXOnXOff = 3,
    }

    public enum DataBits
    {
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8
    }

    public enum SerialPortError
    {
        FramingError,
        OverrunError,
        ReceiveBufferOverflow,
        ReceiveParityError,
        TransmitBufferFull
    }

}
