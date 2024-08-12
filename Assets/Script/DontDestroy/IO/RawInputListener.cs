using MajdataPlay.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityRawInput;

namespace MajdataPlay.IO
{
    public partial class IOManager : MonoBehaviour
    {
        void RawInput_OnKeyUp(RawKey key)
        {
            for (int i = 0; i < IndexToKeyButton.Length; i++)
            {
                if (IndexToKeyButton[i] == key.ToString())
                {
                    if (OnButtonUp != null)
                        OnButtonUp(this, new ButtonEventArgs(i));
                }
            }
        }

        void RawInput_OnKeyDown(RawKey key)
        {
            for (int i = 0; i < IndexToKeyButton.Length; i++)
            {
                if (IndexToKeyButton[i] == key.ToString())
                {
                    if (OnButtonDown != null)
                        OnButtonDown(this, new ButtonEventArgs(i));
                }
            }
        }
    }
}
