using System;

namespace MychIO.Connection
{
    public abstract partial class Connection : IConnection
    {
        public static ConnectionType GetConnectionType()
        {
            throw new NotImplementedException("Error GetConnectionType method not overwitten in base class");
        }

    }

}