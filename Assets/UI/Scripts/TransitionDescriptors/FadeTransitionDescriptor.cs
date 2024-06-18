// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeTransitionDescriptor : LightshipUITransitionDescriptor
    {
        [SerializeField]
        private AnimationCurve appearanceCurve;

        private CanvasGroup _canvasGroup;
        private Coroutine _activeCoroutine;
        private float currentDisplayValue = 1;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            SetAlphaValue(0);
        }
        
        public override void TransitionIn(Action callback)
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            _activeCoroutine = StartCoroutine(FadeTransition(1, callback));
        }

        public override void TransitionOut(Action callback)
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            Action resetCallback = () =>
            {
                _activeCoroutine = null;
                callback?.Invoke();
            };

            if (gameObject.activeInHierarchy)
            {
                _activeCoroutine = StartCoroutine(FadeTransition(0, resetCallback));
            }
            else
            {
                _activeCoroutine = null;
                callback?.Invoke();
            }
        }

        private void SetAlphaValue(float curvePercentage)
        {
            float alphaVal = appearanceCurve.Evaluate(curvePercentage);
            currentDisplayValue = alphaVal;
            _canvasGroup.alpha = alphaVal;
        }

        private IEnumerator FadeTransition(float targetValue, Action cb)
        {
            float elapsedPercentage = 1 - Math.Abs(targetValue - currentDisplayValue);
            float elapsedTime = +(transitionLength * elapsedPercentage);
            while (elapsedTime <= transitionLength)
            {
                elapsedTime += Time.deltaTime;
                elapsedPercentage = elapsedTime / transitionLength;
                if (targetValue == 0)
                {
                    elapsedPercentage = 1 - elapsedPercentage;
                }

                SetAlphaValue(elapsedPercentage);
                yield return null;
            }

            SetAlphaValue(targetValue);
            cb?.Invoke();
        }
    }
}