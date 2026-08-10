using System.Runtime.CompilerServices;

namespace MajdataPlay.IO
{
    /// <summary>
    /// A frame snapshot for a physical button or touch sensor.
    /// Updated once by <see cref="InputManager"/> after raw input has been sampled.
    /// </summary>
    internal struct InputControlState
    {
        float _heldDuration;
        float _pendingClickHeldDuration;
        float _clickCompletionDelay;
        bool _isClickPending;
        bool _completeClickNextFrame;

        public bool IsPressed { get; private set; }
        public bool PressedThisFrame { get; private set; }
        public bool ReleasedThisFrame { get; private set; }
        public bool ClickCompletedThisFrame { get; private set; }
        public float ClickHeldDuration { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Update(bool isPressed, bool pressedThisFrame, bool releasedThisFrame, float deltaTime)
        {
            ClickCompletedThisFrame = _completeClickNextFrame;
            ClickHeldDuration = ClickCompletedThisFrame ? _pendingClickHeldDuration : 0f;
            _completeClickNextFrame = false;

            var elapsed = deltaTime > 0f ? deltaTime : 0f;
            if (pressedThisFrame)
            {
                _heldDuration = 0f;
                _isClickPending = false;
            }
            if (isPressed)
            {
                _heldDuration += elapsed;
            }
            if (releasedThisFrame)
            {
                _pendingClickHeldDuration = _heldDuration;
                _heldDuration = 0f;
                _clickCompletionDelay = InputManager.UI_CLICK_ANIMATION_DURATION_SEC;
                _isClickPending = true;
            }
            else if (_isClickPending)
            {
                _clickCompletionDelay -= elapsed;
                if (_clickCompletionDelay <= 0f)
                {
                    _isClickPending = false;
                    _completeClickNextFrame = true;
                }
            }

            IsPressed = isPressed;
            PressedThisFrame = pressedThisFrame;
            ReleasedThisFrame = releasedThisFrame;
        }
    }

    /// <summary>
    /// Small, callback-free helper for directional menu input with key repeat.
    /// </summary>
    internal struct InputRepeatState
    {
        const float MIN_INTERVAL = 0.001f;

        int _direction;
        float _heldDuration;
        float _nextRepeatAt;
        bool _suppressUntilRelease;

        public bool Update(
            bool positivePressed,
            bool negativePressed,
            float deltaTime,
            float delay,
            float interval,
            out int direction)
        {
            return Update(
                positivePressed,
                negativePressed,
                deltaTime,
                delay,
                interval,
                interval,
                out direction);
        }

        public bool Update(
            bool positivePressed,
            bool negativePressed,
            float deltaTime,
            float delay,
            float firstRepeatInterval,
            float repeatInterval,
            out int direction)
        {
            direction = positivePressed ? 1 : negativePressed ? -1 : 0;
            if (direction == 0)
            {
                Reset();
                return false;
            }
            if (_suppressUntilRelease)
            {
                return false;
            }
            if (direction != _direction)
            {
                if (_direction != 0)
                {
                    _direction = direction;
                    return false;
                }

                _direction = direction;
                _heldDuration = 0f;
                var initialInterval = firstRepeatInterval > MIN_INTERVAL ? firstRepeatInterval : MIN_INTERVAL;
                _nextRepeatAt = delay + initialInterval;
                return true;
            }

            _heldDuration += deltaTime > 0f ? deltaTime : 0f;
            if (_heldDuration < _nextRepeatAt)
            {
                return false;
            }

            _nextRepeatAt += repeatInterval > MIN_INTERVAL ? repeatInterval : MIN_INTERVAL;
            return true;
        }

        public void SuppressUntilRelease()
        {
            Reset();
            _suppressUntilRelease = true;
        }

        public void Reset()
        {
            _direction = 0;
            _heldDuration = 0f;
            _nextRepeatAt = 0f;
            _suppressUntilRelease = false;
        }
    }

}
