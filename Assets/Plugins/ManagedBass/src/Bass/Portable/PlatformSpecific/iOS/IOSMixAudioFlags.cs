using System;

namespace ManagedBass
{
    /// <summary>
    /// Flags to be used with <see cref="Bass.IOSMixAudio"/>.
    /// </summary>
    [Flags]
    public enum IOSMixAudioFlags
    {
        /// <summary>
        /// Allow other audio to mix with app's audio (default).
        /// Enables kAudioSessionProperty_OverrideCategoryMixWithOthers.
        /// </summary>
        MixWithOthers = 0x1,

        /// <summary>
        /// Also 'duck' the other audio.
        /// Enables kAudioSessionProperty_OtherMixableAudioShouldDuck.
        /// </summary>
        OtherMixableAudioShouldDuck = 0x2,

        /// <summary>
        /// Use the 'ambient' category.
        /// Enables kAudioSessionCategory_SoloAmbientSound/AmbientSound instead of kAudioSessionCategory_MediaPlayback.
        /// </summary>
        AmbientSound = 0x4,

        /// <summary>
        /// Route the output to the speaker instead of the receiver. Enables AVAudioSessionCategoryOptionDefaultToSpeaker. 
        /// </summary>
        Speaker = 0x8,

        /// <summary>
        /// Disable BASS's audio session configuration management so that the app can handle that itself. 
        /// </summary>
        Disable = 0x10,
    }
}
