// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using Niantic.Lightship.AR.VpsCoverage;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples {
    [RequireComponent(typeof(RectTransform))]
    public class VpsTargetCardHolder : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _curve;

        [SerializeField]
        private float appearanceTime = 2f;

        [SerializeField]
        private VpsTargetCard targetCardPrefab;
        
        private RectTransform _targetRect;
        private float _rectHeight;
        private bool _isHolderDisplayed = false;
        private VpsTargetCard _displayedTargetCard;
        private Coroutine _activeCoroutine;
        private float _currentVerticalPercentage;
        private bool _isToastDisplayed = false;

        public Action CardDidHide;
        public Action CardOutOfRange;
        public Action CardShouldHide;
        
        public bool ToastRadiator;
        public bool IsToastDisplayed
        {
            get => _isToastDisplayed;
        }

        public bool IsHolderDisplayed
        {
            get => _isHolderDisplayed;
        }
        
        public void Awake()
        {
            _targetRect = this.GetComponent<RectTransform>();
            _rectHeight = _targetRect.rect.height;
        }

        public void UpdateDisplayedCard(LatLng location)
        {
            if (_isHolderDisplayed) {
                double distanceInM = _displayedTargetCard.Target.Center.Distance(location);
                _displayedTargetCard.UpdateUserDistance(distanceInM);
                if (distanceInM >= _displayedTargetCard.DistanceActivationThreshold)
                {
                    DisplayDistanceToast();
                }
            }
        }

        public void DisplayCardForMarker(VpsTargetMapMarker marker, LatLng userLocation, Action beforeDidSelectLocation, Action didSelectLocation)
        {
            //is the card list already displayed
            if (_isHolderDisplayed)
            {
                Action didComplete = () =>
                {
                    Destroy(_displayedTargetCard.gameObject);
                    _isHolderDisplayed = false;
                    DisplayCardForMarker(marker, userLocation, beforeDidSelectLocation, didSelectLocation);
                };
                HideCard(didComplete);
            }
            else
            {
                _isToastDisplayed = false;
                var newCard = CreateCardForMarker(marker, userLocation, beforeDidSelectLocation, didSelectLocation);
                Action didComplete = () =>
                {
                    _isHolderDisplayed = true;
                    _displayedTargetCard = newCard;
                };
                DisplayCard(didComplete);
            }
        }

        public VpsTargetCard DisplayPrefilledCard(VpsTargetCard cardPrefab)
        {
            var newCard = Instantiate(cardPrefab, this.transform);
            Action didComplete = () =>
            {
                _isHolderDisplayed = true;
                _displayedTargetCard = newCard;
            };
            DisplayCard(didComplete);
            return newCard;
        }

        private VpsTargetCard CreateCardForMarker(VpsTargetMapMarker marker, LatLng userLocation, Action beforeDidSelectionLocation, Action didSelectionLocation)
        {
            var targetCard = Instantiate(targetCardPrefab, this.transform);
            targetCard.Init(marker);
            targetCard.BeforeDidSelectLocation += beforeDidSelectionLocation;
            targetCard.DidSelectLocation += didSelectionLocation;
            return targetCard;
        }

        public void DragEndHide(Vector2 dragDistance, GUIDragHandler.DraggedDirection direction)
        {
            if (direction == GUIDragHandler.DraggedDirection.Down)
            {
                CardShouldHide?.Invoke();
            }
        }

        public void PartialHide(Vector2 dragDistance, GUIDragHandler.DraggedDirection direction)
        {
            float maxDown = -1000f;
            var vertPercentage = 1 - Mathf.Clamp((dragDistance.y / maxDown),0,1);
            VerticalPosition(vertPercentage);
        }
        
        public void Hide()
        {
            Action didComplete = () =>
            {
                if (_displayedTargetCard != null) 
                {
                    Destroy(_displayedTargetCard.gameObject);
                }
                _isHolderDisplayed = false;
                _isToastDisplayed = false;
                CardDidHide?.Invoke();

            };
            HideCard(didComplete);
        }
        
        public void Init()
        {
            VerticalPosition(0);
        }
        
        private void DisplayDistanceToast()
        {
            if (_isToastDisplayed)
            {
                return;
            }
            CardOutOfRange?.Invoke();
            _isToastDisplayed = true;
        }

        private void DisplayCard(Action cb)
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }
            _activeCoroutine = StartCoroutine(AnimateCard(1, cb));
        }

        private void HideCard(Action cb)
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }
            _activeCoroutine = StartCoroutine(AnimateCard(0, cb));
        }

        private IEnumerator AnimateCard(float endValue, Action cb)
        {
            float animationTime = 0f;
            if (endValue == 0)
            {
                animationTime = (appearanceTime * (1 - _currentVerticalPercentage));
            }
            else
            {
                animationTime = appearanceTime * _currentVerticalPercentage;
            }
            
            // Calculate where we are on th curve based on the current vertical position
            while (animationTime <= appearanceTime)
            {
                yield return null;
                animationTime += Time.deltaTime;
                
                float curvePercentage = animationTime / appearanceTime;
                if (endValue == 0)
                {
                    curvePercentage = 1 - curvePercentage;
                }

                float displayPercentage = _curve.Evaluate(curvePercentage);
                VerticalPosition(displayPercentage);

            }
            VerticalPosition(endValue);
            cb?.Invoke();
        }

        private void VerticalPosition(float percentage)
        {
            float yValue = _rectHeight - (_rectHeight + (_rectHeight * (1 - percentage)));
            _targetRect.anchoredPosition = new Vector2(0, yValue);
            _currentVerticalPercentage = percentage;
        }
    }
}
