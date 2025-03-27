namespace MajdataPlay.Types
{
    internal interface IRecordHelper
    {
        public void StartRecord();
        public void StopRecord();
        public bool Recording { get; set; }
        public bool Connected { get; set; }
    }
}
