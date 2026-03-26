package net.majdata.majdataplay;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MajdataPlayActivity extends UnityPlayerActivity
{
    static CSharpOnNewIntentCallback onNewIntentCallbackProxy;
    static CSharpOnActivityResultCallback onActivityResultCallbackProxy;
    static Activity currentActivity;
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        currentActivity = this;
        super.onCreate(savedInstanceState);
    }
    @Override
    protected void onNewIntent(Intent intent)
    {
        super.onNewIntent(intent);

        setIntent(intent);
        if (onNewIntentCallbackProxy != null)
        {
            onNewIntentCallbackProxy.OnNewIntent(intent);
        }
    }
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data)
    {
        if (onActivityResultCallbackProxy != null)
        {
            onActivityResultCallbackProxy.OnActivityResult(requestCode, resultCode, data);
        }
    }
    public static Activity getCurrentActivity()
    {
        return currentActivity;
    }
    public static void registerOnNewIntentCallback(CSharpOnNewIntentCallback callback)
    {
        if (onNewIntentCallbackProxy == null)
        {
            onNewIntentCallbackProxy = callback;
        }
    }
    public static void registerOnActivityResultCallback(CSharpOnActivityResultCallback callback)
    {
        if (onActivityResultCallbackProxy == null)
        {
            onActivityResultCallbackProxy = callback;
        }
    }
}
