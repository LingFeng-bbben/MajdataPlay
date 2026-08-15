using System;

namespace UnityEditor.TestTools
{
    /// <summary>
    /// An interface for a callback modifying the <see cref="BuildPlayerOptions"/> when building a player for running tests in the runtime.
    /// </summary>
    public interface ITestPlayerBuildModifier
    {
        /// <summary>
        /// A callback to modify the <see cref="BuildPlayerOptions"/> when building a player for test run. Return the modified version of the provided build options.
        /// </summary>
        /// <param name="playerOptions">The unmodified BuildPlayerOptions.</param>
        /// <returns>The modified BuildPlayerOptions.</returns>
        BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions);

#if UNITY_6000_1_OR_NEWER
        /// <summary>
        /// A callback to modify the <see cref="BuildPlayerWithProfileOptions"/> when building a player for test run. When called a
        /// <see cref="UnityEditor.Build.Profile.BuildProfile"/> should be specified. Build pipeline will use the <see cref="BuildPlayerWithProfileOptions"/>
        /// instead of <see cref="BuildPlayerOptions"/> when a build profile is given.
        /// </summary>
        /// <param name="playerOptions">The unmodified BuildPlayerWithProfileOptions, does not specify a build profile.</param>
        /// <returns>The modified BuildPlayerWithProfileOptions. If a build profile is given, then <see cref="BuildPlayerWithProfileOptions"> is
        /// used to build the player.</returns>
        BuildPlayerWithProfileOptions ModifyOptions(BuildPlayerWithProfileOptions playerOptions) { return playerOptions; }
#endif
    }
}
