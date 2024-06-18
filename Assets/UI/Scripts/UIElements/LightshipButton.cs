// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples {
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(LightshipUIShapeDescriptor))]
    [ExecuteInEditMode]
    public class LightshipButton : Button
    {
        [SerializeField]
        public Color _activeColor = Color.white;

        [SerializeField]
        public Color _disabledColor;

        [SerializeField]
        public Color _activeTextColor;

        [SerializeField]
        public Color _disabledTextColor;

        private const string animName = "ButtonClickAnimation";
        private TMP_Text _textDisplay;
        private float _currentDisplayValue = 1;
        private CanvasGroup _canvasGroup;
        private Animator _buttonAnimator;
        private RectTransform _rectTransform;
        private LightshipUIShapeDescriptor _shapeDescriptor;
        private Coroutine _activeCoroutine = null;
        private Image _backgroundImage;
        private PointerEventData _cachedClickData;

        public float AppearanceLength;
        public float InteractableTransitionLength;
        public AnimationCurve AppearanceCurve;
        public UnityEvent ButtonDidClickBeforeAnimationEvent = new UnityEvent();

        protected override void Awake()
        {
            base.Awake();
            _backgroundImage = GetComponent<Image>();
            Init();
        }

        private void Update()
        {
            if (_rectTransform == null)
            {
                Init();
            }
            _shapeDescriptor.SetShape();
        }
        
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (this.interactable != true)
            {
                return;
            }

            this.interactable = false;
            _cachedClickData = eventData;
            ButtonDidClickBeforeAnimationEvent?.Invoke();
            _buttonAnimator.Play(animName);
        }

        public void SetText(string text)
        {
            _textDisplay.text = text;
        }

        public void SetInteractable(bool state, bool animate)
        {
            var interactState = this.interactable;
            if (interactState == state) return;

            this.interactable = !this.interactable;
            Action<Color> transitionBackground = (Color setColor) => { _backgroundImage.color = setColor; };
            Action<Color> transitionText = (Color setColor) => { _textDisplay.color = setColor; };
            // We want to transition from one to the other. 
            if (state) // We are enabling
            {
                if (animate)
                {
                    StartCoroutine(TransitionColor(_disabledColor, _activeColor, transitionBackground));
                    StartCoroutine(TransitionColor(_disabledTextColor, _activeTextColor, transitionText));
                }
                else
                {
                    _backgroundImage.color = _activeColor;
                    _textDisplay.color = _activeTextColor;
                }
            }
            else
            {
                if (animate)
                {
                    StartCoroutine(TransitionColor(_activeColor, _disabledColor, transitionBackground));
                    StartCoroutine(TransitionColor(_activeTextColor, _disabledTextColor, transitionText));
                }
                else
                {
                    _backgroundImage.color = _disabledColor;
                    _textDisplay.color = _disabledTextColor;
                }
            }
        }

        public void ButtonBecomesActive()
        {
            if (gameObject.activeSelf)
            {
                return;
            }

            gameObject.SetActive(true);
            SetAlphaValue(0);
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            _activeCoroutine = StartCoroutine(FadeTransition(1, null));
        }

        public void ButtonBecomesInactive()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
            }

            Action callback = () =>
            {
                _activeCoroutine = null;
                gameObject.SetActive(false);
            };
            _activeCoroutine = StartCoroutine(FadeTransition(0, callback));
        }

        private void Init()
        {
            _textDisplay = GetComponentInChildren<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
            _buttonAnimator = GetComponent<Animator>();
            _shapeDescriptor = GetComponent<LightshipUIShapeDescriptor>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _buttonAnimator = GetComponent<Animator>();
            _shapeDescriptor.Init();
            this.transition = Transition.None;
        }
        
        public void OnClickAnimationFinish()
        {
            _cachedClickData.button = PointerEventData.InputButton.Left;
            this.interactable = true; 
            base.OnPointerClick( _cachedClickData);
            this._cachedClickData = null;
        }

        private void SetAlphaValue(float curvePercentage)
        {
            float alphaVal = AppearanceCurve.Evaluate(curvePercentage);
            _currentDisplayValue = alphaVal;
            _canvasGroup.alpha = alphaVal;
        }

        private IEnumerator FadeTransition(float targetValue, Action cb)
        {
            float elapsedPercentage = 1 - Math.Abs(targetValue - _currentDisplayValue);
            float elapsedTime = (AppearanceLength * elapsedPercentage);
            while (elapsedTime <= AppearanceLength)
            {
                elapsedTime += Time.deltaTime;
                elapsedPercentage = elapsedTime / AppearanceLength;
                if (targetValue == 0)
                {
                    elapsedPercentage = 1 - elapsedPercentage;
                }

                SetAlphaValue(elapsedPercentage);
                yield return null;
            }

            SetAlphaValue(targetValue);
            cb?.Invoke();
        }
        
        private IEnumerator TransitionColor(Color start, Color end, Action<Color> cb)
        {
            float elapsedTime = 0;
            float elapsedPercentage = 0;
            while (elapsedTime <= InteractableTransitionLength)
            {
                elapsedTime += Time.deltaTime;
                elapsedPercentage = AppearanceCurve.Evaluate(elapsedTime / InteractableTransitionLength);
                Color transitionColor = Color.Lerp(start, end, elapsedPercentage);
                cb?.Invoke(transitionColor);
                yield return null;
            }
            cb?.Invoke(end);
        }
    }
}
