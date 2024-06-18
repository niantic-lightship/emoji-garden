// Copyright 2022-2024 Niantic.

using System;
using System.Collections;
using Niantic.Lightship.AR.VpsCoverage;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples {
    public enum LocationState
    {
        Available,
        NotAvailable
    }
    public struct LocationInformation
    {
        public LatLng LatLng;
        public LocationState LocationState;

        public LocationInformation(LatLng latLng, LocationState locationState)
        {
            this.LatLng = latLng;
            this.LocationState = locationState;
        }
    }
    
    public class LocationController : MonoBehaviour
    {
        [SerializeField]
        private PermissionsManager permissionsManager;
        private LatLng _lastLocation;
        private GPSSmoother _gpsSmoother;

        public Action<LocationInformation> LocationDidUpdate;
        public Action LocationServiceAvailable;
        
        private void OnEnable()
        {
            permissionsManager.locationGrantStatus += LocationPermissionReturn;
        }

        private void OnDisable()
        {
            permissionsManager.locationGrantStatus -= LocationPermissionReturn;
        }

        public void Awake()
        {
            permissionsManager.enabled = false;
            _gpsSmoother = new GPSSmoother();
        }

        public void Update()
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                UpdateLocation();
            }
        }

        public void StartLocationService()
        {
            permissionsManager.enabled = true;
            permissionsManager.AskForPermission(PermissionsManager.PermissionType.location);
        }

        public void LocationPermissionReturn(bool status)
        {
            permissionsManager.enabled = false;
            if (status)
            {
                Action serviceRunning = () =>
                {
                    Input.compass.enabled = true;
                    LocationServiceAvailable?.Invoke();
                };
                StartCoroutine(StartLocationServiceEnumerator(serviceRunning));
            }
        }

        /*
         * Returns the most recent GPS location from our GPS location smoother
         */
        public LocationInformation GetUserLocation()
        {
            var isLocationServicesAlreadyRunning = Input.location.status == LocationServiceStatus.Running;
            if (isLocationServicesAlreadyRunning)
            {
                return new LocationInformation(new LatLng(_lastLocation.Latitude, _lastLocation.Longitude), LocationState.Available);
            }
            else
            {
                return new LocationInformation(new LatLng(0, 0), LocationState.NotAvailable);
            }
        }

        // Updates our location data model with the latest GPS location, run through a GPS location smoother
        private void UpdateLocation()
        {
            var latestLocation = Input.location.lastData;
            _lastLocation = _gpsSmoother.AddSample(latestLocation.latitude, latestLocation.longitude);
            var latestLocationinformation = new LocationInformation(_lastLocation, LocationState.Available);
            LocationDidUpdate?.Invoke(latestLocationinformation);
        }

        private void StopLocationServices()
        {
            Input.location.Stop();
        }

        private IEnumerator StartLocationServiceEnumerator(Action cb)
        {
            bool wasLocationServicesAlreadyRunning = Input.location.status == LocationServiceStatus.Running;
            if (!wasLocationServicesAlreadyRunning)
            {
                Input.location.Start(1);
            }
            yield return new WaitForSeconds(0.1f);
            while (Input.location.status != LocationServiceStatus.Running)
            {
                yield return new WaitForEndOfFrame();
            }
            cb?.Invoke();
        }

        private IEnumerator StartLocationServiceEnumerator(Func<LatLng> cb)
        {
            bool wasLocationServicesAlreadyRunning = Input.location.status == LocationServiceStatus.Running;
            if (!wasLocationServicesAlreadyRunning)
            {
                Input.location.Start(1);
            }

            while (Input.location.status != LocationServiceStatus.Running)
            {
                yield return new WaitForEndOfFrame();
            }

            cb?.Invoke();
        }
    }
}
