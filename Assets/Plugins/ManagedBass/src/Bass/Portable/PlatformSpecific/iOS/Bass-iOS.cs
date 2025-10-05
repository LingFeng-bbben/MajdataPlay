using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    public static partial class Bass
    {
        static readonly IOSNotifyProcedure iosnproc = status => _iosnotify?.Invoke(status);

        static event IOSNotifyProcedure _iosnotify;

        // TODO: Needs MonoPInvokeDelegateAttribute.
        /// <summary>
        /// Fired when an iOS Audio Session Notification occurs.
        /// </summary>
        public static event IOSNotifyProcedure IOSNotification
        {
            add
            {
                if (_iosnotify == null)
                    Configure(Configuration.IOSNotify, Marshal.GetFunctionPointerForDelegate(iosnproc));

                _iosnotify += value;
            }
            remove
            {
                _iosnotify -= value;

                if (_iosnotify == null)
                    Configure(Configuration.IOSNotify, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Gets or Sets the iOS Audio session category management flags.
        /// </summary>
        public static IOSMixAudioFlags IOSMixAudio
        {
            get => (IOSMixAudioFlags)GetConfig(Configuration.IOSMixAudio);
            set => Configure(Configuration.IOSMixAudio, (int)value);
        }

        /// <summary>
        /// When set to true, disables BASS' audio session category management so that the app can handle that itself.
        /// The <see cref="IOSMixAudio"/>, <see cref="IOSSpeaker"/> and <see cref="PlaybackBufferLength"/> settings will have no effect in that case.
        /// </summary>
        public static bool IOSNoCategory
        {
            get => GetConfigBool(Configuration.IOSNoCategory);
            set => Configure(Configuration.IOSNoCategory, value);
        }

        /// <summary>
        /// When set to true, sends the output to the speaker instead of the receiver.
        /// Enables kAudioSessionPropert_OverrideCategoryDefaultToSpeaker.
        /// The default setting is false.
        /// </summary>
        public static bool IOSSpeaker
        {
            get => GetConfigBool(Configuration.IOSSpeaker);
            set => Configure(Configuration.IOSSpeaker, value);
        }
    }
}