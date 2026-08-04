using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.UI
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-1000)]
    public class MainCameraConfigurator : MajBehaviour
    {
        Camera _camera;

        protected override void Awake()
        {
            base.Awake();
            _camera = GetComponent<Camera>();
            SceneSwitcher.MainCamera = _camera;
        }
    }
}
