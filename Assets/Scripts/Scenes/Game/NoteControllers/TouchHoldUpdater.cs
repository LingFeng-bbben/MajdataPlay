using UnityEngine.Profiling;

namespace MajdataPlay.Game
{
    public class TouchHoldUpdater : NoteUpdater
    {
        const string UPDATER_NAME = "TouchHoldUpdater";
        const string PRE_UPDATE_METHOD_NAME = UPDATER_NAME + ".PreUpdate";
        const string UPDATE_METHOD_NAME = UPDATER_NAME + ".Update";
        const string FIXED_UPDATE_METHOD_NAME = UPDATER_NAME + ".FixedUpdate";
        const string LATE_UPDATE_METHOD_NAME = UPDATER_NAME + ".LateUpdate";
        internal override void OnFixedUpdate()
        {
            Profiler.BeginSample(FIXED_UPDATE_METHOD_NAME);
            base.OnFixedUpdate();
            Profiler.EndSample();
        }
        internal override void OnLateUpdate()
        {
            Profiler.BeginSample(LATE_UPDATE_METHOD_NAME);
            base.OnLateUpdate();
            Profiler.EndSample();
        }
        internal override void OnUpdate()
        {
            Profiler.BeginSample(UPDATE_METHOD_NAME);
            base.OnUpdate();
            Profiler.EndSample();
        }
        internal override void OnPreUpdate()
        {
            Profiler.BeginSample(PRE_UPDATE_METHOD_NAME);
            base.OnPreUpdate();
            Profiler.EndSample();
        }
    }
}
