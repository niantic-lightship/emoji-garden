// Copyright 2022-2024 Niantic.
using System;
using Niantic.Lightship.AR.LocationAR;
using Niantic.Lightship.AR.PersistentAnchors;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Niantic.Lightship.AR.VpsCoverage;

namespace Niantic.Lightship.AR.Samples
{
    // The overall manager of the game, controlling VPS Localization, UI, and game mechanics of the scene.
    public class LocalizationProgressManager : MonoBehaviour
    {
        // Enumeration to manage Localization state
        public enum LocalizationState
        {
            None,
            Localizing,
            Localized,
            LostTracking,
            Failed
        }

        //Error handling
        public enum ErrorType
        {
            Fatal,
            Recoverable,
            UserError,
            None
        }
        
        // AR Managers
        [SerializeField]
        private ARSessionManager _arSessionManager;

        [SerializeField]
        private AROcclusionManager _arOcclusionManager;

        [SerializeField]
        private ARLocationManager _arLocationManager;

        [SerializeField]
        private LocalizationFeedbackController _localizationFeedbackController;

        // Public Events
        public Action<ARLocation, string> OnLocalizationSuccessEvent;
        public Action DidCancelLocalizationEvent;
        public Action OnTrackingLost;
        public Action OnTrackingRegained;

        // Variables
        private LocalizationState _localizationState = LocalizationState.None;
        private float _vpsTimeoutLimit = 12.0f;
        private bool _vpsTimerRunning = false;
        private float _vpsTimerTime;
        private LocalizationTarget _localizationTarget; // the selected Public Location from the map view
        private string _payloadStr;
        private string _targetName;
        private ARLocation _arLocation;

        private bool _hasMinimumCoachingBeenMet;
        private bool _wasInitialized = false;
        private bool _isRecovering = false;
        
        /// Unity Lifecycle
        
        private void OnEnable()
        {
            _arLocationManager.locationTrackingStateChanged += OnStateUpdated;
            _arLocationManager.enabled = true;

            _wasInitialized = false;
            _arSessionManager.ARSessionStatus += OnARSessionChange;
            _arSessionManager.EnableARSession();
        }

        private void OnDisable()
        {
            _arLocationManager.locationTrackingStateChanged -= OnStateUpdated;
            _arLocationManager.enabled = false;

            Stop_VPSLocalization();
            _localizationState = LocalizationState.None;
            _arSessionManager.DisableARSession();
        }

        private void OnARSessionChange(bool status)
        {
            if (status && !_wasInitialized)
            {
                Init();
                _wasInitialized = true;
            }
        }

        private void Init()
        {
            // Verify that we have an anchor to work off of
            if (!SetLocalizationSelectedTarget(SharedData.Instance.target, SharedData.Instance.HintImage))
            {
                _localizationFeedbackController.ShowLocalizationTargetError();
                return;
            }

            //Localization state is none
            _localizationState = LocalizationState.None;
            _isRecovering = false;

            //Set our timer for later
            _vpsTimerTime = _vpsTimeoutLimit;

            //Display the first time user experience. Localization is started when the user confirms the modal away.
            _localizationFeedbackController.CouldAcceptLocalization += MinimumLocalizationCoachingMet;
            _localizationFeedbackController.LocalizationCanceled += Cancel;
            _localizationFeedbackController.FreshEnter(SharedData.Instance.HintImage);
            _hasMinimumCoachingBeenMet = false;

            Start_VPSLocalization();
        }

        // Update is called once per frame
        private void Update()
        {
            // If the timer is going, & we're not localized, keep track
            if (_vpsTimerRunning &&
                (_localizationState == LocalizationState.Localizing ||
                    _localizationState == LocalizationState.LostTracking))
            {
                // Decrease the time
                _vpsTimerTime -= Time.deltaTime;

                // see if timer runs out
                if (_vpsTimerTime <= 0)
                {
                    // Reset timer
                    _vpsTimerRunning = false;
                    _vpsTimerTime = _vpsTimeoutLimit;

                    // OnFail
                    _localizationState = LocalizationState.Failed;
                    OnLocalizationFail();
                }
            }
        }
        
        /// Localization Management
        
        // Start VPS Localization
        public void Start_VPSLocalization()
        {
            //Get the current payload
            var payload = new ARPersistentAnchorPayload(_payloadStr);
            var obj = new GameObject("AR Location");
            _arLocation = obj.AddComponent<ARLocation>();
            _arLocation.Payload = payload;
            _arLocationManager.SetARLocations(_arLocation);
            _arLocationManager.StartTracking();

            //Start the Timeout timer
            _vpsTimerRunning = true;
            _localizationState = LocalizationState.Localizing;

            Debug.Log("VPS! Localizing. State is: " + _localizationState.ToString());
        }

        private void OnStateUpdated(ARLocationTrackedEventArgs args)
        {
            if (args.ARLocation == _arLocation)
            {
                if (args.Tracking)
                {
                    // Localized successfully!

                    // We only want to call this event one time when the first localization comes
                    // in. As subsequent localizations come in they'll update the ARLocation's
                    // transform to the currently most accurate pose. These subsequent localizations
                    // come in if continious localization is on or if the device totally loses
                    // tracking (usually due to camera obstruction).
                    if (_localizationState != LocalizationState.Localized)
                    {
                        _localizationState = LocalizationState.Localized;
                        if (IsReadyForLocalization())
                        {
                            OnLocalizationSuccess();
                        }
                    }
                }
                else
                {
                    // Lost Tracking of ARLocation
                    if (_localizationState == LocalizationState.Localized)
                    {
                        OnLocalizationLost();
                    }
                }
            }
            else
            {
                // Found a completely different location?! This won't ever happen because we don't
                // pass any other ARLocations into SetARLocations.
            }
        }

        private void MinimumLocalizationCoachingMet()
        {
            _localizationFeedbackController.CouldAcceptLocalization -= MinimumLocalizationCoachingMet;
            _hasMinimumCoachingBeenMet = true;
            if (IsReadyForLocalization())
            {
                OnLocalizationSuccess();
            }
        }

        // Stop VPS Localization
        public void Stop_VPSLocalization()
        {
            //Stop localizing
            _arLocationManager.StopTracking();
            Destroy(_arLocation.gameObject);

            //Reset VPS variables
            _vpsTimerRunning = false;
            _vpsTimerTime = _vpsTimeoutLimit;

            //Set state
            _localizationState = LocalizationState.None;

            _localizationFeedbackController.LocalizationCanceled -= Cancel;
        }

        public void Cancel()
        {
            Stop_VPSLocalization();
            DidCancelLocalizationEvent?.Invoke();
        }

        private bool IsReadyForLocalization()
        {
            return _hasMinimumCoachingBeenMet && _localizationState == LocalizationState.Localized;
        }

        // OnLocalizationSuccess
        private void OnLocalizationSuccess()
        {
            //Reset VPS variables
            _vpsTimerRunning = false;
            _vpsTimerTime = _vpsTimeoutLimit;

            //Update UI
            _localizationFeedbackController.Localized();

            Debug.LogWarning($"arLocation isnull: {_arLocation == null}");
            if (!_isRecovering)
            {
                OnLocalizationSuccessEvent?.Invoke(_arLocation, _targetName);
            }
            else
            {
                OnTrackingRegained?.Invoke();
            }
        }

        private void OnLocalizationSuccessHide()
        {
        }

        // OnLocalizationFail
        private void OnLocalizationFail()
        {
            _localizationFeedbackController.Timeout();
        }

        private void OnLocalizationLost()
        {
            _vpsTimerRunning = true;
            _vpsTimerTime = _vpsTimeoutLimit;
            _localizationState = LocalizationState.LostTracking;
            _isRecovering = true;

            // Display the visual feedback
            _localizationFeedbackController.AttemptRecovery();

            OnTrackingLost?.Invoke();

            Debug.Log("VPS! Localization Lost. State is: " + _localizationState.ToString());
        }

        // Target Management Logic
        private bool SetLocalizationSelectedTarget(LocalizationTarget obj, Texture2D hintImage)
        {

            if (obj.Equals(null))
            {
                Debug.LogWarning("LocalizationTarget was null, could not proceed with localization");
                return false;
            }

            if (String.IsNullOrWhiteSpace(obj.DefaultAnchor))
            {
                Debug.LogWarning("DefaultAnchor was empty, could not proceed with localization");
                return false;
            }

            _localizationTarget = obj;
            _targetName = obj.Name;
            _payloadStr = obj.DefaultAnchor;

            return true;
        }
    }
}
