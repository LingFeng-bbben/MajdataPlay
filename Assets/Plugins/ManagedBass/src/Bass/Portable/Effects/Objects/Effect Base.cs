using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// Wraps a Bass Effect such that you don't need to touch the Bass functions to Handle it.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IEffectParameter"/></typeparam>
    public abstract class Effect<T> : INotifyPropertyChanged, IDisposable where T : class, IEffectParameter, new()
    {
        int _channel, _effectHandle, _hfsync;
        GCHandle _gch;
        
        /// <summary>
        /// Effect's Parameters.
        /// </summary>
        protected T Parameters { get; private set; } = new T();
        
        /// <summary>
        /// Applies the Effect on a <paramref name="Channel"/>.
        /// </summary>
        /// <param name="Channel">The Channel to apply the Effect on.</param>
        /// <param name="Priority">Priority of the Effect in DSP chain.</param>
        public void ApplyOn(int Channel, int Priority = 0)
        {
            _channel = Channel;
            _priority = Priority;

            if (!_gch.IsAllocated)
                _gch = GCHandle.Alloc(Parameters, GCHandleType.Pinned);
            
            _hfsync = Bass.ChannelSetSync(Channel, SyncFlags.Free, 0, (a, b, c, d) => Dispose());
        }
                
        int _priority;

        /// <summary>
        /// Priority of the Effect in DSP chain.
        /// </summary>
        public int Priority
        {
            get => _priority;
            set
            {
                if (IsActive && Bass.FXSetPriority(_effectHandle, value))
                    _priority = value;
            }
        }
        
        /// <summary>
        /// Removes the effect from the Channel and frees allocated memory.
        /// </summary>
        public void Dispose()
        {
            Bass.ChannelRemoveSync(_channel, _hfsync);

            _channel = _effectHandle = 0;

            if (_gch.IsAllocated)
                _gch.Free();
        }
        
        /// <summary>
        /// Sets the effect parameters to default by initialising a new instance of <typeparamref name="T"/>.
        /// </summary>
        public void Default()
        {
            // Reallocate memory for Parameters
            _gch.Free();
            Parameters = new T();
            _gch = GCHandle.Alloc(Parameters, GCHandleType.Pinned);

            OnPreset();
        }

        /// <summary>
        /// Checks whether the effect is currently enabled and active.
        /// </summary>
        public bool IsActive
        {
            set
            {
                if (_channel == 0)
                    return;

                if (value && !IsActive)
                    _effectHandle = Bass.ChannelSetFX(_channel, Parameters.FXType, 1);
                
                else if (!value && IsActive && Bass.ChannelRemoveFX(_channel, _effectHandle))
                    _effectHandle = 0;

                OnPropertyChanged();
            }
            get => _channel != 0 && _effectHandle != 0;
        }
        
        /// <summary>
        /// Called after applying a Preset to notify that multiple properties have changed.
        /// </summary>
        protected void OnPreset() => OnPropertyChanged("");

        /// <summary>
        /// Fired when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}