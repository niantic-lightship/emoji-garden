// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples 
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve curve;

        [SerializeField]
        private float animationLength;

        [SerializeField]
        private LoadingBar loadingBar;

        [SerializeField]
        private GameObject topoCover;

        private Coroutine activeCoroutine;
        private CanvasGroup canvasGroup;
        private CompassLowpassFilter _compassFilter;
        private double filteredCompassHeading = 0.0;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Start()
        {
            _compassFilter = new CompassLowpassFilter();
            loadingBar.BeginLoading();
            
        }
        
        public void Update()
        {
            // Uncomment this section to enable compass heading control
            //UpdateCompassHeading();
            //SetOrientationRotation(Quaternion.Euler(-90, 0, (float) filteredCompassHeading));
        }

        public void Hide()
        {
            if (activeCoroutine != null)
            {
                return;
            }

            Action didComplete = () =>
            {
                gameObject.SetActive(false);
                activeCoroutine = null;

            };
            activeCoroutine = StartCoroutine(AnimateDisplay(0, didComplete));
        }
        
        private void UpdateCompassHeading()
        {
            if (!Input.compass.enabled)
            {
                // Early-out if compass is disabled
                return;
            }
            
            // Get the device's raw compass heading
            var compassHeading = Input.compass.trueHeading;
            
            // Add the current compass heading as a sample,
            // and get a filtered value this method computes.
            _compassFilter.AddSampleDegrees(compassHeading);
            filteredCompassHeading = _compassFilter.Degrees;
        }
        
        private void SetOrientationRotation(Quaternion rotation)
        {
            topoCover.transform.rotation = rotation;
        }

        private IEnumerator AnimateDisplay(float endValue, Action cb)
        {
            float elapsedTime = 0;
            while (elapsedTime <= animationLength)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                float displayPercentage = curve.Evaluate(elapsedTime / animationLength);
                if (endValue == 0)
                {
                    displayPercentage = 1 - displayPercentage;
                }

                SetAlpha(displayPercentage);
            }

            SetAlpha(endValue);
            cb?.Invoke();
        }

        private void SetAlpha(float val)
        {
            canvasGroup.alpha = val;
        }
    }
}