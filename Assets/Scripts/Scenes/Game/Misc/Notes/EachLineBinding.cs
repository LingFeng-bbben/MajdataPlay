using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Scenes.Game.Notes.Behaviours;
using System;
using System.Collections.Generic;
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

        NoteDrop? _noteInstanceA;
        NoteDrop? _noteInstanceB;
        ProviderType _providerType = ProviderType.None;
        IDistanceProvider? _distanceProvider;

        public void Bind(NoteDrop instance)
        {
            if (_noteInstanceA is null)
            {
                _noteInstanceA = instance;
            }
            else if (_noteInstanceB is null)
            {
                _noteInstanceB = instance;
            }
            if (_distanceProvider is null)
            {
                var isTap = instance is TapDrop;
                _distanceProvider = instance as IDistanceProvider;
                if (isTap)
                {
                    _providerType = ProviderType.FromTap;
                }
                else
                {
                    _providerType = ProviderType.FromHold;
                }
            }
            else if(_providerType == ProviderType.FromHold)
            {
                var isTap = instance is TapDrop;
                if(isTap)
                {
                    _providerType = ProviderType.FromTap;
                    _distanceProvider = instance as IDistanceProvider;
                }
            }
        }
        public void Unbind(NoteDrop instance)
        {
            ThrowIfNull(instance);
            if ((object?)_noteInstanceA == instance)
            {
                _noteInstanceA = null;
                _isAnyNoteEnded = true;
            }
            else if ((object?)_noteInstanceB == instance)
            {
                _noteInstanceB = null;
                _isAnyNoteEnded = true;
            }
            else
            {
                ThrowInvalidOp();
            }
        }
        void ThrowIfNull(object? reference)
        {
            if (reference is null)
            {
                throw new InvalidOperationException("Reference is null.");
            }
        }
        void ThrowInvalidOp()
        {
            throw new InvalidOperationException("Attempted to unbind a Note instance that was never bound");
        }
        enum ProviderType
        {
            None,
            FromTap,
            FromHold
        }
    }
}
