// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Send messages from the Flower animators back to the flower, as are not on the same game object.
    /// </summary>
    public class FlowerMessenger : MonoBehaviour
    {
        [SerializeField]
        private Flower _flower;

        public void NextState()
        {
            // This event is still present in the animators, which throw errors if this method
            // isn't here. But we don't want to respond to it anymore. This is a temporary fix
            // until we do a pass on the animators themselves.
        }

        public void PopFinished()
        {
            _flower.PopFinished();
        }

        public void EnableFloatingHead()
        {
            _flower.EnableFloatingHeadRoot();
        }

        public void PlayParticles()
        {
            _flower.PlayPopFX();
        }

        public void PlaySpinSFX()
        {
            _flower.PlaySpinSFX();
        }

        public void PlayInitialGrowSFX()
        {
            _flower.PlayInitialGrowSFX();
        }

        public void PlayPluckSFX()
        {
            _flower.PlayPluckSFX();
        }

        public void SwapDisplayedHead()
        {
            _flower.PlayPluckSFX();
            _flower.SwapDisplayedHead();
        }

        public void PlaySwellSFX()
        {
            _flower.PlaySwellSFX();
        }
    }
}
