using UnityEngine.Scripting;

namespace MajdataPlay.Settings
{
    [Preserve]
    public enum EnforceGameFailureCondition
    {
        Disabled,
        TrackSkip_S,
        Retry_S,
        TrackSkip_SS,
        Retry_SS,
        TrackSkip_SSS,
        Retry_SSS,
        TrackSkip_SSSPlus,
        Retry_SSSPlus,
        TrackSkip_Best,
        Retry_Best,
        TrackSkip_FC,
        Retry_FC,
        TrackSkip_AP,
        Retry_AP
    }
}
