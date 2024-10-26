using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
    // Important class cannot have more than 1 constructor (see Device Factory)
    public abstract partial class Device<T1, T2, T3> : IDevice<T1, T2> where T1 : Enum where T3 : IConnectionProperties where T2 : Enum
    {
        protected const byte MOST_SIGNIFICANT_BIT = 0b10000000;
        protected const byte LEAST_SIGNIFICANT_BIT = 0b00000001;
        private string _id;
        public string Id
        {
            get => _id;
            set => _id = value;
        }

        protected readonly IOManager _manager;
        protected readonly IConnectionProperties _connectionProperties;
        protected IDictionary<T1, Action<T1, T2>> _inputSubscriptions;
        protected IConnection _connection;
        protected DeviceClassification _classification;

        protected Device(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        )
        {

            // pull base class static methods
            var defaultProperties = (IConnectionProperties)GetBaseClassStaticMethod("GetDefaultConnectionProperties", GetType()).Invoke(null, null);
            _classification = (DeviceClassification)GetBaseClassStaticMethod("GetDeviceClassification", GetType()).Invoke(null, null);

            // construct
            _inputSubscriptions = CreateTypedDictionary(inputSubscriptions);
            _connectionProperties = (null != connectionProperties) ?
                defaultProperties.UpdateProperties(connectionProperties) :
                defaultProperties;
            _connection = ConnectionFactory.GetConnection(this, _connectionProperties, manager);
            Id = _connectionProperties.Id;
            _manager = manager;
        }

        private void OnDestroy()
        {
            Task.Run(() =>
            {
                _connection.Disconnect();
            });
        }

        public IConnectionProperties GetConnectionProperties() => _connectionProperties;

        public void SetInputCallbacks(IDictionary<T1, Action<T1, T2>> inputSubscriptions)
        {
            _inputSubscriptions = inputSubscriptions;
        }

        public void AddInputCallback(T1 interactionZone, Action<T1, T2> callback)
        {
            _inputSubscriptions[interactionZone] = callback;
        }

        public async Task<IDevice> Connect()
        {
            await _connection.Connect();
            return (IDevice)this;
        }
        public async Task Disconnect()
        {
            await _connection.Disconnect();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public IConnection GetConnection()
        {
            return _connection;
        }

        public DeviceClassification GetClassification()
        {
            return _classification;
        }

        public bool CanConnect(IDevice device)
        {
            return _connection.CanConnect(device.GetConnection());
        }
        public abstract void ResetState();

        public abstract Task OnStartWrite();

        public abstract Task OnDisconnectWrite();

        Task IDevice<T1, T2>.SetInputCallbacks(IDictionary<T1, Action<T1, T2>> inputSubscriptions)
        {
            // To prevent side effects due to threading reading will be halted temporarily to load new callbacks
            StopReading();
            _inputSubscriptions = inputSubscriptions;
            StartReading();
            return Task.CompletedTask;
        }

        public bool IsReading()
        {
            return _connection.IsReading();
        }

        public void StopReading()
        {
            if (IsReading())
            {
                _connection.StopReading();
            }
        }

        public void StartReading()
        {
            if (!IsReading())
            {
                _connection.Read();
            }
        }

        // Making these methods virtual introduces overhead so
        // just implement them in all devices objects
        public abstract void ReadData(byte[] data);
        public abstract void ReadData(IntPtr data);

        public abstract Task Write(params Enum[] interactions);

        private static IDictionary<T1, Action<T1, T2>> CreateTypedDictionary(IDictionary<Enum, Action<Enum, Enum>> original)
        {
            var typedDictionary = new Dictionary<T1, Action<T1, T2>>();
            foreach (var kvp in original)
            {
                T1 key = (T1)kvp.Key;
                Action<T1, T2> value = (a1, a2) =>
                {
                    kvp.Value((T1)(object)a1, (T2)(object)a2);
                };
                typedDictionary[key] = value;
            }

            return typedDictionary;
        }
    }
}