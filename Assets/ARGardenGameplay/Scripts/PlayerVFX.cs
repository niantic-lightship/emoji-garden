// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class PlayerVFX : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _flower1Tap;

        [SerializeField]
        private ParticleSystem _flower2Tap;

        [SerializeField]
        private ParticleSystem _flower3Tap;

        [SerializeField]
        private GameObject _flower1Head;

        [SerializeField]
        private GameObject _flower2Head;

        [SerializeField]
        private GameObject _flower3Head;

        private GameObject[] _headArr;
        private ParticleSystem[] _psArr;
        private bool _currentClient;

        protected void Awake()
        {
            _headArr = new GameObject[]
            {
                _flower1Head,
                _flower2Head,
                _flower3Head
            };

            _psArr = new ParticleSystem[]
            {
                _flower1Tap,
                _flower2Tap,
                _flower3Tap
            };
        }

        public void IsCurrentClient(bool currentClient)
        {
            _currentClient = currentClient;
        }

        public void PlayTapVFX(int flowerId)
        {
            _psArr[flowerId - 1].Play();
        }
    }
}
