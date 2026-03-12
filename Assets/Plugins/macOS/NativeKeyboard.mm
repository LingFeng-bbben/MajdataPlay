#import <Foundation/Foundation.h>
#import <GameController/GameController.h>

#include <stdint.h>
#include <string.h>
#include <mutex>

namespace
{
    constexpr uint32_t kNativeKeyboardKeyCount = 134;

    enum NativeKeyboardErrorCode : int32_t
    {
        NativeKeyboardErrorCodeNoError = 0,
        NativeKeyboardErrorCodeNotSupported = 1,
        NativeKeyboardErrorCodeNoDevice = 2,
        NativeKeyboardErrorCodeNullPointer = 3,
        NativeKeyboardErrorCodeInvalidHandle = 4,
        NativeKeyboardErrorCodeInvalidOperation = 5,
    };

    std::mutex g_stateMutex;
    bool g_initialized = false;
    bool g_hasKeyboard = false;
    bool g_keyStates[kNativeKeyboardKeyCount] = {};
    GCKeyboardInput *g_keyboardInput = nil;
    id g_connectObserver = nil;
    id g_disconnectObserver = nil;

    static void ClearKeyStatesLocked()
    {
        memset(g_keyStates, 0, sizeof(g_keyStates));
    }

    static GCKeyCode GetGCKeyCodeByIndex(uint32_t index, bool *supported)
    {
        if (supported != nullptr)
        {
            *supported = true;
        }

        switch (index)
        {
            case 0: return GCKeyCodeApplication;
            case 1: return GCKeyCodeLANG7;
            case 2: return GCKeyCodeLANG6;
            case 3: return GCKeyCodeLANG5;
            case 4: return GCKeyCodeLANG4;
            case 5: return GCKeyCodeLANG3;
            case 6: return GCKeyCodeLANG2;
            case 7: return GCKeyCodeLANG1;
            case 8: return GCKeyCodeKeypadSlash;
            case 9: return GCKeyCodeKeypadPlus;
            case 10: return GCKeyCodeKeypadPeriod;
            case 11: return GCKeyCodeKeypadNumLock;
            case 12: return GCKeyCodeKeypadHyphen;
            case 13: return GCKeyCodeKeypadEqualSign;
            case 14: return GCKeyCodeKeypadEnter;
            case 15: return GCKeyCodeKeypadAsterisk;
            case 16: return GCKeyCodeKeypad9;
            case 17: return GCKeyCodeKeypad8;
            case 18: return GCKeyCodeKeypad7;
            case 19: return GCKeyCodeKeypad6;
            case 20: return GCKeyCodeKeypad5;
            case 21: return GCKeyCodeKeypad4;
            case 22: return GCKeyCodeKeypad3;
            case 23: return GCKeyCodeKeypad2;
            case 24: return GCKeyCodeKeypad1;
            case 25: return GCKeyCodeKeypad0;
            case 26: return GCKeyCodeKeyZ;
            case 27: return GCKeyCodeKeyY;
            case 28: return GCKeyCodeKeyX;
            case 29: return GCKeyCodeKeyW;
            case 30: return GCKeyCodeLANG8;
            case 31: return GCKeyCodeLANG9;
            case 32: return GCKeyCodeLeftAlt;
            case 33: return GCKeyCodeLeftArrow;
            case 34: return GCKeyCodeTwo;
            case 35: return GCKeyCodeThree;
            case 36: return GCKeyCodeTab;
            case 37: return GCKeyCodeSpacebar;
            case 38: return GCKeyCodeSlash;
            case 39: return GCKeyCodeSix;
            case 40: return GCKeyCodeSeven;
            case 41: return GCKeyCodeSemicolon;
            case 42: return GCKeyCodeScrollLock;
            case 43: return GCKeyCodeRightShift;
            case 44: return GCKeyCodeRightGUI;
            case 45: return GCKeyCodeRightControl;
            case 46: return GCKeyCodeRightArrow;
            case 47: return GCKeyCodeRightAlt;
            case 48: return GCKeyCodeKeyV;
            case 49: return GCKeyCodeReturnOrEnter;
            case 50: return GCKeyCodePrintScreen;
            case 51: return GCKeyCodePower;
            case 52: return GCKeyCodePeriod;
            case 53: return GCKeyCodePause;
            case 54: return GCKeyCodePageUp;
            case 55: return GCKeyCodePageDown;
            case 56: return GCKeyCodeOpenBracket;
            case 57: return GCKeyCodeOne;
            case 58: return GCKeyCodeNonUSPound;
            case 59: return GCKeyCodeNonUSBackslash;
            case 60: return GCKeyCodeNine;
            case 61: return GCKeyCodeLeftShift;
            case 62: return GCKeyCodeLeftGUI;
            case 63: return GCKeyCodeLeftControl;
            case 64: return GCKeyCodeQuote;
            case 65: return GCKeyCodeKeyU;
            case 66: return GCKeyCodeKeyT;
            case 67: return GCKeyCodeKeyS;
            case 68: return GCKeyCodeF8;
            case 69: return GCKeyCodeF7;
            case 70: return GCKeyCodeF6;
            case 71: return GCKeyCodeF5;
            case 72: return GCKeyCodeF4;
            case 73: return GCKeyCodeF3;
            case 74:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF20;
                }
                break;
            case 75: return GCKeyCodeF2;
            case 76:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF19;
                }
                break;
            case 77:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF18;
                }
                break;
            case 78:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF17;
                }
                break;
            case 79:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF16;
                }
                break;
            case 80:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF15;
                }
                break;
            case 81:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF14;
                }
                break;
            case 82: return GCKeyCodeF9;
            case 83:
                if (@available(macOS 11.0, *))
                {
                    return GCKeyCodeF13;
                }
                break;
            case 84: return GCKeyCodeF11;
            case 85: return GCKeyCodeF10;
            case 86: return GCKeyCodeF1;
            case 87: return GCKeyCodeEscape;
            case 88: return GCKeyCodeEqualSign;
            case 89: return GCKeyCodeEnd;
            case 90: return GCKeyCodeEight;
            case 91: return GCKeyCodeDownArrow;
            case 92: return GCKeyCodeDeleteOrBackspace;
            case 93: return GCKeyCodeDeleteForward;
            case 94: return GCKeyCodeComma;
            case 95: return GCKeyCodeCloseBracket;
            case 96: return GCKeyCodeCapsLock;
            case 97: return GCKeyCodeBackslash;
            case 98: return GCKeyCodeF12;
            case 99: return GCKeyCodeUpArrow;
            case 100: return GCKeyCodeFive;
            case 101: return GCKeyCodeGraveAccentAndTilde;
            case 102: return GCKeyCodeKeyR;
            case 103: return GCKeyCodeKeyQ;
            case 104: return GCKeyCodeKeyP;
            case 105: return GCKeyCodeKeyO;
            case 106: return GCKeyCodeKeyN;
            case 107: return GCKeyCodeKeyM;
            case 108: return GCKeyCodeKeyL;
            case 109: return GCKeyCodeKeyK;
            case 110: return GCKeyCodeKeyJ;
            case 111: return GCKeyCodeKeyI;
            case 112: return GCKeyCodeKeyH;
            case 113: return GCKeyCodeKeyG;
            case 114: return GCKeyCodeKeyF;
            case 115: return GCKeyCodeKeyE;
            case 116: return GCKeyCodeFour;
            case 117: return GCKeyCodeKeyD;
            case 118: return GCKeyCodeKeyB;
            case 119: return GCKeyCodeKeyA;
            case 120: return GCKeyCodeInternational9;
            case 121: return GCKeyCodeInternational8;
            case 122: return GCKeyCodeInternational7;
            case 123: return GCKeyCodeInternational6;
            case 124: return GCKeyCodeInternational5;
            case 125: return GCKeyCodeInternational4;
            case 126: return GCKeyCodeInternational3;
            case 127: return GCKeyCodeInternational2;
            case 128: return GCKeyCodeInternational1;
            case 129: return GCKeyCodeInsert;
            case 130: return GCKeyCodeHyphen;
            case 131: return GCKeyCodeHome;
            case 132: return GCKeyCodeKeyC;
            case 133: return GCKeyCodeZero;
            default: break;
        }

        if (supported != nullptr)
        {
            *supported = false;
        }

        return 0;
    }

    static bool TryGetIndexByGCKeyCode(GCKeyCode keyCode, uint32_t *indexOut)
    {
        if (indexOut == nullptr)
        {
            return false;
        }

        for (uint32_t index = 0; index < kNativeKeyboardKeyCount; ++index)
        {
            bool supported = false;
            GCKeyCode candidate = GetGCKeyCodeByIndex(index, &supported);
            if (supported && candidate == keyCode)
            {
                *indexOut = index;
                return true;
            }
        }

        return false;
    }

    static void SeedCurrentKeyStatesLocked()
    {
        ClearKeyStatesLocked();
        if (g_keyboardInput == nil)
        {
            return;
        }

        for (uint32_t index = 0; index < kNativeKeyboardKeyCount; ++index)
        {
            bool supported = false;
            GCKeyCode keyCode = GetGCKeyCodeByIndex(index, &supported);
            if (!supported)
            {
                continue;
            }

            GCDeviceButtonInput *button = [g_keyboardInput buttonForKeyCode:keyCode];
            g_keyStates[index] = button != nil ? button.pressed : false;
        }
    }

    static void DetachKeyboardLocked()
    {
        if (g_keyboardInput != nil)
        {
            g_keyboardInput.keyChangedHandler = nil;
            g_keyboardInput = nil;
        }

        g_hasKeyboard = false;
        ClearKeyStatesLocked();
    }

    static void AttachToKeyboardLocked(GCKeyboard *keyboard)
    {
        DetachKeyboardLocked();

        GCKeyboardInput *keyboardInput = keyboard.keyboardInput;
        if (keyboardInput == nil)
        {
            return;
        }

        g_keyboardInput = keyboardInput;
        g_hasKeyboard = true;
        SeedCurrentKeyStatesLocked();

        g_keyboardInput.keyChangedHandler = ^(GCKeyboardInput *input, GCDeviceButtonInput *button, GCKeyCode keyCode, BOOL pressed)
        {
            (void)input;
            (void)button;

            uint32_t keyIndex = 0;
            if (!TryGetIndexByGCKeyCode(keyCode, &keyIndex))
            {
                return;
            }

            std::lock_guard<std::mutex> lock(g_stateMutex);
            if (keyIndex < kNativeKeyboardKeyCount)
            {
                g_keyStates[keyIndex] = pressed;
            }
        };
    }

    static void RefreshKeyboardLocked()
    {
        GCKeyboard *keyboard = GCKeyboard.coalescedKeyboard;
        GCKeyboardInput *currentInput = keyboard.keyboardInput;

        if (currentInput == g_keyboardInput)
        {
            g_hasKeyboard = currentInput != nil;
            return;
        }

        if (currentInput == nil)
        {
            DetachKeyboardLocked();
            return;
        }

        AttachToKeyboardLocked(keyboard);
    }

    static void InstallObserversLocked()
    {
        NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
        if (g_connectObserver == nil)
        {
            g_connectObserver = [center addObserverForName:GCKeyboardDidConnectNotification
                                                    object:nil
                                                     queue:nil
                                                usingBlock:^(__unused NSNotification *notification)
            {
                std::lock_guard<std::mutex> lock(g_stateMutex);
                RefreshKeyboardLocked();
            }];
        }

        if (g_disconnectObserver == nil)
        {
            g_disconnectObserver = [center addObserverForName:GCKeyboardDidDisconnectNotification
                                                       object:nil
                                                        queue:nil
                                                   usingBlock:^(__unused NSNotification *notification)
            {
                std::lock_guard<std::mutex> lock(g_stateMutex);
                RefreshKeyboardLocked();
            }];
        }
    }

    static void RemoveObserversLocked()
    {
        NSNotificationCenter *center = [NSNotificationCenter defaultCenter];

        if (g_connectObserver != nil)
        {
            [center removeObserver:g_connectObserver];
            g_connectObserver = nil;
        }

        if (g_disconnectObserver != nil)
        {
            [center removeObserver:g_disconnectObserver];
            g_disconnectObserver = nil;
        }
    }
}

extern "C"
{
    int32_t Init(void)
    {
        NSLog(@"[NativeKeyboard/macOS] Init begin");
        std::lock_guard<std::mutex> lock(g_stateMutex);
        if (g_initialized)
        {
            NSLog(@"[NativeKeyboard/macOS] Init aborted: already initialized");
            return NativeKeyboardErrorCodeInvalidOperation;
        }

        g_initialized = true;
        InstallObserversLocked();
        RefreshKeyboardLocked();
        NSLog(@"[NativeKeyboard/macOS] Init completed, hasKeyboard=%d", g_hasKeyboard ? 1 : 0);
        return g_hasKeyboard ? NativeKeyboardErrorCodeNoError : NativeKeyboardErrorCodeNoDevice;
    }

    int32_t Free(void)
    {
        std::lock_guard<std::mutex> lock(g_stateMutex);
        if (!g_initialized)
        {
            return NativeKeyboardErrorCodeInvalidOperation;
        }

        RemoveObserversLocked();
        DetachKeyboardLocked();
        g_initialized = false;
        return NativeKeyboardErrorCodeNoError;
    }

    intptr_t GetKeyboardHandle(void)
    {
        std::lock_guard<std::mutex> lock(g_stateMutex);
        if (!g_initialized)
        {
            return 0;
        }

        RefreshKeyboardLocked();
        return g_hasKeyboard ? 1 : 0;
    }

    int32_t IsPressed(uint32_t keyCodeIndex, uint8_t *isPressedOut)
    {
        if (isPressedOut == nullptr)
        {
            return NativeKeyboardErrorCodeNullPointer;
        }

        *isPressedOut = 0;

        std::lock_guard<std::mutex> lock(g_stateMutex);
        if (!g_initialized)
        {
            return NativeKeyboardErrorCodeInvalidOperation;
        }

        RefreshKeyboardLocked();
        if (!g_hasKeyboard)
        {
            return NativeKeyboardErrorCodeNoDevice;
        }

        bool supported = false;
        (void)GetGCKeyCodeByIndex(keyCodeIndex, &supported);
        if (!supported)
        {
            return NativeKeyboardErrorCodeNotSupported;
        }

        *isPressedOut = g_keyStates[keyCodeIndex] ? 1 : 0;
        return NativeKeyboardErrorCodeNoError;
    }

    int32_t IsPressedWithHandle(intptr_t keyboardHandle, uint32_t keyCodeIndex, uint8_t *isPressedOut)
    {
        if (keyboardHandle == 0)
        {
            if (isPressedOut != nullptr)
            {
                *isPressedOut = 0;
            }
            return NativeKeyboardErrorCodeInvalidHandle;
        }

        return IsPressed(keyCodeIndex, isPressedOut);
    }
}
