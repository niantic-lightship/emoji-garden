// Copyright 2022-2024 Niantic.

using UnityEngine;
using Niantic.Lightship.AR.LocationAR;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

namespace Niantic.Lightship.AR.Samples
{
    public class LocationBasedExperienceController : MonoBehaviour
    {
        // Reference to a MonoBehaviour that implements the ILocationBasedExperience interface.
        // This is enforced through the editor script.
        [SerializeField] private MonoBehaviour _appSerializedField;

        // Accessor to get a reference to the interface from the component in the serialized field.
        private ILocationBasedExperience LocationApp
        {
            get => _appSerializedField as ILocationBasedExperience;
        }

        // Accessor to get the gameObject of the component in the serialized field.
        private GameObject LocationAppGameObject
        {
            get => _appSerializedField?.gameObject ?? null;
        }

        private WayfindingController _mapController;
        private LocalizationProgressManager _localizationController;
        private ARSession _arSession;
        private XROrigin _xrOrigin;

        protected void OnEnable()
        {
            if (LocationApp == null)
            {
                Debug.LogError("No ILocationBasedExperience provided to the LocationBasedAppController. You will not see your content as expected.");
            }

            _xrOrigin = FindObjectOfType<XROrigin>(true);
            if (_xrOrigin == null)
            {
                Debug.LogError("No XROrigin could be found in the scene!");
            }

            _mapController = FindObjectOfType<WayfindingController>(true);
            if (_mapController == null)
            {
                Debug.LogError("No WayfindingController could be found in the scene!");
            }

            _localizationController = FindObjectOfType<LocalizationProgressManager>(true);
            if (_localizationController == null)
            {
                Debug.LogError("No LocalizationProgressManager could be found in the scene!");
            }

            _arSession = FindObjectOfType<ARSession>(true);
            if (_arSession == null)
            {
                Debug.LogError("No ARSession could be found in the scene!");
            }

            DisableXRMachine();

#if UNITY_EDITOR
            // The AR session is disabled by default in the scene. This is because enabling the
            // session triggers camera permission request. So for our release builds we need to keep
            // this disabled until the user selects a location. We can then turn on the ARSession,
            // prompting for camera permissions.
            //
            // However, in Editor the ARSession is what installs the playback data.
            // So unless this is enabled here, we don't get GPS data from the playback until we
            // start the camera feed and that's way too late.
            _arSession.enabled = true;
#endif

            // TODO: Is this necessary??
            _mapController.DidSelectLocation += BootXRMachine;

            // TODO: Setup the helpers that respond to these events
            _localizationController.OnLocalizationSuccessEvent += StartApp;
            _localizationController.OnTrackingLost += HideApp;
            _localizationController.OnTrackingRegained += ShowApp;
            _localizationController.DidCancelLocalizationEvent += ReturnToMap;
        }

        protected void OnDisable()
        {
            if (_mapController != null)
            {
                _mapController.DidSelectLocation -= BootXRMachine;
            }

            if (_localizationController != null)
            {
                _localizationController.OnLocalizationSuccessEvent -= StartApp;
                _localizationController.OnTrackingLost -= HideApp;
                _localizationController.OnTrackingRegained -= ShowApp;
                _localizationController.DidCancelLocalizationEvent -= ReturnToMap;
            }
        }

        public void ReturnToMap()
        {
            DisableXRMachine();
        }

        private void StartApp(ARLocation location, string location_name)
        {
            var args = new ILocationBasedExperience.LocationArgs();
            args.Location = location;
            args.ReadableLocationName = location_name;

            Debug.Log($"Location names: {location.gameObject.name} {location_name}");

            if (LocationApp == null)
            {
                Debug.LogError("No location experience set! Please check the LocationBasedAppController inspector");
                return;
            }

            LocationAppGameObject.SetActive(true);
            LocationApp.StartExperience(args, this);
        }

        private void HideApp()
        {
            LocationApp?.PauseDueToLocalizationLost();
        }

        private void ShowApp()
        {
            LocationApp?.UnpauseDueToLocalizationRegained();
        }

        private void BootXRMachine()
        {
            _mapController.HideMap();
            _xrOrigin.gameObject.SetActive(true);
            _localizationController.gameObject.SetActive(true);
        }

        private void DisableXRMachine()
        {
            if (LocationAppGameObject != null)
            {
                LocationAppGameObject.SetActive(false);
            }

            _xrOrigin.gameObject.SetActive(false);
            _mapController.ShowMap();
        }
    }
}
