package net.majdata.majdataplay;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.Application;
import android.os.Bundle;
import android.view.ViewTreeObserver;

public final class AppStatus
{

    private static boolean isForeground = true;
    private static boolean isFocused = true;
    private static int startedActivityCount = 0;

    public static void register(final CSharpAppStatusListenerCallback listener) {
        final Activity currentActivity = MajdataPlayActivity.getCurrentActivity();
        if (currentActivity == null) return;

        isFocused = currentActivity.hasWindowFocus();

        ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
        ActivityManager.getMyMemoryState(appProcessInfo);

        isForeground = (appProcessInfo.importance == ActivityManager.RunningAppProcessInfo.IMPORTANCE_FOREGROUND ||
                appProcessInfo.importance == ActivityManager.RunningAppProcessInfo.IMPORTANCE_VISIBLE);

        startedActivityCount = isForeground ? 1 : 0;

        currentActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                currentActivity.getWindow().getDecorView().getViewTreeObserver().addOnWindowFocusChangeListener(
                        new ViewTreeObserver.OnWindowFocusChangeListener() {
                            @Override
                            public void onWindowFocusChanged(boolean hasFocus) {
                                if (isFocused != hasFocus) {
                                    isFocused = hasFocus;
                                    if (listener != null) listener.onFocusChanged(hasFocus);
                                }
                            }
                        }
                );
            }
        });

        Application app = currentActivity.getApplication();
        app.registerActivityLifecycleCallbacks(new Application.ActivityLifecycleCallbacks() {
            @Override
            public void onActivityStarted(Activity activity) {
                startedActivityCount++;
                if (startedActivityCount >= 1) {
                    isForeground = true;
                    if (listener != null) listener.onForegroundChanged(true);
                }
            }

            @Override
            public void onActivityStopped(Activity activity) {
                if (startedActivityCount > 0) {
                    startedActivityCount--;
                }

                if (startedActivityCount == 0) {
                    isForeground = false;
                    if (listener != null) listener.onForegroundChanged(false);
                }
            }

            @Override public void onActivityCreated(Activity activity, Bundle savedInstanceState) {}
            @Override public void onActivityResumed(Activity activity) {}
            @Override public void onActivityPaused(Activity activity) {}
            @Override public void onActivitySaveInstanceState(Activity activity, Bundle outState) {}
            @Override public void onActivityDestroyed(Activity activity) {}
        });
    }

    public static boolean isAppInForeground() {
        return isForeground;
    }

    public static boolean isAppFocused() {
        return isFocused;
    }
}