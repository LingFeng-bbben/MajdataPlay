using System;

namespace MajdataPlay.Types
{
    internal interface IRecordHelper : IDisposable
    {
        public void StartRecord();
        public void StopRecord();
        public bool Recording { get; set; }
        public bool Connected { get; set; }
    }
}
