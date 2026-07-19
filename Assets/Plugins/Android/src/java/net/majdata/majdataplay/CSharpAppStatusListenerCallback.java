package net.majdata.majdataplay;

public interface CSharpAppStatusListenerCallback
{
    void onForegroundChanged(boolean isForeground);
    void onFocusChanged(boolean isFocused);
}
