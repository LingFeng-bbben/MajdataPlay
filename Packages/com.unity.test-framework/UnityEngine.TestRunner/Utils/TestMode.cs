using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Represents the different test modes that can be included in a test run.
    /// </summary>
    [Flags]
    public enum TestMode
    {
        /// <summary>
        /// No test modes included.
        /// </summary>
        None = 0,

        /// <summary>
        /// Edit mode tests are included.
        /// </summary>
        EditMode = 1 << 0,

        /// <summary>
        /// Play mode tests are included.
        /// </summary>
        PlayMode = 1 << 1,

        /// <summary>
        /// Player tests are included.
        /// </summary>
        Player = 1 << 2,
    }
}
