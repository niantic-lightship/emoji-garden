// Copyright 2022-2024 Niantic.
using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;
using Niantic.Lightship.AR.VpsCoverage;
using Random = UnityEngine.Random;

namespace Niantic.Lightship.AR.Samples
{
    public class VpsTargetMapMarker : MonoBehaviour
    {
        [SerializeField]
        public SpriteRenderer image;

        [SerializeField]
        private AnimationCurve selectedGrowCurve;

        [SerializeField]
        private AnimationCurve easeInOutCurve;

        [SerializeField]
        private float appearTime = 0.4f;

        [SerializeField]
        private float animationTime;

        [SerializeField]
        private Transform internalScaleTranform;

        [SerializeField]
        private Sprite missingImage;

        [SerializeField]
        private float missingImageScale = 1.0f;
        
        private Vector3 defaultScale;
        private float currentScale;

        public Action MarkerWasHit;
        public Action<VpsTargetMapMarker> MarkerDidAppear;
        public LocalizationTarget Target { get; private set; }

        public void Awake()
        {
            defaultScale = internalScaleTranform.localScale;
            currentScale = 1;
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void Initialize(LocalizationTarget newTarget, CoverageClientManager manager, float scaleTarget)
        {
            this.Target = newTarget;
            SetTargetScale(0);
            
            // Transform
            async void DidComplete()
            {
                await DelayStart();
                if ((this != null) && (this.gameObject.activeInHierarchy))
                {
                    StartCoroutine(AnimateTargetRoutine(easeInOutCurve, appearTime, scaleTarget, MarkerDidAppear));
                }
            }

            if (string.IsNullOrEmpty(newTarget.ImageURL))
            {
                image.sprite = missingImage;
                image.transform.localScale = Vector3.one * missingImageScale;
                DidComplete();
                return;
            }

            manager.TryGetImageFromUrl(newTarget.ImageURL, (newTex) => {
                // This can happen if the server denies our request for whatever reason.
                // (rate limit etc.)
                if ((newTex == null) && (gameObject != null)){
                    Destroy(gameObject);
                    return;
                }

                if (this.image != null)
                {
                    Sprite newSprite = Sprite.Create((Texture2D) newTex, new Rect(0, 0, newTex.width, newTex.height),
                        new Vector2(0.5f, 0.5f), 100.0f);
                    newSprite.name = $"Sprite_{Target.Name}"; // so that the name isn't blank anymore in the editor
                    this.image.sprite = newSprite;
                }

                DidComplete();
            });
        }

        public void Initialize(Sprite image, Vector3 spriteScale)
        {
            this.image.sprite = image;
            this.image.transform.localScale = spriteScale;
            SetTargetScale(0);
            StartCoroutine(AnimateTargetRoutine(easeInOutCurve, appearTime, 1, MarkerDidAppear));
        }

        public Texture2D MarkerHintImage()
        {
            return image.sprite.texture;
        }

        private async Task DelayStart()
        {
            int minWaitInMiliseconds = 0;
            int maxWaitInMiliseconds = (int) (appearTime * 1000) / 2;
            int randomWaitTime = Random.Range(minWaitInMiliseconds, maxWaitInMiliseconds);
            await Task.Delay(randomWaitTime);
        }

        public void AnimateTarget(float scaleTarget)
        {
            if ((this.gameObject != null) && (this.gameObject.activeSelf))
            {
                StartCoroutine(AnimateTargetRoutine(selectedGrowCurve, animationTime, scaleTarget, null));
            }
        }

        public async void AnimateAfterDelay(float scaleTarget)
        {
            await DelayStart();
            AnimateTarget(scaleTarget);
        }

        public void HideTarget(Action<VpsTargetMapMarker> cb)
        {
            StartCoroutine(AnimateTargetRoutine(selectedGrowCurve, animationTime, 0f, cb));
        }

        private IEnumerator AnimateTargetRoutine
        (
            AnimationCurve curve,
            float animationLength,
            float targetScaleFactor,
            Action<VpsTargetMapMarker> callback
        )
        {
            float elapsedTime = 0f;
            float startingScaleFactor = currentScale;
            while (elapsedTime <= animationLength)
            {
                yield return null;
                elapsedTime += Time.deltaTime;

                float scaleCurveValue = curve.Evaluate(elapsedTime / animationLength);
                float scaleFactor = Mathf.Lerp(startingScaleFactor, targetScaleFactor, scaleCurveValue);
                SetTargetScale(scaleFactor);
            }
            SetTargetScale(targetScaleFactor);
            callback?.Invoke(this);
        }

        private void SetTargetScale(float scalePercentage)
        {
            currentScale = scalePercentage;
            internalScaleTranform.localScale = defaultScale * scalePercentage;
        }
    }
}
