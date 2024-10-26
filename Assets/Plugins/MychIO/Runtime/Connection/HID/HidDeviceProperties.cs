using System;

namespace MychIO.Connection.HidDevice
{
    public class HidDeviceProperties : ConnectionProperties
    {
        public int ProductId { get; set; }
        public int VendorId { get; set; }
        public int BufferSize { get; set; }
        public int LeftBytesToTruncate { get; set; }
        public int BytesToRead { get; set; }
        public int PollingRateMs { get; set; }

        // Constructor that initializes all properties
        public HidDeviceProperties(
            int productId = 0x0021,
            int vendorId = 0x0CA3,
            int bufferSize = 64,
            int leftBytesToTruncate = 0,
            int bytesToRead = 64,
            int pollingRateMs = 0
        )
        {
            ProductId = productId;
            VendorId = vendorId;
            BufferSize = bufferSize;
            LeftBytesToTruncate = leftBytesToTruncate;
            BytesToRead = bytesToRead;
            pollingRateMs = PollingRateMs;
        }

        public HidDeviceProperties(HidDeviceProperties existing,
            int? productId = null,
            int? vendorId = null,
            int? bufferSize = null,
            int? leftBytesToTruncate = null,
            int? bytesToRead = null,
            int? pollingRateMs = null
        )
        {
            ProductId = productId ?? existing.ProductId;
            VendorId = vendorId ?? existing.VendorId;
            BufferSize = bufferSize ?? existing.BufferSize;
            LeftBytesToTruncate = leftBytesToTruncate ?? existing.LeftBytesToTruncate;
            BytesToRead = bytesToRead ?? existing.BytesToRead;
            PollingRateMs = pollingRateMs ?? existing.PollingRateMs;
        }


        public override ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
    }
}