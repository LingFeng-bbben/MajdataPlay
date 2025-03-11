using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.References;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
#nullable enable
namespace MajdataPlay
{
    internal class GameUpdater : MonoBehaviour
    {
        InputManager _inputManager;
        readonly ReadOnlyRef<GamePlayManager?> _gpManagerRef = new(ref Majdata<GamePlayManager>.Instance);
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
            Profiler.BeginSample("GameUpdater.PreUpdate");
            try
            {
                OnPreUpdate();
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
            Profiler.EndSample();
            Profiler.BeginSample("GameUpdater.Update");
            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
            Profiler.EndSample();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnPreUpdate()
        {
            // Time Update
            MajTimeline.OnPreUpdate();
            _inputManager.OnPreUpdate();
            try
            {
                switch (SceneSwitcher.CurrentScene)
                {
                    case MajScenes.Game:
                        _gpManagerRef.Target?.OnPreUpdate();
                        break;
                }
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
                        _gpManagerRef.Target?.OnUpdate();
                        break;
                }
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        void LateUpdate()
        {
            Profiler.BeginSample("GameUpdater.LateUpdate");
            try
            {
                switch (SceneSwitcher.CurrentScene)
                {
                    case MajScenes.Game:
                        _gpManagerRef.Target?.OnLateUpdate();
                        break;
                }
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
            Profiler.EndSample();
        }
    }
}
