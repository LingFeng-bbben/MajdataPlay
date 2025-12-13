using UnityEngine.Scripting;

namespace MajdataPlay.Settings
{
    [Preserve]
    public enum JudgeDisplayOption
    {
        /// <summary>
        /// CriticalPerfect, Perfect, Great, Good
        /// </summary>
        All,
        /// <summary>
        /// Perfect, Great, Good
        /// </summary>
        BelowCP,
        /// <summary>
        /// Great, Good
        /// </summary>
        BelowP,
        /// <summary>
        /// Good
        /// </summary>
        BelowGR,
        /// <summary>
        /// Miss
        /// </summary>
        MissOnly,
        /// <summary>
        /// None
        /// </summary>
        Disable,
    }
}
