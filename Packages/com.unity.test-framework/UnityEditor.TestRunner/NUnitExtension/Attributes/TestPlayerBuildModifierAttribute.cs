using System;

namespace UnityEditor.TestTools
{
    /// <summary>
    /// <para>
    /// You can use the ```TestPlayerBuildModifier``` attribute to accomplish a couple of different scenarios.
    /// ## Modify the Player build options for Play Mode tests
    ///
    /// It is possible to change the [BuildPlayerOptions](https://docs.unity3d.com/ScriptReference/BuildPlayerOptions.html) for the test **Player**, to achieve custom behavior when running **Play Mode** tests. Modifying the build options allows for changing the target location of the build as well as changing [BuildOptions](https://docs.unity3d.com/ScriptReference/BuildOptions.html).
    ///
    /// To modify the `BuildPlayerOptions`, do the following:
    ///
    /// * Implement the `ITestPlayerBuildModifier`
    /// * Reference the implementation type in a `TestPlayerBuildModifier` attribute on an assembly level.
    /// </para>
    /// <code>
    /// using UnityEditor;
    /// using UnityEditor.TestTools;
    ///
    /// [assembly:TestPlayerBuildModifier(typeof(BuildModifier))]
    /// public class BuildModifier : ITestPlayerBuildModifier
    /// {
    ///     public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
    ///     {
    ///         if (playerOptions.target == BuildTarget.iOS)
    ///         {
    ///             playerOptions.options |= BuildOptions.SymlinkLibraries; // Enable symlink libraries when running on iOS
    ///         }
    ///
    ///         playerOptions.options |= BuildOptions.AllowDebugging; // Enable allow Debugging flag on the test Player.
    ///         return playerOptions;
    ///     }
    /// }
    /// </code>
    /// <para>
    /// &gt; **Note:** When building the Player, it includes all `TestPlayerBuildModifier` attributes across all loaded assemblies, independent of the currently used test filter. As the implementation references the `UnityEditor` namespace, the code is typically implemented in an Editor only assembly, as the `UnityEditor` namespace is not available otherwise.
    ///
    /// ## Split build and run
    /// It is possible to use the Unity Editor for building the Player with tests, without [running the tests](./workflow-run-playmode-test-standalone.md). This allows for running the Player on e.g. another machine. In this case, it is necessary to modify the Player to build and implement a custom handling of the test result.
    /// By using `TestPlayerBuildModifier`, you can alter the `BuildOptions` to not start the Player after the build as well as build the Player at a specific location. Combined with [PostBuildCleanup](./reference-setup-and-cleanup.md#prebuildsetup-and-postbuildcleanup), you can automatically exit the Editor on completion of the build.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// using System;
    /// using System.IO;
    /// using System.Linq;
    /// using Tests;
    /// using UnityEditor;
    /// using UnityEditor.TestTools;
    /// using UnityEngine;
    /// using UnityEngine.TestTools;
    ///
    /// [assembly:TestPlayerBuildModifier(typeof(HeadlessPlayModeSetup))]
    /// [assembly:PostBuildCleanup(typeof(HeadlessPlayModeSetup))]
    ///
    /// namespace Tests
    /// {
    ///     public class HeadlessPlayModeSetup : ITestPlayerBuildModifier, IPostBuildCleanup
    ///     {
    ///         private static bool s_RunningPlayerTests;
    ///         public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
    ///         {
    ///             // Do not launch the player after the build completes.
    ///             playerOptions.options &amp;= ~BuildOptions.AutoRunPlayer;
    ///
    ///             // Set the headlessBuildLocation to the output directory you desire. It does not need to be inside the project.
    ///             var headlessBuildLocation = Path.GetFullPath(Path.Combine(Application.dataPath, ".//..//PlayModeTestPlayer"));
    ///             var fileName = Path.GetFileName(playerOptions.locationPathName);
    ///             if (!string.IsNullOrEmpty(fileName))
    ///             {
    ///                 headlessBuildLocation = Path.Combine(headlessBuildLocation, fileName);
    ///             }
    ///             playerOptions.locationPathName = headlessBuildLocation;
    ///
    ///             // Instruct the cleanup to exit the Editor if the run came from the command line.
    ///             // The variable is static because the cleanup is being invoked in a new instance of the class.
    ///             s_RunningPlayerTests = true;
    ///             return playerOptions;
    ///         }
    ///
    ///         public void Cleanup()
    ///         {
    ///             if (s_RunningPlayerTests &amp;&amp; IsRunningTestsFromCommandLine())
    ///             {
    ///             // Exit the Editor on the next update, allowing for other PostBuildCleanup steps to run.
    ///             EditorApplication.update += () =&gt; { EditorApplication.Exit(0); };
    ///             }
    ///         }
    ///
    ///         private static bool IsRunningTestsFromCommandLine()
    ///         {
    ///             var commandLineArgs = Environment.GetCommandLineArgs();
    ///             return commandLineArgs.Any(value =&gt; value == "-runTests");
    ///         }
    ///     }
    /// }
    /// </code>
    /// <para>
    /// If the Editor is still running after the Play Mode tests have run, the Player tries to report the results back, using [PlayerConnection](https://docs.unity3d.com/ScriptReference/Networking.PlayerConnection.PlayerConnection.html), which has a reference to the IP address of the Editor machine, when built.
    ///
    /// To implement a custom way of reporting the results of the test run, let one of the assemblies in the Player include a `TestRunCallbackAttribute`. At `RunFinished`, it is possible to get the full test report as XML from the [NUnit](http://www.nunit.org/) test result by calling `result.ToXml(true)`. You can save the result and then save it on the device or send it to another machine as needed.
    /// </para>
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class TestPlayerBuildModifierAttribute : Attribute
    {
        private Type m_Type;

        /// <summary>
        /// Initializes and returns an instance of TestPlayerBuildModifierAttribute or throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="type">A target type that implements ITestPlayerBuildModifier.</param>
        /// <exception cref="ArgumentException">Throws a <see cref="ArgumentException"/> if the type provided does not implemented the `ITestPlayerBuildModifier` interface. </exception>
        public TestPlayerBuildModifierAttribute(Type type)
        {
            var interfaceType = typeof(ITestPlayerBuildModifier);
            if (!interfaceType.IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format("Type provided to {0} does not implement {1}", GetType().Name, interfaceType.Name));
            }
            m_Type = type;
        }

        internal ITestPlayerBuildModifier ConstructModifier()
        {
            return Activator.CreateInstance(m_Type) as ITestPlayerBuildModifier;
        }
    }
}
