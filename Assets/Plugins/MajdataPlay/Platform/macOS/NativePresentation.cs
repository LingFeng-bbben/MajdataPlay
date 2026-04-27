using System;
using System.Runtime.InteropServices;

namespace MajdataPlay.Platform.MacOS
{
    public static class NativePresentation
    {
        const ulong PresentationHideDock = 1UL << 1;
        const ulong PresentationHideMenuBar = 1UL << 3;

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        const string ObjCLib = "/usr/lib/libobjc.A.dylib";

        const ulong WindowCollectionBehaviorCanJoinAllSpaces = 1UL << 0;
        const ulong WindowCollectionBehaviorFullScreenPrimary = 1UL << 7;
        const ulong WindowStyleMaskFullScreen = 1UL << 14;
        const ulong PresentationFullScreen = 1UL << 10;

        static IntPtr _fullscreenDelegate;
        static IntPtr _originalWindowDelegate;
        static CGSize _fullscreenContentSize;
        static readonly WillUseFullScreenContentSizeDelegate _willUseFullScreenContentSize = WillUseFullScreenContentSize;
        static readonly WillUseFullScreenPresentationOptionsDelegate _willUseFullScreenPresentationOptions = WillUseFullScreenPresentationOptions;
        static readonly WindowDidEnterFullScreenDelegate _windowDidEnterFullScreen = WindowDidEnterFullScreen;

        [DllImport(ObjCLib)]
        static extern IntPtr objc_getClass(string name);

        [DllImport(ObjCLib)]
        static extern IntPtr sel_registerName(string name);

        [DllImport(ObjCLib)]
        static extern IntPtr objc_allocateClassPair(IntPtr superclass, string name, IntPtr extraBytes);

        [DllImport(ObjCLib)]
        static extern void objc_registerClassPair(IntPtr cls);

        [DllImport(ObjCLib)]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool class_addMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);

        [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
        static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
        static extern IntPtr IntPtr_objc_msgSend_UInt64(IntPtr receiver, IntPtr selector, ulong arg);

        [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
        static extern void Void_objc_msgSend_UInt64(IntPtr receiver, IntPtr selector, ulong arg);

        [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
        static extern void Void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

        [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
        static extern ulong UInt64_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(ObjCLib, EntryPoint = "objc_msgSend")]
        static extern CGRect CGRect_objc_msgSend(IntPtr receiver, IntPtr selector);
#endif

        public static bool EnterNativeFullscreenIfNeeded()
        {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            var window = GetUnityWindow();
            if (window == IntPtr.Zero)
            {
                return false;
            }

            Void_objc_msgSend_UInt64(
                window,
                sel_registerName("setCollectionBehavior:"),
                WindowCollectionBehaviorCanJoinAllSpaces | WindowCollectionBehaviorFullScreenPrimary);

            var styleMask = UInt64_objc_msgSend(window, sel_registerName("styleMask"));
            if ((styleMask & WindowStyleMaskFullScreen) != 0)
            {
                return true;
            }

            InstallFullscreenDelegate(window);
            Void_objc_msgSend_IntPtr(window, sel_registerName("toggleFullScreen:"), IntPtr.Zero);
            return true;
#else
            return false;
#endif
        }

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        static IntPtr GetUnityWindow()
        {
            var application = GetSharedApplication();
            if (application == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var window = IntPtr_objc_msgSend(application, sel_registerName("mainWindow"));
            if (window != IntPtr.Zero)
            {
                return window;
            }

            window = IntPtr_objc_msgSend(application, sel_registerName("keyWindow"));
            return window != IntPtr.Zero
                ? window
                : GetFirstApplicationWindow(application);
        }

        static IntPtr GetFirstApplicationWindow(IntPtr application)
        {
            var windows = IntPtr_objc_msgSend(application, sel_registerName("windows"));
            if (windows == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var count = UInt64_objc_msgSend(windows, sel_registerName("count"));
            for (ulong i = 0; i < count; i++)
            {
                var window = IntPtr_objc_msgSend_UInt64(windows, sel_registerName("objectAtIndex:"), i);
                if (window != IntPtr.Zero)
                {
                    return window;
                }
            }

            return IntPtr.Zero;
        }

        static void InstallFullscreenDelegate(IntPtr window)
        {
            var screen = GetWindowScreen(window);
            if (screen != IntPtr.Zero)
            {
                _fullscreenContentSize = CGRect_objc_msgSend(screen, sel_registerName("frame")).Size;
            }

            var fullscreenDelegate = GetFullscreenDelegate();
            if (fullscreenDelegate == IntPtr.Zero)
            {
                return;
            }

            _originalWindowDelegate = IntPtr_objc_msgSend(window, sel_registerName("delegate"));
            Void_objc_msgSend_IntPtr(window, sel_registerName("setDelegate:"), fullscreenDelegate);
        }

        static IntPtr GetWindowScreen(IntPtr window)
        {
            var screen = IntPtr_objc_msgSend(window, sel_registerName("screen"));
            if (screen != IntPtr.Zero)
            {
                return screen;
            }

            var screenClass = objc_getClass("NSScreen");
            return screenClass == IntPtr.Zero
                ? IntPtr.Zero
                : IntPtr_objc_msgSend(screenClass, sel_registerName("mainScreen"));
        }

        static IntPtr GetFullscreenDelegate()
        {
            if (_fullscreenDelegate != IntPtr.Zero)
            {
                return _fullscreenDelegate;
            }

            var nsObjectClass = objc_getClass("NSObject");
            var delegateClass = objc_allocateClassPair(nsObjectClass, "MajdataFullscreenDelegate", IntPtr.Zero);
            if (delegateClass == IntPtr.Zero)
            {
                delegateClass = objc_getClass("MajdataFullscreenDelegate");
            }
            else
            {
                class_addMethod(
                    delegateClass,
                    sel_registerName("window:willUseFullScreenContentSize:"),
                    Marshal.GetFunctionPointerForDelegate(_willUseFullScreenContentSize),
                    "{CGSize=dd}@:@{CGSize=dd}");
                class_addMethod(
                    delegateClass,
                    sel_registerName("window:willUseFullScreenPresentationOptions:"),
                    Marshal.GetFunctionPointerForDelegate(_willUseFullScreenPresentationOptions),
                    "Q@:@Q");
                class_addMethod(
                    delegateClass,
                    sel_registerName("windowDidEnterFullScreen:"),
                    Marshal.GetFunctionPointerForDelegate(_windowDidEnterFullScreen),
                    "v@:@");
                objc_registerClassPair(delegateClass);
            }

            _fullscreenDelegate = delegateClass == IntPtr.Zero
                ? IntPtr.Zero
                : IntPtr_objc_msgSend(IntPtr_objc_msgSend(delegateClass, sel_registerName("alloc")), sel_registerName("init"));
            return _fullscreenDelegate;
        }

        [MonoPInvokeCallback(typeof(WillUseFullScreenContentSizeDelegate))]
        static CGSize WillUseFullScreenContentSize(IntPtr self, IntPtr selector, IntPtr window, CGSize proposedSize)
        {
            return _fullscreenContentSize.Width > 0 && _fullscreenContentSize.Height > 0
                ? _fullscreenContentSize
                : proposedSize;
        }

        [MonoPInvokeCallback(typeof(WillUseFullScreenPresentationOptionsDelegate))]
        static ulong WillUseFullScreenPresentationOptions(IntPtr self, IntPtr selector, IntPtr window, ulong proposedOptions)
        {
            return PresentationFullScreen | PresentationHideDock | PresentationHideMenuBar;
        }

        [MonoPInvokeCallback(typeof(WindowDidEnterFullScreenDelegate))]
        static void WindowDidEnterFullScreen(IntPtr self, IntPtr selector, IntPtr notification)
        {
            var window = IntPtr_objc_msgSend(notification, sel_registerName("object"));
            if (window != IntPtr.Zero && _originalWindowDelegate != IntPtr.Zero)
            {
                Void_objc_msgSend_IntPtr(window, sel_registerName("setDelegate:"), _originalWindowDelegate);
                _originalWindowDelegate = IntPtr.Zero;
            }
        }

        static IntPtr GetSharedApplication()
        {
            var applicationClass = objc_getClass("NSApplication");
            return applicationClass == IntPtr.Zero
                ? IntPtr.Zero
                : IntPtr_objc_msgSend(applicationClass, sel_registerName("sharedApplication"));
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CGPoint
        {
            public double X;
            public double Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CGSize
        {
            public double Width;
            public double Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CGRect
        {
            public CGPoint Origin;
            public CGSize Size;
        }

        delegate CGSize WillUseFullScreenContentSizeDelegate(IntPtr self, IntPtr selector, IntPtr window, CGSize proposedSize);
        delegate ulong WillUseFullScreenPresentationOptionsDelegate(IntPtr self, IntPtr selector, IntPtr window, ulong proposedOptions);
        delegate void WindowDidEnterFullScreenDelegate(IntPtr self, IntPtr selector, IntPtr notification);

        sealed class MonoPInvokeCallbackAttribute : Attribute
        {
            public MonoPInvokeCallbackAttribute(Type type)
            {
            }
        }
#endif
    }
}
