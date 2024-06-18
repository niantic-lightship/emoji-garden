// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class WayfindingFTUEController: MonoBehaviour
    {
        [SerializeField]
        private Sprite icon;

        [SerializeField]
        private VpsTargetCard FTUE_Target_Card;

        private const string FTUE_KEY = "FTUE_HAS_RUN";
        private UIController _uiController;
        private VpsTargetCard card;
        private VpsTargetMapMarker marker;
        private MapContentController _mapContentController;
        public Action<bool> FTUEComplete;
        public Action FTUEIsHiding;

        public void RunFTUE
        (
            MapContentController mapContentController,
            LocationController locationController,
            UIController uiController,
            MapController mapController
        )
        {
            _uiController = uiController;
            //one - check our data model to see if the FTUE has already been completed.
            if (CheckFTUEState())
            {
                FTUEDidComplete();
            }
            else
            {
                StartCoroutine(ConstructFTUEIcon(mapContentController, locationController, mapController));
            }
        }

        private IEnumerator ConstructFTUEIcon
            (MapContentController mapContentController, LocationController locationController, MapController mapController)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            marker = mapContentController.AddNonVPSTargetToMap
                ("FTUE", locationController.GetUserLocation().LatLng);

            marker.MarkerDidAppear += MarkerDidAppearCallback;
            marker.Initialize(icon, Vector3.one);
            _mapContentController = mapContentController;
            mapController.CenterMapOnLocation(LocalizationUtils.ToMapsLatLng(locationController.GetUserLocation().LatLng));
        }

        private void MarkerDidAppearCallback(VpsTargetMapMarker marker)
        {
            StartCoroutine(DisplayCardAfterWait());
        }

        private IEnumerator DisplayCardAfterWait()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            marker.AnimateTarget(1.5f);
            marker.MarkerWasHit += MarkerHitResponse;
            _mapContentController.ForceSelectedMarker(marker);
        }

        private void MarkerHitResponse()
        {
            card = _uiController.DisplayPrefilledCard(FTUE_Target_Card);
            card.DidSelectLocation += FTUEDidComplete;
        }

        private void FTUEDidComplete()
        {
            if (marker != null)
            {
                // Hide the card
                _mapContentController.DeselectMarker();
                FTUEIsHiding?.Invoke();
                _uiController.HideCard();

                // Hide Icon
                marker.HideTarget(IconDidHide);
            }
            else
            {
                FTUEComplete?.Invoke(true);
            }
        }

        private void IconDidHide(VpsTargetMapMarker mapMarker)
        {
            Destroy(card.gameObject);
            mapMarker.gameObject.SetActive(false);
            SaveFTUEState(1);
            FTUEComplete?.Invoke(true);
        }

        private void SaveFTUEState(int val)
        {
            PlayerPrefs.SetInt(FTUE_KEY, val);
            PlayerPrefs.Save();
        }

        private bool CheckFTUEState()
        {
            return false; //This is set to return false so the FTUE always runs.
            //return PlayerPrefs.HasKey(FTUE_KEY);
        }
    }
}