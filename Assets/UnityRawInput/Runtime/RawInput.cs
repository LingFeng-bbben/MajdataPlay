using System;
using System.Collections.Generic;
using AOT;
using UnityEngine;

namespace UnityRawInput
{
    public static class RawInput
    {
        /// <summary>
        /// Event invoked when user presses a key.
        /// </summary>
        public static event Action<RawKey> OnKeyDown;
        /// <summary>
        /// Event invoked when user releases a key.
        /// </summary>
        public static event Action<RawKey> OnKeyUp;

        /// <summary>
        /// Whether the service is running and input messages are being processed.
        /// </summary>
        public static bool IsRunning => hooks.Count > 0;
        /// <summary>
        /// Whether any key is currently pressed.
        /// </summary>
        public static bool AnyKeyDown => pressedKeys.Count > 0;
        /// <summary>
        /// Whether input messages should be handled when the application is not in focus.
        /// </summary>
        public static bool WorkInBackground { get; set; }
        /// <summary>
        /// Whether handled input messages should not be propagated further.
        /// </summary>
        public static bool InterceptMessages { get; set; }
        /// <summary>
        /// Currently pressed keys.
        /// </summary>
        public static IReadOnlyCollection<RawKey> PressedKeys => pressedKeys;

        private static readonly HashSet<RawKey> pressedKeys = new HashSet<RawKey>();
        private static readonly List<IntPtr> hooks = new List<IntPtr>();

        /// <summary>
        /// Initializes the service and starts processing input messages.
        /// </summary>
        /// <returns>Whether the service started successfully.</returns>
        public static bool Start ()
        {
            if (IsRunning) return false;
            EnsureRunInBackgroundEnabled();
            SetHooks();
            return hooks.TrueForAll(h => h != IntPtr.Zero);
        }

        /// <summary>
        /// Terminates the service and stops processing input messages.
        /// </summary>
        public static void Stop ()
        {
            RemoveHooks();
            pressedKeys.Clear();
        }

        /// <summary>
        /// Checks whether provided key is currently pressed.
        /// </summary>
        public static bool IsKeyDown (RawKey key)
        {
            return pressedKeys.Contains(key);
        }

        private static void SetHooks ()
        {
            hooks.Add(Win32API.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, HandleKeyboardProc, IntPtr.Zero, 0));
            hooks.Add(Win32API.SetWindowsHookEx(HookType.WH_MOUSE_LL, HandleMouseProc, IntPtr.Zero, 0));
        }

        private static void RemoveHooks ()
        {
            foreach (var pointer in hooks)
                if (pointer != IntPtr.Zero)
                    Win32API.UnhookWindowsHookEx(pointer);
            hooks.Clear();
        }

        [MonoPInvokeCallback(typeof(Win32API.HookProc))]
        private static int HandleKeyboardProc (int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0 || !CanHandleHook()) return Win32API.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            var args = (KeyboardArgs)lParam;
            var state = (RawKeyState)wParam;
            var key = (RawKey)args;

            if (state == RawKeyState.KeyDown || state == RawKeyState.SysKeyDown) HandleKeyDown(key);
            else HandleKeyUp(key);

            return InterceptMessages ? 1 : Win32API.CallNextHookEx(IntPtr.Zero, 0, wParam, lParam);
        }

        [MonoPInvokeCallback(typeof(Win32API.HookProc))]
        private static int HandleMouseProc (int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0 || !CanHandleHook()) return Win32API.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            var args = (MouseArgs)lParam;
            var state = (RawMouseState)wParam;

            if (state == RawMouseState.LeftButtonDown) HandleKeyDown(RawKey.LeftButton);
            else if (state == RawMouseState.LeftButtonUp) HandleKeyUp(RawKey.LeftButton);
            else if (state == RawMouseState.RightButtonDown) HandleKeyDown(RawKey.RightButton);
            else if (state == RawMouseState.RightButtonUp) HandleKeyUp(RawKey.RightButton);
            else if (state == RawMouseState.MiddleButtonDown) HandleKeyDown(RawKey.MiddleButton);
            else if (state == RawMouseState.MiddleButtonUp) HandleKeyUp(RawKey.MiddleButton);
            else if (state == RawMouseState.ExtraButtonDown) HandleKeyDown(GetExtraButtonKey());
            else if (state == RawMouseState.ExtraButtonUp) HandleKeyUp(GetExtraButtonKey());
            else if (state == RawMouseState.MouseWheel) HandleKeyDownUp(GetWheelKey());
            else if (state == RawMouseState.MouseWheelHorizontal) HandleKeyDownUp(GetWheelHorizontalKey());

            return InterceptMessages ? 1 : Win32API.CallNextHookEx(IntPtr.Zero, 0, wParam, lParam);

            short GetWheelDelta () => (short)(args.MouseData >> 16 & 0xFFFF);
            RawKey GetWheelKey () => GetWheelDelta() < 0 ? RawKey.WheelDown : RawKey.WheelUp;
            RawKey GetWheelHorizontalKey () => GetWheelDelta() < 0 ? RawKey.WheelLeft : RawKey.WheelRight;
            RawKey GetExtraButtonKey () => (short)(args.MouseData >> 16 & 0xFFFF) == 1 ? RawKey.ExtraButton1 : RawKey.ExtraButton2;
        }

        private static void HandleKeyDown (RawKey key)
        {
            var added = pressedKeys.Add(key);
            if (added) OnKeyDown?.Invoke(key);
        }

        private static void HandleKeyUp (RawKey key)
        {
            pressedKeys.Remove(key);
            OnKeyUp?.Invoke(key);
        }

        private static void HandleKeyDownUp (RawKey key)
        {
            HandleKeyDown(key);
            HandleKeyUp(key);
        }

        private static bool CanHandleHook ()
        {
            return WorkInBackground || Application.isFocused;
        }

        // https://github.com/Elringus/UnityRawInput/issues/19#issuecomment-1227462101
        private static void EnsureRunInBackgroundEnabled ()
        {
            if (Application.runInBackground) return;
            Debug.LogWarning("Application isn't set to run in background! Not enabling this option will " +
                             "cause severe mouse slowdown if the window isn't in focus. Enabling behavior for this play session, " +
                             "but you should explicitly enable this in \"Build Settings→Player Settings→Player→Resolution and " +
                             "Presentation→Run In Background\".");
            Application.runInBackground = true;
        }
    }
}
