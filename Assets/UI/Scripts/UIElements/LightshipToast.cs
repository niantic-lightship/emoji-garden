// Copyright 2022-2024 Niantic.
using System.Collections;
using TMPro;
using UnityEngine;
using System;

namespace Niantic.Lightship.AR.Samples
{
    [ExecuteInEditMode]
    public class LightshipToast : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private TMP_Text swapText;

        [SerializeField]
        private AnimationCurve curve;

        [SerializeField]
        private float animationTime;

        [SerializeField]
        private float initialToastOffset;

        [SerializeField]
        private float swapTextOffset;

        [SerializeField]
        private float swapTextTime;

        private LightshipUIShapeDescriptor _shapeDescriptor;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private float _initialYPos;
        private float _initialMainTextYPos;
        private float _initialSwapTextYPos;
        private Coroutine _activeHideCoroutine;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _shapeDescriptor = GetComponent<LightshipUIShapeDescriptor>();
            _initialMainTextYPos = text.rectTransform.anchoredPosition.y;
            _shapeDescriptor.Init();
        }

        private void Update()
        {
            if (_canvasGroup == null)
            {
                Init();
            }
            _shapeDescriptor.SetShape();
        }

        private void InitForAnimation()
        {
            swapText.enabled = false;
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, initialToastOffset * -1);

            _initialYPos = _rectTransform.anchoredPosition.y;

            SetToastOpacity(0);
            SetToastPosition(0);
            _shapeDescriptor.SetShape();
        }
        
        public void SetText(string message)
        {
            text.text = message;
        }

        public void DisplayToastWithText(string message)
        {
            InitForAnimation();
            text.text = message;

            StartCoroutine(DisplayToastEnumerator(null));

        }

        public void ChangeToastText(string message)
        {
            if (text.text == message)
            {
                return;
            }

            if (_activeHideCoroutine != null)
            {
                StopCoroutine(_activeHideCoroutine);
            }

            //set text on new text
            swapText.text = message;

            //swapTextHasBasePosition
            _initialSwapTextYPos = _initialMainTextYPos - swapTextOffset;
            SetTextOffset(0, _initialSwapTextYPos, swapText);
            SetTextOpacity(0, swapText);

            //run the animation
            swapText.enabled = true;

            StartCoroutine(SwapTextEnumerator(_initialSwapTextYPos, _initialMainTextYPos, null));
        }

        public void HideAfterWait(float waitInSeconds, Action cb)
        {
            _activeHideCoroutine = StartCoroutine(AwaitDisplayEnumerator(waitInSeconds, cb));
        }

        public void HideToast(Action cb)
        {
            if (this == null)
            {
                cb?.Invoke();
                return;
            }
            StartCoroutine(HideToastEnumerator(cb));
        }

        private IEnumerator SwapTextEnumerator(float startSwapPosition, float startMainPosition, Action cb)
        {
            float elapsedTime = 0;
            while (elapsedTime <= swapTextTime)
            {
                elapsedTime += Time.deltaTime;
                float curveEval = curve.Evaluate(elapsedTime / swapTextTime);
                SetTextOpacity(curveEval, swapText);
                SetTextOpacity(1 - curveEval, text);
                SetTextOffset(curveEval, startSwapPosition, swapText);
                SetTextOffset(curveEval, startMainPosition, text);
                yield return null;
            }

            text.text = swapText.text;
            text.alpha = 1;
            text.rectTransform.anchoredPosition = new Vector2(0, startMainPosition);

            swapText.alpha = 0;
            swapText.rectTransform.anchoredPosition =
                new Vector2(swapText.rectTransform.anchoredPosition.x, startSwapPosition);

            swapText.enabled = false;
        }

        private IEnumerator DisplayToastEnumerator(Action cb)
        {
            float elapsedTime = 0;
            while (elapsedTime <= animationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
                float curveEval = curve.Evaluate(elapsedTime / animationTime);
                SetToastPosition(curveEval);
                SetToastOpacity(curveEval);
            }

            SetToastPosition(1);
            SetToastOpacity(1);
            cb?.Invoke();
        }

        private IEnumerator AwaitDisplayEnumerator(float length, Action cb)
        {
            yield return new WaitForSecondsRealtime(length);
            cb?.Invoke();
        }

        private IEnumerator HideToastEnumerator(Action cb)
        {
            float elapsedTime = 0;
            while (elapsedTime <= animationTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
                float timeEval = elapsedTime / animationTime;
                SetToastOpacity(1 - timeEval);
            }

            SetToastOpacity(0);
            cb?.Invoke();
        }

        private void SetToastPosition(float percentage)
        {
            _rectTransform.anchoredPosition = new Vector2
            (
                _rectTransform.anchoredPosition.x,
                _initialYPos + (initialToastOffset * -1 * percentage)
            );
        }

        private void SetToastOpacity(float percentage)
        {
            _canvasGroup.alpha = percentage;
        }

        private void SetTextOpacity(float percentage, TMP_Text message)
        {
            message.alpha = percentage;
        }

        private void SetTextOffset(float percentage, float startPosition, TMP_Text message)
        {
            var newY = startPosition + (percentage * swapTextOffset);
            message.rectTransform.anchoredPosition = new Vector2(0, newY);
        }
    }
}
