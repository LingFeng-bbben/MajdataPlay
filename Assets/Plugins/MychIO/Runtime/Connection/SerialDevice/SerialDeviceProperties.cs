namespace MychIO.Connection.SerialDevice
{
    public class SerialDeviceProperties : ConnectionProperties
    {
        public const int DEFAULT_WRITE_TIMEOUT_MS = 1000;

        public string ComPortNumber { get; set; }
        public int PollTimeoutMs { get; set; }
        public int BufferByteLength { get; set; }
        public int WriteTimeoutMS { get; set; }
        public int PortNumber { get; set; }
        public BaudRate BaudRate { get; set; }
        public StopBits StopBit { get; set; }
        public Parity ParityBit { get; set; }
        public DataBits DataBits { get; set; }
        public Handshake Handshake { get; set; }
        public bool Dtr { get; set; }
        public bool Rts { get; set; }

        // Constructor that initializes all properties
        public SerialDeviceProperties(
            string comPortNumber = "COM21",
            int pollingRateMs = 100,
            int bufferByteLength = 9,
            int writeTimeoutMS = DEFAULT_WRITE_TIMEOUT_MS,
            int portNumber = 0,
            BaudRate baudRate = BaudRate.Bd9600,
            StopBits stopBit = StopBits.One,
            Parity parityBit = Parity.None,
            DataBits dataBits = DataBits.Eight,
            Handshake handshake = Handshake.None,
            bool dtr = false,
            bool rts = false
        )
        {
            ComPortNumber = comPortNumber;
            PollTimeoutMs = pollingRateMs;
            BufferByteLength = bufferByteLength;
            WriteTimeoutMS = writeTimeoutMS;
            PortNumber = portNumber;
            BaudRate = baudRate;
            StopBit = stopBit;
            ParityBit = parityBit;
            DataBits = dataBits;
            Handshake = handshake;
            Dtr = dtr;
            Rts = rts;
            PopulatePropertiesFromFields();
        }

        public SerialDeviceProperties(
            SerialDeviceProperties existing,
            string comPortNumber = null,
            int? pollingRateMs = null,
            int? bufferByteLength = null,
            int? writeTimeoutMS = null,
            int? portNumber = null,
            BaudRate? baudRate = null,
            StopBits? stopBit = null,
            Parity? parityBit = null,
            DataBits? dataBits = null,
            Handshake? handshake = null,
            bool? dtr = null,
            bool? rts = null
        )
        {
            ComPortNumber = comPortNumber ?? existing.ComPortNumber;
            PollTimeoutMs = pollingRateMs ?? existing.PollTimeoutMs;
            BufferByteLength = bufferByteLength ?? existing.BufferByteLength;
            WriteTimeoutMS = writeTimeoutMS ?? existing.WriteTimeoutMS;
            PortNumber = portNumber ?? existing.PortNumber;
            BaudRate = baudRate ?? existing.BaudRate;
            StopBit = stopBit ?? existing.StopBit;
            ParityBit = parityBit ?? existing.ParityBit;
            DataBits = dataBits ?? existing.DataBits;
            Handshake = handshake ?? existing.Handshake;
            Dtr = dtr ?? existing.Dtr;
            Rts = rts ?? existing.Rts;
        }

        public override ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
    }
}