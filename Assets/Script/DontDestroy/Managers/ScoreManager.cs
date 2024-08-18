using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class ScoreManager: MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }


    public void Awake()
    {
        Instance = this;
    }
}
