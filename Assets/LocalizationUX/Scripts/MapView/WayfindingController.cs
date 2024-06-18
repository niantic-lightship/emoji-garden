// Copyright 2022-2024 Niantic.
// Copyright 2023 Niantic, Inc. All Rights Reserved.
using System;
using Niantic.Lightship.AR.VpsCoverage;
using UnityEngine;
using MapsLatLng = Niantic.Lightship.Maps.Core.Coordinates.LatLng;

namespace Niantic.Lightship.AR.Samples {
    public class WayfindingController : MonoBehaviour
    {
        [Tooltip("Should experimental Public Locations be included in coverage API results?")]
        [SerializeField]
        private bool productionOnly = false;

        [Tooltip("Should the map FTUE run?")]
        [SerializeField]
        private bool runFTUE;

        [Header("Map Recenter Settings")]
        [Tooltip("How many meters off the user can the map be panned before the recenter button becomes active")]
        [SerializeField]
        private float _recenterOffsetThreshold;

        [Header("Controller Connections")]
        [SerializeField]
        private CoverageClientManager coverageClientManager;

        [SerializeField]
        private UIController uiController;

        [SerializeField]
        private LocationController locationController;

        [SerializeField]
        private MapController mapController;

        [SerializeField]
        private MapContentController mapContentController;

        [SerializeField]
        private WayfindingFTUEController ftueController;
        
        private enum InitializationState
        {
            NotInitialized,
            Initalizing,
            Initizialized
        };

        //Data Model
        private InitializationState _initializationState = InitializationState.NotInitialized;
        private int _startingCoverageRadius = 200;
        private int _followUpCoverageRadius = 1000;
        private float _errorToastDuration = 2.5f;
        private bool _interactionsConnected = false;
        
        public Action BeforeDidSelectLocation;
        public Action DidSelectLocation;
        
        public void ShowMap()
        {
            gameObject.SetActive(true);
        }

        public void HideMap()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ConnectInteractions();
            mapController.MapIsReady += MapIsReady;
            locationController.LocationServiceAvailable += OnLocationService;
            mapContentController.MarkerWasSelected += MarkerWasSelected;
            mapContentController.MarkerWasDeselected += MarkerWasDeselected;
            mapContentController.AllMarkersHaveAdded += AllMarkersHaveAdded;
            uiController.CardDidHideResponse += CardDidHideResponse;
            ftueController.FTUEComplete += FTUEDidComplete;
            ftueController.FTUEIsHiding += FTUEIsHiding;
        }
        
        private void OnDisable()
        {
            DisconnectInteractions();
            mapController.MapIsReady += MapIsReady;
            locationController.LocationServiceAvailable -= OnLocationService;
            mapContentController.MarkerWasSelected -= MarkerWasSelected;
            mapContentController.MarkerWasDeselected -= MarkerWasDeselected;
            mapContentController.AllMarkersHaveAdded -= AllMarkersHaveAdded;
            ftueController.FTUEComplete -= FTUEDidComplete;
            ftueController.FTUEIsHiding -= FTUEIsHiding;
        }

        private void Update()
        {
            if (_initializationState == InitializationState.NotInitialized)
            {
                uiController.Init();
                _initializationState = InitializationState.Initalizing;
                NetworkCheck.CheckNetworkConnection(this,NetworkDidCheck);
            }
        }

        /// Initialization Flow

        private void InitializeMapScene()
        {
            DisconnectInteractions();
            coverageClientManager.UseCurrentLocation = false;
            locationController.StartLocationService();
        }

        private void NetworkDidCheck(bool result)
        {
            if (result)
            {
                InitializeMapScene();
            }
            else
            {
                uiController.DisplayNetworkError();
            }
        }

        private void OnLocationService()
        {
            var location = locationController.GetUserLocation();
            if (location.LocationState == LocationState.Available)
            {
                MapsLatLng mapLoc = LocalizationUtils.ToMapsLatLng(location.LatLng);
                mapController.InitializeMap(mapLoc);
            }
        }

        private void MapIsReady()
        {
            uiController.HideLoadingScreen();
            ConnectInteractions();  
            var location = locationController.GetUserLocation();
            mapContentController.AddUserToMap(LocalizationUtils.ToMapsLatLng(location.LatLng));
            _initializationState = InitializationState.Initizialized;
            locationController.LocationDidUpdate += UpdateUserMarker;
            if (runFTUE)
            {
                mapController.MapDidCompletePan -= MapDidCompletePan;
                ftueController.RunFTUE(mapContentController, locationController, uiController, mapController);
            }
            else
            {
                FTUEDidComplete(false);
            }   
        }

        private void FTUEDidComplete(bool enablePan)
        {
            if (enablePan)
            {
                mapController.MapDidCompletePan += MapDidCompletePan;
                RecenterWasClicked();
            }

            var latLng = LocalizationUtils.ToMapsLatLng(locationController.GetUserLocation().LatLng);
            RequestAreas(latLng);
        }
        
        private void FTUEIsHiding()
        {   
            mapController.SmoothZoom(0.5f);
        }

        private void UpdateUserMarker(LocationInformation currentLocation)
        {
            var currentLocationInMapsLatLng = LocalizationUtils.ToMapsLatLng(currentLocation.LatLng);
            var scenePosition = mapController.GetScenePosition(currentLocationInMapsLatLng);
            mapContentController.UpdateUserMarker(scenePosition);
        }
        
        private void CardDidHideResponse()
        {
            mapContentController.DeselectMarker();
        }

        private void AllMarkersHaveAdded()
        {
            if (mapContentController.SelectedMarker == null)
            {
                uiController.HideToast();
            }
        }

        private void ConnectInteractions()
        {
            if (_interactionsConnected == false)
            {
                mapController.MapDidPan += MapDidPan;
                mapController.MapDidCompletePan += MapDidCompletePan;
                mapController.MapWasTouched += MapWasTouched;
                _interactionsConnected = true;
            }
        }

        private void DisconnectInteractions()
        {
            if (_interactionsConnected)
            {
                mapController.MapDidPan -= MapDidPan;
                mapController.MapDidCompletePan -= MapDidCompletePan;
                mapController.MapWasTouched -= MapWasTouched;
                _interactionsConnected = false;
            }
        }

        public void RequestAreas(MapsLatLng areaCenter)
        {
            if (mapContentController.SelectedMarker == null)
            {
                uiController.DisplayToast("Finding Public Locations");
            }
            coverageClientManager.QueryRadius = _startingCoverageRadius;
            coverageClientManager.QueryLatitude = (float) areaCenter.Latitude;
            coverageClientManager.QueryLongitude = (float) areaCenter.Longitude;
            coverageClientManager.TryGetCoverage(StartingCoverageResponse);
            mapContentController.AnimateMarker();
        }

        private void StartingCoverageResponse(AreaTargetsResult areaTargetsResult)
        {
            if (areaTargetsResult.Status == ResponseStatus.Success)
            {
                mapContentController.AddTargetsToMap(areaTargetsResult.AreaTargets, productionOnly, coverageClientManager);

                coverageClientManager.QueryRadius = _followUpCoverageRadius;
                coverageClientManager.TryGetCoverage(FollowUpCoverageResponse);
            }
            else
            {
                ShowNetworkErrorToast(areaTargetsResult.Status);
            }
        }

        private void FollowUpCoverageResponse(AreaTargetsResult areaTargetsResult)
        {
            if (areaTargetsResult.Status == ResponseStatus.Success)
            {
                if (areaTargetsResult.AreaTargets.Count <= 0)
                {
                    AllMarkersHaveAdded();
                    if (mapContentController.SelectedMarker == null)
                    {
                        uiController.DisplayToast("No Public Locations Nearby", _errorToastDuration);
                    }
                }
                else
                {
                    if (mapContentController.SelectedMarker == null)
                    {
                        uiController.HideToast();    
                    }
                    
                }
                mapContentController.AddTargetsToMap(areaTargetsResult.AreaTargets, productionOnly, coverageClientManager);
            }
            else
            {
                if (mapContentController.SelectedMarker == null)
                {
                    ShowNetworkErrorToast(areaTargetsResult.Status);
                }
            }
        }

        private void ShowNetworkErrorToast(ResponseStatus coverageResponse)
        {
            if (coverageResponse == ResponseStatus.ApiKeyMissing ||
                coverageResponse == ResponseStatus.Forbidden)
            {
                uiController.DisplayToast("Api key missing or invalid", _errorToastDuration);
            }
            else if (coverageResponse == ResponseStatus.InternalGatewayError ||
                     coverageResponse == ResponseStatus.TooManyRequests)
            {
                uiController.DisplayToast("Rate limit reached", _errorToastDuration);
            }
            else if (coverageResponse == ResponseStatus.ConnectionError ||
                     coverageResponse == ResponseStatus.ProtocolError)
            {
                uiController.DisplayToast("Couldn't reach server", _errorToastDuration);
            }
            else
            {
                uiController.DisplayToast("We're having trouble finding Public Locations", _errorToastDuration);
            }
        }

        /// Map interaction responses

        //  Called when a user touches on a Public Location Marker on the map.
        public void RecenterWasClicked()
        {
            var location = locationController.GetUserLocation();
            var recenterLocationMapConvertedLatLng = LocalizationUtils.ToMapsLatLng(location.LatLng);
            mapController.CenterMapOnLocation(recenterLocationMapConvertedLatLng);
        }

        private void MarkerWasSelected()
        {
            var hitMarker = mapContentController.SelectedMarker; //get the selected marker
            if (hitMarker.Target.Identifier != null)
            {
                SharedData.Instance.target = hitMarker.Target;
                SharedData.Instance.HintImage = hitMarker.MarkerHintImage();
                uiController.FillMapCard
                (
                    hitMarker,
                    locationController.GetUserLocation().LatLng,
                    BeforeDidSelectLocation, //Create a new info card : NOTE - this connects the DidSelectLocation callback that is used to trigger the localization scene
                    DidSelectLocation
                );
                var recenterLocationMapConvertedLatLng = LocalizationUtils.ToMapsLatLng(hitMarker.Target.Center);
                mapController.CenterMapOnLocation(recenterLocationMapConvertedLatLng);
                locationController.LocationDidUpdate += uiController.UpdateCardDistance;
            }
            hitMarker.MarkerWasHit?.Invoke();
        }

        private void MapWasTouched()
        {
            mapContentController.DeselectMarker();
        }

        private void MarkerWasDeselected()
        {
            locationController.LocationDidUpdate -= uiController.UpdateCardDistance;
            uiController.HideCard();
        }

        // Checks to see if the map has been panned enough to recenter
        private void MapDidPan()
        {
            var mapCenter = mapController.GetMapCenter();
            var playerPosition = locationController.GetUserLocation();
            var delta = LatLng.Distance(LocalizationUtils.ToARDKLatLng(mapCenter), playerPosition.LatLng);
            if (delta > _recenterOffsetThreshold)
            {
                uiController.ActivateRecenterButton();
            }
            else
            {
                uiController.DeactivateRecenterButton();
            }
        }

        private void MapDidCompletePan()
        {
            RequestAreas(mapController.GetMapCenter());
        }
    }
}
