// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Tweens the scale of a game object
    /// This is used to scale the head of the plant when the user taps on it
    /// Usually this would be controlled by a tween engine such as DOTween
    /// </summary>
    public class ScaleTweener : MonoBehaviour
    {
        [SerializeField]
        private float _tweenTime = 0.5f;

        // Used to keep the start scale value - not used if the scale needs to be set through a script
        [SerializeField]
        private bool _isScaleStoredOnStart = true;

        private float _tweenProgress = 0;

        private Vector3 _initialScale;
        private Vector3 _targetScale;

        public Vector3 ScaleToKeep { get; set; } = Vector3.one;

        public bool IsTweening { get; private set; } = false;

        // Keep track of the original scale if required
        private void Start()
        {
            if (_isScaleStoredOnStart)
            {
                ScaleToKeep = transform.localScale;
            }
        }

        // Performed in LateUpdate to override any animation values that are unintentionally set
        private void LateUpdate()
        {
            if (IsTweening)
            {
                // Progress the tween by the normalized time in order to keep it as a percentage
                // Claped between 0 and 1 so the tween can't overshoot
                _tweenProgress = Mathf.Clamp01(_tweenProgress + (Time.deltaTime / _tweenTime));

                float easedTween = EaseOutBack(_tweenProgress);

                // Set the scale of the object based on the eased tween value
                transform.localScale = Vector3.LerpUnclamped(_initialScale, _targetScale, easedTween);

                // If the tween is complete then store a new ScaleToKeep and stop tweening
                if (_tweenProgress >= 1)
                {
                    ScaleToKeep = transform.localScale;
                    IsTweening = false;
                }
            }
            else
            {
                // Set the scale to ScaleToKeep to ignore any unintentional animation values
                transform.localScale = ScaleToKeep;
            }
        }

        // Attempt to start a new tween, if not tweening already
        public void StartScaleTween(float scale, bool resetProgress = true)
        {
            if (resetProgress)
            {
                _tweenProgress = 0;
            }

            _initialScale = transform.localScale;
            ScaleToKeep = _targetScale = Vector3.one * scale;

            IsTweening = true;
        }

        public void SetScale(float scale)
        {
            IsTweening = false;
            transform.localScale = ScaleToKeep = _targetScale = Vector3.one * scale;
        }

        // Easing function for tween. Overshoots slightly, then returns to 1
        private float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return 1 + (c3 * Mathf.Pow(x - 1, 3)) + (c1 * Mathf.Pow(x - 1, 2));
        }
    }
}
