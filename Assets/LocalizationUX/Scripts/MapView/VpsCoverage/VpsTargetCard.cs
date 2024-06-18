// Copyright 2022-2024 Niantic.
using UnityEngine;
using System;
using Niantic.Lightship.AR.VpsCoverage;
using TMPro;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples {
    public class VpsTargetCard : MonoBehaviour
    {
        [SerializeField]
        private LightshipButton _activationButton;

        [SerializeField]
        private RawImage wayspotImage;

        [SerializeField]
        private TMP_Text titleLabel;

        [SerializeField]
        private TMP_Text distanceLabel;

        [SerializeField]
        private RectTransform titleLabelRect;

        [SerializeField]
        private LayoutElement _distanceLayoutElement;

        private Animator _buttonAnimator;
        private LocalizationTarget _target;
        private const float _distanceActivationThreshold = 50f;

        public Action BeforeDidSelectLocation;
        public Action DidSelectLocation;

        public string TitleLabelText
        {
            get => titleLabel.text;
            set => titleLabel.text = value;
        }

        public string DistanceLabelText
        {
            get => distanceLabel.text;
            set => distanceLabel.text = value;
        }

        public float DistanceActivationThreshold
        {
            get => _distanceActivationThreshold;
        }

        public LocalizationTarget Target
        {
            get => _target;
        }
        
        public void Init(VpsTargetMapMarker targetMapMarker)
        {
            FillCardValues(targetMapMarker);
        }

        public void Update()
        {
            MatchHeight(titleLabelRect, _distanceLayoutElement);
        }
        
        public void RescaleImage()
        {
            var parent = wayspotImage.transform.parent.GetComponentInParent<RectTransform>();
            var imageTransform = wayspotImage.GetComponent<RectTransform>();
            float w = 0, h = 0;
            float ratio = wayspotImage.texture.width / (float) wayspotImage.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }

            w = bounds.width;
            h = w / ratio;
            imageTransform.sizeDelta = new Vector2(w, h);
        }
        
        public void AnimationCompletedNextButtonClicked()
        {
            DidSelectLocation?.Invoke();
        }

        public void BeforeAnimationNextButtonClick()
        {
            BeforeDidSelectLocation?.Invoke();
        }
        
        private void FillCardValues(VpsTargetMapMarker targetMapMarker)
        {
             _target = targetMapMarker.Target;
            transform.name = _target.Name;

            wayspotImage.texture = targetMapMarker.MarkerHintImage();
            RescaleImage();

            TitleLabelText = _target.Name;
        }

        public void UpdateUserDistance(double distance)
        {
            //Distance must be in increments of 10.
            if (distance < 10)
            {
                DistanceLabelText = "< 10 m";
            }
            else
            {
                var distanceInTens = (int) Math.Round(distance / 10.0) * 10;
                DistanceLabelText = distanceInTens.ToString("N0") + " m";
                
                if (distanceInTens >= _distanceActivationThreshold)
                {
                    _activationButton.SetInteractable(false, true);
                }
                else
                {
                    _activationButton.SetInteractable(true, true);
                }
            }
        }
        
        private void MatchHeight(RectTransform sourceRect, LayoutElement targetLayoutElement)
        {
            if (sourceRect != null)
            {
                var h = sourceRect.rect.height;
                targetLayoutElement.preferredHeight = h;
            }
        }
    }
}
