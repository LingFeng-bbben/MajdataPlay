using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.IO
{
    public interface IEventPublisher<T>
    {
        void AddSubscriber(T handler);
        void RemoveSubscriber(T handler);
    }
}
