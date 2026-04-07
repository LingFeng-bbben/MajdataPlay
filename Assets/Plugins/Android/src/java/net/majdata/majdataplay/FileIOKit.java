package net.majdata.majdataplay;

import android.content.Intent;
import android.net.Uri;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public final class FileIOKit
{
    static int CODE_NO_ERROR = 0;
    static int CODE_FILE_NOT_FOUND = 1;
    static int CODE_IO_ERROR = 2;
    public static int CopyContentToFile(Uri contentUri, String dst)
    {
        var activity = MajdataPlayActivity.getCurrentActivity();
        var contentResolver = activity.getContentResolver();
        try
        {
            var inFileStream = contentResolver.openInputStream(contentUri);
            var outFileStream = new FileOutputStream(dst);

            var buffer = new byte[8192];
            var len = 0;
            while ((len = inFileStream.read(buffer)) > 0)
            {
                outFileStream.write(buffer, 0, len);
            }
            inFileStream.close();
            outFileStream.close();
        }
        catch (FileNotFoundException e)
        {
            return CODE_FILE_NOT_FOUND;
        }
        catch (IOException e)
        {
            return CODE_IO_ERROR;
        }
        return CODE_NO_ERROR;
    }
}
