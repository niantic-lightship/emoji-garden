// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using Permission = UnityEngine.Android.Permission;

namespace Niantic.Lightship.AR.Samples
{
    public class PermissionsManager : MonoBehaviour
    {
        // Modal setup
        [SerializeField]
        private PermissionsModalData locationModalData;

        [SerializeField]
        private PermissionsModalData cameraModalData;

        [SerializeField]
        private OneButtonModalData locationDeniedModalData;

        [SerializeField]
        private OneButtonModalData cameraDeniedModalData;

        [SerializeField]
        private Canvas canvas;

        private const string LOCATION_PREF_KEY = "LocationPermission";
        private const string CAMERA_PREF_KEY = "CameraPermission";
        private const int PERM_GRANTED = 1;
        private const int PERM_DENIED = -1;
        private const int PERM_FIRST_TIME = 0;

        private ModalView _activeModal;
        private bool shouldRestartFlow = false;
        private PermissionType activeType;

        public Action<bool> locationGrantStatus;
        public Action<bool> cameraGrantStatus;

        public enum PermissionType
        {
            location,
            camera
        }

        // Handles when a user leaves the app to enable location in settings.
        public void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"OnApplicationPause: pausing/unpausing: {pauseStatus}, should restart permission check: {shouldRestartFlow}, active permission: {activeType}");
            if (pauseStatus == false)
            {
                if (shouldRestartFlow)
                {
                    AskForPermission(activeType);
                }
            }
        }

        // Uses a combination of native APIs to check on the status of current permissions,
        // and a playerpref cache to track if the app has previously asked for permission.
        //
        // This allows us to determine if we can surface the native settings prompt, or need to direct
        // players to the settings page in order to enable a feature
        public void AskForPermission(PermissionType type)
        {
            shouldRestartFlow = false;
            var enabled = SyncPermissionCache(type);
            var grant = GetPermissionState(type);
            // Player has already granted location permission
            if (grant == 1)
            {
                switch (type)
                {
                    case PermissionType.camera :
                        cameraGrantStatus?.Invoke(true);
                        break;
                    case PermissionType.location :
                        locationGrantStatus?.Invoke(true);
                        break;
                }
                return;
            }

            // Player has *DENIED* permission for this application
            if (grant == PERM_DENIED)
            {
                shouldRestartFlow = true;
                DisplayPreviouslyDeniedModal(type, grant);
                return;
            }

            // The user has neither previously denied permission, nor previously granted permission.
            DisplayInitialPromptModal(type);
        }

        // Makes sure the app is tracking changes to app permision that may hav been made directly from settings
        private bool SyncPermissionCache(PermissionType permissionType)
        {
            var key = GetPermissionKey(permissionType);
            var permissionGrant = PlayerPrefs.GetInt(key);
            var enabled = PermissionCommunicator.IsPermissionGranted(permissionType);

            if ((permissionGrant == PERM_GRANTED) && (enabled == false))
            {
                SavePermissionState(permissionType, PERM_DENIED);
            } else if ((permissionGrant == PERM_DENIED) && (enabled == true))
            {
                SavePermissionState(permissionType, PERM_GRANTED);
            }

            return enabled;
        }

        // Saves a permission state to player prefs
        private void SavePermissionState(PermissionType type, int val)
        {
            PlayerPrefs.SetInt(GetPermissionKey(type), val);
            PlayerPrefs.Save();
        }

        private string GetPermissionKey(PermissionType type)
        {
            switch (type)
            {
                case PermissionType.camera :
                    return  CAMERA_PREF_KEY;
                case PermissionType.location :
                    return LOCATION_PREF_KEY;
                default :
                    return "";
            }
        }

        private int GetPermissionState(PermissionType type)
        {
            return PlayerPrefs.GetInt(GetPermissionKey(type));
        }

        private void DisplayPreviouslyDeniedModal(PermissionType type, int grantStatus)
        {
            var modalDescription = new ModalDescription();
            switch (type)
            {
                case PermissionType.camera :
                    
                    modalDescription.header = "";
                    modalDescription.body = cameraDeniedModalData.BodyText;
                    modalDescription.primaryButtonText = cameraDeniedModalData.PrimaryText;
                    
                    break;
                case PermissionType.location :
                    modalDescription.header = "";
                    modalDescription.body = locationDeniedModalData.BodyText;
                    modalDescription.primaryButtonText = locationDeniedModalData.PrimaryText;
                    break;

            }
            Action intent = null;
            if (grantStatus == PERM_FIRST_TIME)
            {
                intent = () =>
                {
                    switch (type)
                    {
                        case PermissionType.camera:
                            StartNativeCameraPrompt();
                            break;
                        case PermissionType.location:
                            StartNativeLocationPrompt();
                            break;
                    }

                };
            }
            else
            {
                intent = () =>
                {
                    activeType = type;
                    PermissionCommunicator.OpenSettings(type);
                };
            }

            modalDescription.primaryButtonCallback += intent;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.OneButtonModal,modalDescription, canvas);
        }

        // Displays the pre-ask modal that tells players we're about to open the native location prompt
        private void DisplayInitialPromptModal(PermissionType type)
        {
            Action previousDeniedModal = null;
            var modalDescription = new ModalDescription();
            PermissionsModalData modalData = null;
            switch (type)
            {
                case PermissionType.camera:
                    previousDeniedModal = () =>
                    {
                        DisplayPreviouslyDeniedModal(PermissionType.camera, PERM_FIRST_TIME);
                    };
                    modalDescription.primaryButtonCallback = StartNativeCameraPrompt;
                    modalData = cameraModalData;
                    break;
                case PermissionType.location:
                    previousDeniedModal = () =>
                    {
                        DisplayPreviouslyDeniedModal(PermissionType.location, PERM_FIRST_TIME);
                    };
                    modalDescription.primaryButtonCallback = StartNativeLocationPrompt;
                    modalData = locationModalData;
                    break;
            }
            modalDescription.header = modalData.HeaderText;
            modalDescription.body = modalData.BodyText;
            modalDescription.image = modalData.ModalImage;
            modalDescription.primaryButtonText = modalData.PrimaryText;
            modalDescription.secondaryButtonText = modalData.DenyText;
            modalDescription.secondaryButtonCallback = previousDeniedModal;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.PermissionsModal,modalDescription,canvas);
        }
        
        // Displays the native location settings prompt
        private void StartNativeLocationPrompt()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                RequestPermissionAndroid(PermissionType.location);
            #elif UNITY_IOS && !UNITY_EDITOR
                StartCoroutine(StartLocationServiceEnumerator());
            #elif UNITY_EDITOR
            SavePermissionState(PermissionType.location, PERM_GRANTED);

            locationGrantStatus?.Invoke(true);
            #endif

        }

        private void StartNativeCameraPrompt()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                RequestPermissionAndroid(PermissionType.camera);
            #elif UNITY_IOS && !UNITY_EDITOR
                StartCoroutine(StartCameraEnumerator());
            #elif UNITY_EDITOR
            SavePermissionState(PermissionType.camera, PERM_GRANTED);
            cameraGrantStatus?.Invoke(true);
            #endif
        }

        // Callback that gets fired when a permission is granted
        private void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            switch (permissionName)
            {
                case Permission.FineLocation :
                    SavePermissionState(PermissionType.location, PERM_GRANTED);
                    locationGrantStatus?.Invoke(true);
                    break;
                case Permission.Camera :

                    SavePermissionState(PermissionType.camera, PERM_GRANTED);
                    cameraGrantStatus?.Invoke(true);
                    break;
            }
        }

        // Callback that gets fired when a permission is denied
        private void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            switch (permissionName)
            {
                case Permission.FineLocation :
                    SavePermissionState(PermissionType.location, PERM_DENIED);
                    AskForPermission(PermissionType.location);
                    break;
                case Permission.Camera :
                    SavePermissionState(PermissionType.camera, PERM_DENIED);
                    AskForPermission(PermissionType.camera);
                    break;
            }
        }

        // Triggers the prompt for location permission on IOS
        private IEnumerator StartLocationServiceEnumerator()
        {
            bool wasLocationServicesAlreadyRunning = Input.location.status == LocationServiceStatus.Running;
            if (!wasLocationServicesAlreadyRunning)
            {
                Input.location.Start(1);
            }

            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return new WaitForEndOfFrame();
            }
            if (Input.location.status == LocationServiceStatus.Running)
            {
                PermissionCallbacks_PermissionGranted(Permission.FineLocation);
            } else if (Input.location.status == LocationServiceStatus.Failed)
            {
                PermissionCallbacks_PermissionDenied(Permission.FineLocation);
            }
        }

        // Listener for changes to camera permissions (IOS only)
        private IEnumerator StartCameraEnumerator()
        {
            PermissionCommunicator.RequestCameraPermission();
            while (PermissionCommunicator.CameraInFlight() == (int)PermissionCommunicator.PermissionStatus.NotDetermined)
            {

                yield return new WaitForEndOfFrame();
            }

            if (PermissionCommunicator.CameraInFlight() == (int)PermissionCommunicator.PermissionStatus.AuthorizedAlways)
            {
                PermissionCallbacks_PermissionGranted(Permission.Camera);
            }
            else
            {
                PermissionCallbacks_PermissionDenied(Permission.Camera);
            }
        }

        // Triggers the prompt for location permission on Android
        private void RequestPermissionAndroid(PermissionType type)
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionDeniedAndDontAskAgain +=PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;

            switch (type)
            {
                case PermissionType.camera :
                    Permission.RequestUserPermission(Permission.Camera, callbacks);
                    break;
                case PermissionType.location :
                    Permission.RequestUserPermission(Permission.FineLocation, callbacks);
                    break;
            }
        }
    }
}
