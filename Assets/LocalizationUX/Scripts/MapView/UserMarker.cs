// Copyright 2022-2024 Niantic.
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class UserMarker : MonoBehaviour
    {
        [SerializeField]
        private GameObject _headingIndicator;

        [SerializeField]
        private GameObject _shadowIndicator;

        [SerializeField]
        private GameObject _centerDot;

        [SerializeField]
        private AnimationCurve _curve;
        
        private const float ShadowOffsetMagnitude = -0.02f;
        private CompassLowpassFilter _compassFilter;
        private double filteredCompassHeading;

        private void Start()
        {
            _compassFilter = new CompassLowpassFilter();
        }

        private void Update()
        {
            UpdateCompassHeading();
            SetOrientationRotation(Quaternion.Euler(-90, 0, (float)filteredCompassHeading));
            UpdateShadowPosition();
        }
        
        public void LoadingAnimation()
        {
            StartCoroutine(RotationAnimation(2f));
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
        
        private void UpdateShadowPosition()
        {
            float theta = (float)(filteredCompassHeading * Mathf.Deg2Rad); // Convert degree to radians
            // Calculate the offset for the shadow based on the current compass heading
            float offsetX = ShadowOffsetMagnitude * Mathf.Sin(theta);
            float offsetY = ShadowOffsetMagnitude * Mathf.Cos(theta);

            // Set the new position of the shadow relative to the triangle's position
            _shadowIndicator.transform.localPosition = Vector3.zero + new Vector3(offsetX, offsetY, 0);
        }
        
        private void SetOrientationRotation(Quaternion rotation)
        {
            _headingIndicator.transform.rotation = rotation;
        }

        private IEnumerator RotationAnimation(float duration)
        {
            _centerDot.transform.localEulerAngles = new Vector3(0, 0, 0);
            float elapsed = 0f;
            float startRotation = _centerDot.transform.localEulerAngles.y;
            float endRotation = startRotation + 720f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float curveAmount = _curve.Evaluate(elapsed / duration);
                float currentRotation = Mathf.Lerp(startRotation, endRotation, curveAmount);
                _centerDot.transform.localEulerAngles = new Vector3(0, currentRotation, 0);
                yield return null;
            }

            // Ensure it ends up exactly at the desired rotation
           _centerDot.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
}