using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnPreUpdate()
        {
            // Time Update
            MajTimeline.OnPreUpdate();
            try
            {
                switch (SceneSwitcher.CurrentScene)
                {
                    case MajScenes.Game:
                        var gpManager = Majdata<GamePlayManager>.Instance;
                        gpManager?.OnPreUpdate();
                        break;
                }
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        void Update()
        {
            try
            {
                OnPreUpdate();
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
            
            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnUpdate()
        {
            try
            {
                switch (SceneSwitcher.CurrentScene)
                {
                    case MajScenes.Game:
                        var gpManager = Majdata<GamePlayManager>.Instance;
                        gpManager?.OnUpdate();
                        break;
                }
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
            finally
            {
                _inputManager.OnUpdate();
            }
        }
        void LateUpdate()
        {
            try
            {
                switch (SceneSwitcher.CurrentScene)
                {
                    case MajScenes.Game:
                        var gpManager = Majdata<GamePlayManager>.Instance;
                        gpManager?.OnLateUpdate();
                        break;
                }
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
    }
}
