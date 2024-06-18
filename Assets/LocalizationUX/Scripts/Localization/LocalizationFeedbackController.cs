// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;

namespace Niantic.Lightship.AR.Samples {
    public class LocalizationFeedbackController : MonoBehaviour
    {
        [Header("Asset References")]
        [SerializeField]
        private GameObject _localizingBubble;

        [Header("Prefab References")]
        [SerializeField]
        private RectTransform _hintImageRoot;

        [SerializeField]
        private RawImage _hintImage;

        [SerializeField]
        private LightshipButton _closeButton;

        [SerializeField]
        private GameObject _screen;

        [SerializeField]
        private Canvas _targetCanvas;

        [Header("Modal Data")]
        [SerializeField]
        private OneButtonWithImageModalData _FTUEModalData;

        [SerializeField]
        private TwoButtonModalData _cancelModalData;

        [SerializeField]
        private TwoButtonWithImageModalData _errorModalData;

        [Header("In Scene References")]
        [SerializeField]
        private ARRaycastManager _arRaycastManager;

        [SerializeField]
        private Transform _bubbleParent;

        [SerializeField]
        private ToastController _toastController;

        [Header("Parameters")]
        [SerializeField]
        private AnimationCurve _hintImgAnimCurve;

        [SerializeField]
        private float _hintImgAnimLengthSeconds;

        [SerializeField]
        private float _hintImgAnimInitialValue;

        [SerializeField]
        private float _hintImgAnimEndValue;

        public Action CouldAcceptLocalization;
        public Action LocalizationCanceled;

        private float _pointsUpdateRateSeconds = 2.0f;
        private int _numberFeaturePointSpotsToCheck = 10;
        private Coroutine _depthPointCoroutine;

        private IObjectPool<GameObject> _bubbleInstances;

        private enum FeedbackState
        {
            Hidden,
            InitialModalPrompt,
            Localizing,
            ErrorModalPrompt,
            CancellationPrompt
        }
        private FeedbackState _currentState;
        private float _hintImageTransitionControlValue;
        private float _stateStartTimestamp;
        private bool _waitForTraining;
        private bool _recovering;

        protected void Awake()
        {
            _bubbleInstances = new ObjectPool<GameObject>(
                () => Instantiate(_localizingBubble, Vector3.zero, Quaternion.identity, _bubbleParent),
                (bubble) => bubble.SetActive(true),
                (bubble) => bubble.SetActive(false),
                (bubble) => Destroy(bubble),
                true,                                  // Throw errors if misusing pool
                _numberFeaturePointSpotsToCheck,       // # of preallocated objects
                _numberFeaturePointSpotsToCheck * 2    // Max allocated objects
            );

            _toastController.TargetTransform = transform;
        }

        // Sets default state to hidden
        protected void OnEnable()
        {
            _hintImageRoot.anchorMax = new Vector2(1.0f, _hintImgAnimInitialValue);
            _hintImageRoot.anchorMin = new Vector2(0.0f, _hintImgAnimInitialValue);
            _hintImageRoot.gameObject.SetActive(true);

            SetState(FeedbackState.Hidden);
            RegisterToButtons();
        }

        // When object is disabled, make sure to jump into hidden state
        protected void OnDisable()
        {
            UnregisterFromButtons();
            SetState(FeedbackState.Hidden);
        }

        protected void Update()
        {
            switch (_currentState)
            {
                case FeedbackState.Localizing:
                {
                    HintImageAnimate(true);

                    if (!_waitForTraining || (Time.time - _stateStartTimestamp) > 1.0f)
                    {
                        CouldAcceptLocalization?.Invoke();
                    }

                    break;
                }
            }

            if (_currentState != FeedbackState.Localizing)
            {
                HintImageAnimate(false);
            }
        }

        // UI Events

        private void RegisterToButtons()
        {
            _closeButton.onClick.AddListener(ShowCancellationDialog);
        }

        private void UnregisterFromButtons()
        {
            _closeButton.onClick.RemoveListener(ShowCancellationDialog);
        }

        private void StartLocalization()
        {
            if (_currentState != FeedbackState.Hidden)
            {
                SetState(FeedbackState.Localizing);
            }
        }

        private void CancelLocalization()
        {
            if (_currentState != FeedbackState.Hidden)
            {
                SetState(FeedbackState.Hidden);
                LocalizationCanceled?.Invoke();
            }
        }

        private void ShowCancellationDialog()
        {
            if (_currentState != FeedbackState.Hidden)
            {
                SetState(FeedbackState.CancellationPrompt);
            }
        }

        private void DisplayCoachingModal()
        {
            var modalDescription = new ModalDescription();
            modalDescription.header = _FTUEModalData.HeaderText;
            modalDescription.body = _FTUEModalData.BodyText;
            modalDescription.image = _FTUEModalData.ModalImage;
            modalDescription.primaryButtonText = _FTUEModalData.PrimaryText;
            modalDescription.primaryButtonCallback += StartLocalization;
            modalDescription.animatorController = _FTUEModalData.AnimatorController;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.OneButtonWithImageModal,modalDescription, _targetCanvas);
        }

        private void DisplayErrorModal()
        {
            var modalDescription = new ModalDescription();
            modalDescription.header = _errorModalData.HeaderText;
            modalDescription.body = _errorModalData.BodyText;
            modalDescription.image = _errorModalData.ModalImage;
            modalDescription.primaryButtonText = _errorModalData.PrimaryText;
            modalDescription.secondaryButtonText = _errorModalData.SecondaryText;
            modalDescription.primaryButtonCallback += CancelLocalization;
            modalDescription.secondaryButtonCallback += StartLocalization;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.TwoButtonWithImageModal,modalDescription, _targetCanvas);
        }

        private void DisplayCancelModal()
        {
            var modalDescription = new ModalDescription();
            modalDescription.header = _cancelModalData.HeaderText;
            modalDescription.body = _cancelModalData.BodyText;
            modalDescription.primaryButtonText = _cancelModalData.PrimaryText;
            modalDescription.secondaryButtonText = _cancelModalData.SecondaryText;
            modalDescription.primaryButtonCallback += CancelLocalization;
            modalDescription.secondaryButtonCallback += StartLocalization;
            LightshipModalManager.Instance.DisplayModal(LightshipModalManager.ModalType.TwoButtonModal,modalDescription, _targetCanvas);
        }

        // These functions are called by "business logic" in LocalizationProgressManager

        public void FreshEnter(Texture2D hintImage)
        {
            _hintImage.texture = hintImage;
            SetState(FeedbackState.InitialModalPrompt);
        }

        public void Timeout()
        {
            SetState(FeedbackState.ErrorModalPrompt);
        }

        public void Localized()
        {
            if (_recovering)
            {
                _toastController.DisplayToast("Tracking Regained", 2.0f);
            }
            SetState(FeedbackState.Hidden);
        }

        public void AttemptRecovery()
        {
            _toastController.DisplayToast("Tracking Lost");
            SetState(FeedbackState.Localizing);
        }

        public void ShowLocalizationTargetError()
        {
            SetState(FeedbackState.ErrorModalPrompt);
        }

        private void SetState(FeedbackState nextState)
        {
            _stateStartTimestamp = Time.time;

            // Exit State
            switch (_currentState)
            {
                case FeedbackState.Hidden:
                {
                    break;
                }
                case FeedbackState.InitialModalPrompt:
                {
                    _waitForTraining = true;
                    _toastController.DisplayToast("Walk slowly around the Public Location to capture multiple angles", 5.0f);
                    LightshipModalManager.Instance.HideModal();
                    _screen.gameObject.SetActive(false);
                    break;
                }
                case FeedbackState.Localizing:
                {
                    _waitForTraining = false;
                    _toastController.HideToast();
                    StopCoroutine(_depthPointCoroutine);
                    break;
                }
                case FeedbackState.ErrorModalPrompt:
                {
                    LightshipModalManager.Instance.HideModal();
                    _screen.gameObject.SetActive(false);
                    break;
                }
                case FeedbackState.CancellationPrompt:
                {
                    LightshipModalManager.Instance.HideModal();
                    _screen.gameObject.SetActive(false);
                    break;
                }
            }

            _currentState = nextState;
            // Enter State
            switch (nextState)
            {
                case FeedbackState.Hidden:
                {
                    _toastController.HideToast();
                    break;
                }
                case FeedbackState.InitialModalPrompt:
                {
                    _screen.gameObject.SetActive(true);
                    DisplayCoachingModal();
                    break;
                }
                case FeedbackState.Localizing:
                {
                    _depthPointCoroutine = StartCoroutine(FeaturePointsViaDepth());
                    break;
                }
                case FeedbackState.ErrorModalPrompt:
                {
                    _screen.gameObject.SetActive(true);
                    DisplayErrorModal();
                    break;
                }
                case FeedbackState.CancellationPrompt:
                {
                    _screen.gameObject.SetActive(true);
                    DisplayCancelModal();
                    break;
                }
            }
        }


        private void HintImageAnimate(bool animateIn)
        {
            if (animateIn)
            {
                _hintImageTransitionControlValue += Time.deltaTime / _hintImgAnimLengthSeconds;
            }
            else
            {
                _hintImageTransitionControlValue -= Time.deltaTime / _hintImgAnimLengthSeconds;
            }

            _hintImageTransitionControlValue = Mathf.Clamp01(_hintImageTransitionControlValue);
            float transitionInT = _hintImgAnimCurve.Evaluate(_hintImageTransitionControlValue);
            float anchorValue = Mathf.Lerp(_hintImgAnimInitialValue, _hintImgAnimEndValue, transitionInT);
            _hintImageRoot.anchorMax = new Vector2(1.0f, anchorValue);
            _hintImageRoot.anchorMin = new Vector2(0.0f, anchorValue);
        }

        private ARRaycastHit ScreenRayToWorld(Vector2 pos_V2)
        {
            //Our return variable
            ARRaycastHit myHit = new ARRaycastHit();

            //Create list for our Hits, and get camera position for screen
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            TrackableType trackableTypes = TrackableType.AllTypes;

            //Perform the Raycast from the camera to straight forward
            if (_arRaycastManager.Raycast(pos_V2, hits, trackableTypes))
            {
                //Get our hit
                myHit = hits[0];
            }

            //Return
            return myHit;
        }

        private IEnumerator FeaturePointsViaDepth()
        {
            var elapsedTime = 0f;
            while (true)
            {
                //Number of Milliseconds cadence (1000 milliseconds in a second..), let's do this after 100 milliseconds (1/10th of a second)
                elapsedTime += Time.deltaTime; //Increment timing
                if (elapsedTime > _pointsUpdateRateSeconds) //This is working
                {
                    //Reset our timer
                    elapsedTime = 0f;

                    //Pick 10 pixels or places on the top 2/3 of the screen screen
                    for (int i = 0; i < _numberFeaturePointSpotsToCheck; i++)
                    {
                        //Make a square area of where they can spawn, in the center of the screen
                        float x_MaxRight = (Screen.width / 4) * 3.0f; //Right 3/4 limit
                        float x_MinLeft = Screen.width / 4; //Left 1/4 limit
                        float y_MaxUp = (Screen.height / 4) * 3; //Upper 3/4 limit
                        float y_MinBottom = Screen.height / 4;
                        float randomScreenX = Random.Range(x_MinLeft, x_MaxRight);
                        float randomScreenY = Random.Range(y_MinBottom, y_MaxUp);

                        //For each spot...Do a Raycast of the Depth
                        ARRaycastHit hitTest = ScreenRayToWorld(new Vector2(randomScreenX, randomScreenY));

                        //If we got a hit....
                        if (hitTest != null && hitTest.hitType != TrackableType.None)
                        {
                            //if Depth is between 1m and 10m --> Then it's in VPS range and we'll do something
                            if (hitTest.distance >= 1 && hitTest.distance <= 10)
                            {
                                //Then instantiate a prefab that is a ~0.2m sphere
                                var bubble = _bubbleInstances.Get();
                                bubble.transform.position = hitTest.pose.position;
                                bubble.transform.rotation = Quaternion.identity;

                                yield return new WaitForSeconds(0.1f);
                            }
                        }
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
