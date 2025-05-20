using MajdataPlay.Game;
using MajdataPlay.IO;
using MajdataPlay.Unsafe;
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
    internal class GameUpdater : MajSingleton
    {
        readonly ReadOnlyRef<GamePlayManager?> _gpManagerRef = new(ref Majdata<GamePlayManager>.Instance);
        DummyTouchPanelRenderer _dummyTouchPanelRenderer;
        DummyLedRenderer _dummyLedRenderer;
        protected override void Awake()
        {
            base.Awake();
            MajInstances.GameUpdater = this;
        }
        void Start()
        {
            _dummyTouchPanelRenderer = Majdata<DummyTouchPanelRenderer>.Instance!;
            _dummyLedRenderer = Majdata<DummyLedRenderer>.Instance!;
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
            InputManager.OnPreUpdate();
            LedRing.OnPreUpdate();
            _dummyTouchPanelRenderer.OnPreUpdate();
            _dummyLedRenderer.OnPreUpdate();
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
