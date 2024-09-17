using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.IO
{
    public partial class InputManager : MonoBehaviour
    {
        async void COMReceiveAsync(CancellationToken token)
        {
            SerialPort? serial = null;
            try
            {
                serial = new SerialPort("COM3", 9600);
                recvTask = Task.Run(() =>
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        if (serial.IsOpen)
                        {
                            int count = serial.BytesToRead;
                            var buf = new byte[count];
                            serial.Read(buf, 0, count);
                            if (buf.Length > 0)
                            {

                                if (buf[0] == '(')
                                {
                                    int k = 0;
                                    for (int i = 1; i < 8; i++)
                                    {
                                        //print(buf[i].ToString("X2"));
                                        for (int j = 0; j < 5; j++)
                                        {
                                            COMReport[k] = (buf[i] & 0x01 << j) > 0;
                                            k++;
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            serial.Open();
                            //see https://github.com/Sucareto/Mai2Touch/tree/main/Mai2Touch
                            serial.Write("{RSET}");
                            serial.Write("{HALT}");
                            //send ratio
                            for(byte a = 0x41; a <= 0x62; a++)
                            {
                                serial.Write("{L"+(char)a+"r2}");
                            }
                            //send sensitivity
                            //adx have another method to set sens, so we dont do it here
                            /*for (byte a = 0x41; a <= 0x62; a++)
                            {
                                serial.Write("{L" + (char)a + "k"+sens+"}");
                            }*/
                            serial.Write("{STAT}");
                        }
                    }
                });
                await recvTask;
            }
            catch(IOException)
            {
                Debug.LogWarning("Cannot open COM3, using Mouse as fallback.");
                useDummy = true;
            }
            finally
            {
                serial!.Close();
            }
        }
    }
}
