package net.majdata.majdataplay.runtime;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.os.Build;
import android.util.Base64;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.nio.charset.StandardCharsets;
import java.security.KeyStore;
import java.security.cert.X509Certificate;
import javax.net.ssl.TrustManager;
import javax.net.ssl.TrustManagerFactory;
import javax.net.ssl.X509TrustManager;

public final class SystemCAImporter
{
    public static boolean tryInit(Context context) {
        File cacheDir = new File(context.getFilesDir(), "Runtime/Networking");

        if (!cacheDir.exists()) {
            cacheDir.mkdirs();
        }
        File pemFile = new File(cacheDir, "ca.pem");
        File versionFile = new File(cacheDir, "ca.version");

        if (isCacheValid(context, versionFile, pemFile)) {

            return true;
        }

        try {
            String pem = extractAndroidSystemCerts();

            writeToFile(pemFile, pem);
            writeToFile(versionFile, getVersionFingerprint(context));

            return true;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    private static boolean isCacheValid(Context context, File versionFile, File pemFile) {
        if (!versionFile.exists()) return false;
        if (!pemFile.exists()) return false;

        try {
            String cached = readFromFile(versionFile).trim();
            return cached.equals(getVersionFingerprint(context));
        } catch (Exception e) {
            return false;
        }
    }

    private static String getVersionFingerprint(Context context) {
        String appVersion = "unknown";
        try {
            PackageInfo pInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), 0);
            appVersion = pInfo.versionName;
        } catch (Exception e) {
            e.printStackTrace();
        }
        String osVersion = Build.VERSION.RELEASE + "_" + Build.VERSION.SDK_INT;
        return appVersion + "|" + osVersion;
    }

    private static String extractAndroidSystemCerts() throws Exception {
        StringBuilder sb = new StringBuilder(256 * 1024);

        String algorithm = TrustManagerFactory.getDefaultAlgorithm();
        TrustManagerFactory tmf = TrustManagerFactory.getInstance(algorithm);

        tmf.init((KeyStore) null);

        TrustManager[] trustManagers = tmf.getTrustManagers();
        if (trustManagers == null || trustManagers.length == 0) {
            throw new Exception("No TrustManagers found");
        }

        X509TrustManager tm = (X509TrustManager) trustManagers[0];
        X509Certificate[] certs = tm.getAcceptedIssuers();

        if (certs == null) {
            throw new Exception("getAcceptedIssuers returned null");
        }

        int count = 0;
        for (X509Certificate cert : certs) {
            if (cert == null) continue;
            try {
                byte[] der = cert.getEncoded();
                if (der == null || der.length == 0) continue;

                sb.append("-----BEGIN CERTIFICATE-----\n");
                String base64Str = Base64.encodeToString(der, Base64.DEFAULT);
                sb.append(base64Str);

                // 确保有换行符分隔
                if (!base64Str.endsWith("\n")) {
                    sb.append("\n");
                }
                sb.append("-----END CERTIFICATE-----\n");
                count++;
            } catch (Exception e) {
                e.printStackTrace();
            }
        }

        return sb.toString();
    }

    private static void writeToFile(File file, String content) throws Exception {
        try (FileOutputStream fos = new FileOutputStream(file)) {
            fos.write(content.getBytes(StandardCharsets.US_ASCII));
        }
    }

    private static String readFromFile(File file) throws Exception {
        try (FileInputStream fis = new FileInputStream(file)) {
            byte[] data = new byte[(int) file.length()];
            fis.read(data);
            return new String(data, StandardCharsets.US_ASCII);
        }
    }

}
