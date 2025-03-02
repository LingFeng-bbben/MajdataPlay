namespace MajdataPlay.View.Types
{
    public partial class MajWsResponseBase
    {
        public MajWsResponseType responseType { get; set; }
        public object responseData { get; set; }
    }
    public enum MajWsResponseType
    {
        Error = 400,
        Ok = 200,
        PlayStarted = 201
    }
}