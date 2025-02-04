namespace UnityRawInput
{
    public enum RawMouseState : ushort
    {
        MouseMove = 0x0200,
        LeftButtonDown = 0x0201,
        LeftButtonUp = 0x0202,
        LeftButtonDoubleClick = 0x0203,
        RightButtonDown = 0x0204,
        RightButtonUp = 0x0205,
        RightButtonDoubleClick = 0x0206,
        MiddleButtonDown = 0x0207,
        MiddleButtonUp = 0x0208,
        MiddleButtonDoubleClick = 0x0209,
        MouseWheel = 0x020A,
        ExtraButtonDown = 0x20B,
        ExtraButtonUp = 0x20C,
        ExtraButtonDoubleClick = 0x20D,
        MouseWheelHorizontal = 0x20E
    }

    // https://www.pinvoke.net/default.aspx/Constants/WM.html
}
