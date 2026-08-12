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
        float _pendingReleaseHeldDuration;
        float _clickCompletionDelay;
        float _releaseCompletionDelay;
        bool _isClickPending;
        bool _isReleasePending;
        bool _completeClickNextFrame;
        bool _completeReleaseNextFrame;
        bool _suppressUntilRelease;

        public bool IsPressed { get; private set; }
        public bool PressedThisFrame { get; private set; }
        public bool ReleasedThisFrame { get; private set; }
        public bool ClickCompletedThisFrame { get; private set; }
        public bool ReleaseCompletedThisFrame { get; private set; }
        public float ReleaseHeldDuration { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Update(bool isPressed, bool pressedThisFrame, bool releasedThisFrame, float deltaTime)
        {
            if (_suppressUntilRelease)
            {
                if (!isPressed)
                {
                    _suppressUntilRelease = false;
                }
                return;
            }

            ClickCompletedThisFrame = _completeClickNextFrame;
            ReleaseCompletedThisFrame = _completeReleaseNextFrame;
            ReleaseHeldDuration = ReleaseCompletedThisFrame ? _pendingReleaseHeldDuration : 0f;
            _completeClickNextFrame = false;
            _completeReleaseNextFrame = false;

            var elapsed = deltaTime > 0f ? deltaTime : 0f;
            if (_isClickPending)
            {
                _clickCompletionDelay -= elapsed;
                if (_clickCompletionDelay <= 0f)
                {
                    _isClickPending = false;
                    _completeClickNextFrame = true;
                }
            }
            if (_isReleasePending)
            {
                _releaseCompletionDelay -= elapsed;
                if (_releaseCompletionDelay <= 0f)
                {
                    _isReleasePending = false;
                    _completeReleaseNextFrame = true;
                }
            }
            if (pressedThisFrame)
            {
                _heldDuration = 0f;
                if (!_isClickPending && !_completeClickNextFrame && !ClickCompletedThisFrame)
                {
                    _clickCompletionDelay = InputManager.UI_CLICK_ANIMATION_DURATION_SEC * 2f;
                    _isClickPending = true;
                }
            }
            if (isPressed)
            {
                _heldDuration += elapsed;
            }
            if (releasedThisFrame)
            {
                var releasedHeldDuration = _heldDuration;
                _heldDuration = 0f;
                if (!_isReleasePending && !_completeReleaseNextFrame && !ReleaseCompletedThisFrame)
                {
                    _pendingReleaseHeldDuration = releasedHeldDuration;
                    _releaseCompletionDelay = InputManager.UI_CLICK_ANIMATION_DURATION_SEC;
                    _isReleasePending = true;
                }
            }

            IsPressed = isPressed;
            PressedThisFrame = pressedThisFrame;
            ReleasedThisFrame = releasedThisFrame;
        }

        internal void ResetForSceneChange(bool isCurrentlyPressed)
        {
            this = default;
            _suppressUntilRelease = isCurrentlyPressed;
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
