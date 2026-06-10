using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Scenes.Game.Notes.Behaviours;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    class EachLineBinding : IEachLineNoteBinding, IEachLineDistanceProvider
    {
        float IEachLineDistanceProvider.Distance
        {
            get
            {
                return _distanceProvider?.Distance ?? 0f;
            }
        }
        bool IEachLineDistanceProvider.IsAnyNoteEnded
        {
            get
            {
                return _isAnyNoteEnded;
            }
        }

        bool _isAnyNoteEnded = false;

        ProviderType _providerType = ProviderType.None;
        IDistanceProvider? _distanceProvider;
        
        int _noteCount = 0;
        readonly NoteDrop?[] _noteInstances = new NoteDrop[8];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(NoteDrop instance)
        {
            if (instance is null)
            {
                ThrowArgumentNull();
                return;
            }
            if (_noteCount < 8)
            {
                _noteInstances[_noteCount++] = instance;
            }

            var isTap = instance is TapDrop;

            if (instance is IDistanceProvider provider)
            {
                if (_distanceProvider is null)
                {
                    _providerType = isTap ? ProviderType.FromTap : ProviderType.FromHold;
                    _distanceProvider = provider;
                }
                else if (_providerType == ProviderType.FromHold && isTap)
                {
                    _providerType = ProviderType.FromTap;
                    _distanceProvider = provider;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unbind(NoteDrop instance)
        {
            if (instance is null)
            {
                ThrowArgumentNull();
                return;
            }

            for (var i = 0; i < _noteCount; i++)
            {
                ref var currentInstance = ref _noteInstances[i];
                if ((object?)currentInstance == instance)
                {
                    _noteCount--;
                    ref var lastInstance = ref _noteInstances[_noteCount];
                    currentInstance = lastInstance;
                    lastInstance = null;

                    _isAnyNoteEnded = true;
                    if(_noteCount == 0)
                    {
                        _distanceProvider = null;
                    }
                    return;
                }
            }

            ThrowInvalidOp();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowArgumentNull()
        {
            throw new ArgumentNullException("Reference is null.");
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowInvalidOp()
        {
            throw new InvalidOperationException("Attempted to unbind a Note instance that was never bound");
        }
        enum ProviderType : byte
        {
            None = 0,
            FromTap = 1,
            FromHold = 2
        }
    }
}
