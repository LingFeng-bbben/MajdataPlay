using System;
using System.Collections;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// In an Edit Mode test, you can use `IEditModeTestYieldInstruction` interface to implement your own instruction. There are also a couple of commonly used implementations available:
    /// - <see cref = "EnterPlayMode"/>
    /// - <see cref = "ExitPlayMode"/>
    /// - <see cref = "RecompileScripts"/>
    /// - <see cref = "WaitForDomainReload"/>
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [UnityTest]
    /// public IEnumerator PlayOnAwakeDisabled_DoesntPlayWhenEnteringPlayMode()
    /// {
    ///    var videoPlayer = PrefabUtility.InstantiatePrefab(m_VideoPlayerPrefab.GetComponent<VideoPlayer>()) as VideoPlayer;
    ///
    ///    videoPlayer.playOnAwake = false;
    ///
    ///    yield return new EnterPlayMode();
    ///
    ///    var videoPlayerGO = GameObject.Find(m_VideoPlayerPrefab.name);
    ///
    ///    Assert.IsFalse(videoPlayerGO.GetComponent<VideoPlayer>().isPlaying);
    ///
    ///    yield return new ExitPlayMode();
    ///
    ///    Object.DestroyImmediate(GameObject.Find(m_VideoPlayerPrefab.name));
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IEditModeTestYieldInstruction
    {
        /// <summary>
        /// Whether or not the instruction expects a domain reload to occur.
        /// </summary>
        bool ExpectDomainReload { get; }
        /// <summary>
        /// Whether or not the instruction expects the Unity Editor to be in **Play Mode**.
        /// </summary>
        bool ExpectedPlaymodeState { get; }
        /// <summary>
        ///  Used to define multi-frame operations performed when instantiating a yield instruction.
        /// </summary>
        /// <returns>Enumerable collection of operations to perform.</returns>
        IEnumerator Perform();
    }
}
