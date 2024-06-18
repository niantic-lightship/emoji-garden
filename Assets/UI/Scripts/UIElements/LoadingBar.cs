// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class LoadingBar: MonoBehaviour
    {
        [SerializeField]
        private RectTransform barTransform;

        [SerializeField]
        private RectTransform sliderTransform;

        [SerializeField]
        private AnimationCurve _curve;

        [SerializeField]
        private float _appearanceTime;

        [SerializeField]
        private float _barPeriod;
        // Start is called before the first frame update

        public void BeginLoading()
        {

            Action animateBar = () => { StartCoroutine(AnimateSliderBar(_barPeriod)); };
            StartCoroutine(DisplayLoadingBar(0, 5, animateBar));
        }

        public void EndLoading()
        {
            StopAllCoroutines();
        }

        private IEnumerator DisplayLoadingBar(float startHeight, float targetHeight, Action cb)
        {
            SetBarLocation(-1f);
            SetHeight(barTransform, startHeight);
            float elaspedTime = 0;
            while (elaspedTime <= _appearanceTime)
            {
                yield return null;

                elaspedTime += Time.deltaTime;
                float newHeight = Mathf.Lerp
                    (startHeight, targetHeight, _curve.Evaluate(elaspedTime / _appearanceTime));

                SetHeight(barTransform, newHeight);
            }

            SetHeight(barTransform, targetHeight);
            cb?.Invoke();
        }

        private IEnumerator AnimateSliderBar(float periodDuration)
        {
            var elapsedTime = 0f;
            while (true)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.PingPong(elapsedTime, periodDuration) / periodDuration;
                float easedValue = _curve.Evaluate(normalizedTime);
                float finalValue = 2f * easedValue - 1f;
                SetBarLocation(finalValue);
                yield return null;
            }
        }

        private void SetHeight(RectTransform rectTransform, float newHeight)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
        }

        private void SetBarLocation(float location)
        {
            // Calculate the maximum possible offset
            float maxOffset = barTransform.rect.width - sliderTransform.rect.width;

            // Calculate the target position. This assumes the pivot of the child is at its center.
            float targetXPosition = location * maxOffset * 0.5f;

            // Set the new position
            sliderTransform.anchoredPosition = new Vector2
                (targetXPosition, sliderTransform.anchoredPosition.y);
        }
    }
}