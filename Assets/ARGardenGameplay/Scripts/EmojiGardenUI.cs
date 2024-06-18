// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;

namespace Niantic.Lightship.AR.Samples
{
    public class EmojiGardenUI : MonoBehaviour
    {
        [Header("BackToMap")]
        [SerializeField]
        private Button _backToMapButton;

        [Header("Settings")]
        [SerializeField]
        private Button _openSettingsButton;

        [SerializeField]
        private Button _closeSettingsButton;

        [SerializeField]
        private GameObject _settingsRoot;

        [Header("Settings/Depth")]
        [SerializeField]
        private Toggle _depthToggle;

        [SerializeField]
        private AROcclusionManager _depthManager;

        [Header("Settings/CameraMode")]
        [SerializeField]
        private Toggle _cameraModeToggle;

        [SerializeField]
        private Canvas _uiRoot;

        [Header("Garden Placement")]
        [SerializeField]
        private GameObject _initialPlacementRoot;

        [SerializeField]
        private Button _initialPlacementButton;

        [SerializeField]
        private TwoButtonModalData _placementConfirmationModalData;

        [SerializeField]
        private OneButtonModalData _planeHelperConfirmationModalData;

        [SerializeField]
        private GameObject _waitingRoot;

        [SerializeField]
        private Canvas _modalCanvas;

        [Header("Player Counter")]
        [SerializeField]
        private GameObject _playerCounterRoot;

        [SerializeField]
        private TMP_Text _playerCounterText;

        public Action InitialPlacement;
        public Action RetryPlacement;
        public Action ConfirmPlacement;
        public Action PlaneHelperHidden;
        public Action BackToMap;

        private bool _listenForAnyTap = false;
        private float _debounce = 0.0f;

        protected void OnEnable()
        {
            _openSettingsButton.onClick.AddListener(OnSettingsGear);
            _closeSettingsButton.onClick.AddListener(OnCloseSettings);

            _depthToggle.onValueChanged.AddListener(OnDepthToggle);

            _cameraModeToggle.onValueChanged.AddListener(OnCameraModeToggle);

            _initialPlacementButton.onClick.AddListener(OnInitialPlacement);

            _backToMapButton.onClick.AddListener(OnBackToMap);
        }

        protected void OnDisable()
        {
            _openSettingsButton.onClick.RemoveListener(OnSettingsGear);
            _closeSettingsButton.onClick.RemoveListener(OnCloseSettings);

            _depthToggle.onValueChanged.RemoveListener(OnDepthToggle);

            _cameraModeToggle.onValueChanged.RemoveListener(OnCameraModeToggle);

            _initialPlacementButton.onClick.RemoveListener(OnInitialPlacement);

            _backToMapButton.onClick.RemoveListener(OnBackToMap);
        }

        private void OnSettingsGear()
        {
            _settingsRoot.SetActive(!_settingsRoot.activeSelf);
        }

        private void OnCloseSettings()
        {
            _settingsRoot.SetActive(false);
        }

        private void OnDepthToggle(bool value)
        {
            _depthManager.enabled = value;
        }

        private void OnCameraModeToggle(bool value)
        {
            if (!value)
                return;

            _uiRoot.enabled = false;
            _listenForAnyTap = true;
            _debounce = 0.0f;

            _cameraModeToggle.isOn = false;
        }

        public void ShowGardenPlacement(bool placing)
        {
            _initialPlacementRoot.SetActive(placing);
            _waitingRoot.SetActive(!placing);
        }

        public void ShowPlaneHelper()
        {
            DisplayPlaneHelperModal();
        }

        public void ShowPlayerCounter()
        {
            _playerCounterRoot.SetActive(true);
        }

        public void HidePlaneHelper()
        {
            PlaneHelperHidden?.Invoke();
        }

        public void HideWaitingForPlacement()
        {
            _waitingRoot.SetActive(false);
        }

        public void HideGardenPlacement()
        {
            _initialPlacementRoot.SetActive(false);
            _waitingRoot.SetActive(false);
            ShowPlayerCounter();
        }

        public void HidePlayerCounter()
        {
            _playerCounterRoot.SetActive(false);
        }

        public void UpdatePlayerCounter(int newCount)
        {
            if (newCount > 1)
            {
                _playerCounterText.text = $"{newCount} Players";
            }
            else
            {
                _playerCounterText.text = $"{newCount} Player";
            }
        }

        private void OnInitialPlacement()
        {
            _initialPlacementRoot.SetActive(false);
            DisplayPlacementModal();

            InitialPlacement?.Invoke();
        }

        private void OnRetryPlacement()
        {
            _initialPlacementRoot.SetActive(true);
            HidePlacementModal();

            RetryPlacement?.Invoke();
        }

        private void DisplayPlacementModal()
        {
            var modalDescription = new ModalDescription();
            modalDescription.body = _placementConfirmationModalData.BodyText;
            modalDescription.header = _placementConfirmationModalData.HeaderText;
            modalDescription.primaryButtonText = _placementConfirmationModalData.PrimaryText;
            modalDescription.secondaryButtonText = _placementConfirmationModalData.SecondaryText;
            modalDescription.primaryButtonCallback += OnRetryPlacement;
            modalDescription.secondaryButtonCallback += OnConfirmPlacement;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.TwoButtonModal,modalDescription,_modalCanvas);
        }

        private void DisplayPlaneHelperModal()
        {
            var modalDescription = new ModalDescription();
            modalDescription.header = _planeHelperConfirmationModalData.HeaderText;
            modalDescription.body = _planeHelperConfirmationModalData.BodyText;
            modalDescription.primaryButtonText = _planeHelperConfirmationModalData.PrimaryText;
            modalDescription.primaryButtonCallback += HidePlaneHelper;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.OneButtonModal, modalDescription, _modalCanvas);
        }

        private void HidePlacementModal()
        {
            LightshipModalManager.Instance.HideModal();
        }

        private void OnConfirmPlacement()
        {
           HidePlacementModal();
           ConfirmPlacement?.Invoke();
        }

        private void OnBackToMap()
        {
            BackToMap?.Invoke();
        }

        protected void Update()
        {
            if (!_listenForAnyTap)
            {
                return;
            }

            _debounce += Time.deltaTime;
            if (_debounce <= 0.25f)
            {
                return;
            }

            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                _listenForAnyTap = false;
                _uiRoot.enabled = true;
            }
        }
    }
}
