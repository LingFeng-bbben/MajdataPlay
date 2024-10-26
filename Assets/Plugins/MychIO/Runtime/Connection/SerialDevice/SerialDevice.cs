using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;

namespace MychIO.Connection.SerialDevice
{
    public class SerialDeviceConnection : Connection
    {

        private SerialPort _serialPort;
        private int _pollTimeoutMs;
        private int _bufferByteLength;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public SerialDeviceConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        { }

        private void OnDestroy()
        {
            Task.Run(async () =>
            {
                await Disconnect();
            }).Wait();
        }

        public new static ConnectionType GetConnectionType() => ConnectionType.SerialDevice;

        public override Task Connect()
        {

            if (IsConnected())
            {
                // TODO: Set event here
                return Task.CompletedTask;
            }

            if (_connectionProperties is not SerialDeviceProperties)
            {
                throw new Exception("Invalid connection object passed to SerialDevice class");
            }
            var serialDeviceProperties = (SerialDeviceProperties)_connectionProperties;

            _pollTimeoutMs = serialDeviceProperties.PollTimeoutMs;
            _bufferByteLength = serialDeviceProperties.BufferByteLength;
            _serialPort = new SerialPort(serialDeviceProperties.ComPortNumber)
            {
                BaudRate = (int)serialDeviceProperties.BaudRate,
                Parity = (System.IO.Ports.Parity)serialDeviceProperties.ParityBit,
                StopBits = (System.IO.Ports.StopBits)serialDeviceProperties.StopBit,
                DataBits = (int)serialDeviceProperties.DataBits,
                WriteTimeout = 0 == serialDeviceProperties.WriteTimeoutMS ?
                    SerialPort.InfiniteTimeout :
                    serialDeviceProperties.WriteTimeoutMS,
                Handshake = (System.IO.Ports.Handshake)serialDeviceProperties.Handshake,
                RtsEnable = serialDeviceProperties.Rts,
                DtrEnable = serialDeviceProperties.Dtr
            };

            // Functionality to detect attach and detach device is not present on 
            // .net SerialPort could potentially add using this method:
            // https://stackoverflow.com/questions/13408476/detecting-when-a-serialport-gets-disconnected
            _serialPort.Open();

            Task.Run(async () =>
            {
                await _device.OnStartWrite();
                await RecieveData();
            });

            _manager.handleEvent(IOEventType.Attach, _device.GetClassification(), _device.GetType().ToString() + " Device connected");

            return Task.CompletedTask;
        }

        private async Task RecieveData()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    //int bytesRead = _serialPort.Read(buffer, 0, _bufferByteLength);
                    int bytesRead = _serialPort.BytesToRead;
                    byte[] buffer = new byte[bytesRead];
                    _serialPort.Read(buffer, 0, bytesRead);
                    if (bytesRead < _bufferByteLength) { continue; } // Handle case where not enough data to read
                    _device.ReadData(buffer);
                    await Task.Delay(_pollTimeoutMs, _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Nothing to do here event was sent to detach
            }
            catch (Exception e)
            {
                // Throw event here potentially in the future for now just disconnect
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + "device connection failed due to following exception: " + e);
                await Disconnect();
            }
        }

        private void StopReadPolling()
        {
            _cancellationTokenSource.Cancel();
        }

        public override async Task Disconnect()
        {
            _device.ResetState();
            if (IsReading())
            {
                StopReadPolling();
            }
            if (IsConnected())
            {
                await _device.OnDisconnectWrite();
                _serialPort.Close();
            }
            _serialPort = null;
            _manager.handleEvent(IOEventType.Detach, _device.GetClassification(), _device.GetType().ToString() + "device disconnected");
        }

        public override bool IsConnected()
        {
            return _serialPort?.IsOpen ?? false;
        }

        public async override Task Write(byte[] data)
        {
            await _serialPort.BaseStream.WriteAsync(data, 0, data.Length);
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            return !(connectionProperties is SerialDeviceProperties) ||
             ((SerialDeviceProperties)connectionProperties).ComPortNumber !=
              ((SerialDeviceProperties)_connectionProperties).ComPortNumber;
        }

        public override bool IsReading()
        {
            return !_cancellationTokenSource.Token.IsCancellationRequested;
        }

        public override void Read()
        {
            if (!IsReading())
            {
                _cancellationTokenSource.Dispose(); // Dispose the old one if it's not null
                _cancellationTokenSource = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    await _device.OnStartWrite();
                    await RecieveData();
                });
            }
        }

        public override void StopReading()
        {
            StopReadPolling();
        }
    }

}