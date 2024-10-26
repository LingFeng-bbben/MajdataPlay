using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Generic;

namespace MychIO.Device
{
    public interface IDevice : IIdentifier
    {
        void ResetState();
        void ReadData(byte[] data);
        void ReadData(IntPtr intPtr);
        Task OnStartWrite();
        Task OnDisconnectWrite();
        Task<IDevice> Connect();
        Task Disconnect();
        bool IsConnected();
        bool IsReading();
        void StopReading();
        void StartReading();
        bool CanConnect(IDevice device);
        IConnection GetConnection();
        Task Write(params Enum[] interactions);
        DeviceClassification GetClassification();
    }
    // Where T1 is the input type, e.g. A1, and T2 is the InputState
    interface IDevice<T1, T2> : IDevice where T1 : Enum where T2 : Enum
    {
        IConnectionProperties GetConnectionProperties();
        // Callback has parameters Input Type, and Interaction State (e.g. On/Off) respectively
        Task SetInputCallbacks(IDictionary<T1, Action<T1, T2>> inputSubscriptions);
        void AddInputCallback(T1 interactionZone, Action<T1, T2> callback);

    }
}