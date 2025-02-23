using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal class GameUpdater : MonoBehaviour
    {
        InputManager _inputManager;
        void Awake()
        {
            MajInstances.GameUpdater = this;
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            _inputManager = MajInstances.InputManager;
        }
        //void FixedUpdate()
        //{
        //    var gpManager = MajInstanceHelper<GamePlayManager>.Instance;
        //    var noteManager = MajInstanceHelper<NoteManager>.Instance;

        //    gpManager?.OnFixedUpdate();
        //    noteManager?.OnFixedUpdate();
        //    _inputManager.OnFixedUpdate();
        //}
        void Update()
        {
            var gpManager = MajInstanceHelper<GamePlayManager>.Instance;
            var noteManager = MajInstanceHelper<NoteManager>.Instance;

            gpManager?.OnUpdate();
            noteManager?.OnUpdate();
            _inputManager.OnUpdate();
        }
    }
}
