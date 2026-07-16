using MajdataPlay.Scenes.Game.Notes.Behaviours;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Profiling;

namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    internal sealed class EachLineUpdater : NoteUpdater<EachLineDrop>
    {
        const string UPDATER_NAME = "EachLineUpdater";
        const string PRE_UPDATE_METHOD_NAME = UPDATER_NAME + ".PreUpdate";
        const string UPDATE_METHOD_NAME = UPDATER_NAME + ".Update";
        const string FIXED_UPDATE_METHOD_NAME = UPDATER_NAME + ".FixedUpdate";
        const string LATE_UPDATE_METHOD_NAME = UPDATER_NAME + ".LateUpdate";

        void Awake()
        {
            Majdata<EachLineUpdater>.Instance = this;
        }
        void OnDestroy()
        {
            Majdata<EachLineUpdater>.Free();
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnFixedUpdate()
        {
            //using (UnityProfiler.Create(FIXED_UPDATE_METHOD_NAME))
            //{
            //    var instanceCount = NoteInstances.Length;
            //    ref var instances = ref MemoryMarshal.GetReference(NoteInstances.AsSpan());
            //    for (var i = 0; i < instanceCount; i++)
            //    {
            //        ref readonly var instance = ref Unsafe.Add(ref instances, i);
            //    }
            //}
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnLateUpdate()
        {
            using (UnityProfiler.Create(LATE_UPDATE_METHOD_NAME))
            {
                var instanceCount = NoteInstances.Length;
                ref var instances = ref MemoryMarshal.GetReference(NoteInstances.AsSpan());
                for (var i = 0; i < instanceCount; i++)
                {
                    ref readonly var instance = ref Unsafe.Add(ref instances, i);
                    if (instance.State > NoteStatus.Start && instance.State < NoteStatus.End)
                    {
                        instance.OnLateUpdate();
                    }
                }
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnUpdate()
        {

        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnPreUpdate()
        {

        }
    }
}
