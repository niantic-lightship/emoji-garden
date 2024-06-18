// Copyright 2022-2024 Niantic.
using System;
using Niantic.Lightship.AR.VpsCoverage;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class UIController : MonoBehaviour
    {
        [SerializeField]
        private LightshipCircleButton recenterButton;

        [SerializeField]
        private LoadingScreen _loadingScreen;

        [SerializeField]
        private RectTransform UITransform;

        [SerializeField]
        private VpsTargetCardHolder targetCardHolder;

        [SerializeField]
        private ToastController toastController;

        public Action CardDidHideResponse;
        
        public void OnEnable()
        {
            targetCardHolder.CardDidHide += CardDidHide;
            targetCardHolder.CardOutOfRange += CardOutOfRange;
            targetCardHolder.CardShouldHide += HideCard;
        }
        
        public void OnDisable()
        {
            targetCardHolder.CardShouldHide -= HideCard;
            targetCardHolder.CardDidHide -= CardDidHide;
            targetCardHolder.CardOutOfRange -= CardOutOfRange;
        }
        
        private void CardDidHide()
        {
           CardDidHideResponse?.Invoke();
        }
        
        
        private void CardOutOfRange()
        {
            toastController.DisplayToast("Move closer to the Public Location to start");
        }
        
        public void Init()
        {
            targetCardHolder.Init();
            toastController.TargetTransform = UITransform;
            recenterButton.gameObject.SetActive(false);
        }
        
        public void HideLoadingScreen()
        {
            if (_loadingScreen.isActiveAndEnabled)
            {
                _loadingScreen.Hide();
            }
        }

        public void UpdateCardDistance(LocationInformation locationInformation)
        {
            targetCardHolder.UpdateDisplayedCard(locationInformation.LatLng);
        }

        public void ActivateRecenterButton()
        {
            recenterButton.ButtonBecomesActive();
        }

        public void DeactivateRecenterButton()
        {
            recenterButton.ButtonBecomesInactive();
        }

        public void DisplayToast(string toastMessage)
        {
            toastController.DisplayToast(toastMessage);
        }

        public void DisplayToast(string toastMessage, float timeoutInSeconds)
        {
            toastController.DisplayToast(toastMessage, timeoutInSeconds);
        }

        public void HideToast()
        {
            toastController.HideToast();
        }

        public void HideCard()
        {
            if (targetCardHolder.IsHolderDisplayed)
            {
                if (targetCardHolder.IsToastDisplayed)
                {
                    HideToast();
                }

                targetCardHolder.Hide();
            }
        }

        public void FillMapCard
        (
            VpsTargetMapMarker targetMapMarker,
            LatLng userLocation,
            Action BeforeDidSelectLocation,
            Action DidSelectLocation
        )
        {
            targetCardHolder.DisplayCardForMarker
            (
                targetMapMarker,
                userLocation,
                BeforeDidSelectLocation, 
                DidSelectLocation
            );
        }

        public VpsTargetCard DisplayPrefilledCard(VpsTargetCard cardPrefab)
        {
           return targetCardHolder.DisplayPrefilledCard(cardPrefab);
        }
        
        public void DisplayNetworkError() 
        {
            toastController.DisplayToast("Please check your internet connection before continuing");
        }
    }
}