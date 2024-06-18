// Copyright 2022-2024 Niantic.
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class FeaturePoint : MonoBehaviour
    {
        //Features of a Feature Point
        private float lifespanMax = 5.0f;
        private float lifespanTimer = 0.0f; //Starts at the maximum

        //Pulse variables
        private enum PulseState
        {
            increaseOne,
            increaseTwo,
            shrinkOne,
            shrinkTwo,
            empty
        };

        private bool shouldPulse = true;
        private float currentRatio = 0.025f;
        private float increase1_Max = 1.6f;
        private float shrink1_Min = 1.0f;
        private float increase2_Max = 1.3f;
        private float shrink2_Min = 0.01f;
        private float pulseSpeed_Initial = 0.0005f;
        private float pulseSpeed_WithRise = 0.001f;
        private float originalScale = 0.025f;
        private PulseState _pulseState = PulseState.increaseOne;

        //Rise variables
        private Vector3 positionStart;
        private Vector3 positionDest;
        private float riseSpeed = 1.25f;
        private bool shouldRise = false;

        private void Start()
        {
            //Set scaling props
            SetScalingProperties();

            //Set position variables
            positionStart = this.transform.position;
            positionDest = this.transform.position + new Vector3(0, 1, 0);

            //Set our current lifespanTimer to the maximum
            lifespanTimer = lifespanMax;

            //Pulse
            StartCoroutine(Pulse());
        }

        private void Update()
        {
            //LifespanTimer
            UpdateLifespan();

            //If we should rise, rise
            if (shouldRise)
            {
                StartCoroutine(Rise());
            }
        }

        //Manage the timer
        private void UpdateLifespan()
        {
            //Decrease the time
            lifespanTimer -= Time.deltaTime;

            //if lifespan is up, destroy this object
            if (lifespanTimer <= 0)
            {
                //Destroy the object
                Destroy(this.gameObject);
            }
        }

        //Collision detection
        void OnCollisionEnter(Collision col)
        {
            //If we collided with another feature point..
            if (col.gameObject.tag.Equals("DepthFeaturePoint"))
            {
                Destroy(this.gameObject);
            }
        }

        //Pulse the size up and down a bit
        private IEnumerator Pulse()
        {
            System.Func<float, float, bool> GreaterThanOrEqual = (a, b) => a >= b;
            System.Func<float, float, bool> LessThanOrEqual = (a, b) => a <= b;
            while (shouldPulse)
            {
                switch (_pulseState)
                {
                    case PulseState.increaseOne:
                        CalculateRatio(increase1_Max, pulseSpeed_Initial, PulseState.shrinkOne,
                            GreaterThanOrEqual);
                        yield return new WaitForEndOfFrame();
                        break;

                    case PulseState.shrinkOne:
                        CalculateRatio(shrink1_Min, pulseSpeed_Initial, PulseState.increaseTwo,
                            LessThanOrEqual);
                        //Start rising
                        shouldRise = true;
                        yield return new WaitForEndOfFrame();
                        break;

                    case PulseState.increaseTwo:
                        CalculateRatio(increase2_Max, pulseSpeed_WithRise, PulseState.shrinkTwo,
                            GreaterThanOrEqual);
                        yield return new WaitForEndOfFrame();
                        break;

                    case PulseState.shrinkTwo:
                        CalculateRatio(shrink2_Min, pulseSpeed_WithRise, PulseState.empty,
                            LessThanOrEqual);
                        yield return new WaitForEndOfFrame();
                        break;
                    case PulseState.empty:
                        shouldPulse = false;
                        break;
                }
            }

            yield break;
        }

        private void CalculateRatio
        (
            float end,
            float delta,
            PulseState nextState,
            System.Func<float, float, bool> operation
        )
        {
            currentRatio = Mathf.MoveTowards(currentRatio, end, delta);
            this.gameObject.transform.localScale = Vector3.one * currentRatio;

            if (operation(currentRatio, end))
            {
                _pulseState = nextState;
            }
        }

        //Rise
        private IEnumerator Rise()
        {
            //Instantiate 'time'
            float time = 0;

            //While we haven't hit our destination
            while (this.transform.position.y <= positionDest.y)
            {
                //Lerp
                this.transform.position = Vector3.Lerp(positionStart, positionDest, time / riseSpeed);
                this.transform.rotation = Quaternion.identity;

                //Increase time
                time += Time.deltaTime;

                yield return null;
            }

            //Move
            this.transform.position = positionDest;
            this.transform.rotation = Quaternion.identity;

            yield return new WaitForEndOfFrame();
        }

        //Set our scaling targets based on the prefab's original scale (so instead of setting the scale,
        // set the scale as a product relative to the prefab's original scale)
        private void SetScalingProperties()
        {
            increase1_Max *= originalScale;
            shrink1_Min *= originalScale;
            increase2_Max *= originalScale;
            shrink2_Min *= originalScale;
        }
    }
}