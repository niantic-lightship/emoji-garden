// Copyright 2022-2024 Niantic.
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Used to control each flower and its state
    /// Receives messages from the Garden, FlowerMessenger, and IFlowerStates
    /// </summary>
    public class Flower : MonoBehaviour, INetTapListener
    {
        [SerializeField]
        private Animator _groundedAnimator;
        public Animator GroundedAnimator => _groundedAnimator;

        [SerializeField]
        private Animator _floatingAnimator;
        public Animator FloatingAnimator => _floatingAnimator;

        [SerializeField]
        private ScaleTweener _groundedObjectToScale;

        [SerializeField]
        private ScaleTweener _floatingObjectToScale;

        [SerializeField]
        private GameObject _groundedHead;

        [SerializeField]
        private GameObject _floatingHead;

        [SerializeField]
        private GameObject _floatingHeadRoot;

        [Header("VFX")]
        [SerializeField]
        private ParticleSystem _bustParticles;

        [SerializeField]
        private ParticleSystem _tapSparks;

        [SerializeField]
        private FollowTarget _tapFollowTarget;

        [SerializeField]
        private Transform _groundedTapFollowTarget;

        [SerializeField]
        private Transform _floatTapFollowTarget;

        [Header("Audio Sources")]
        [SerializeField]
        private AudioSource _tapAudio;

        [SerializeField]
        private AudioSource _floatAudio;

        [SerializeField]
        private AudioSource _swellAudio;

        [SerializeField]
        private AudioSource _popStingerAudio;

        [SerializeField]
        private AudioSource _popFXAudio;

        [SerializeField]
        private AudioSource _spinAudio;

        [SerializeField]
        private AudioSource _initialGrowAudio;

        [SerializeField]
        private AudioSource _pluckAudio;

        [SerializeField]
        private AudioSource _scatterAudio;

        [Header("Audio Clips")]
        [SerializeField]
        private List<AudioClip> _tapSFXList;

        [SerializeField]
        private List<AudioClip> _floatSFXList;

        [SerializeField]
        private List<AudioClip> _swellSFXList;

        [SerializeField]
        private List<AudioClip> _popSFXList;

        [SerializeField]
        private List<AudioClip> _pluckSFXList;

        [SerializeField]
        private FlowerLookAt _flowerLookAt;

        private int _currentTapAudio = 0;
        private Vector3 _headStartScale = Vector3.one;
        private long _lastTimestamp = 0;
        private int _plantState = 0;

        private const long TAP_COOLDOWN = 3000; // 3s

        public void TweenGroundedPlantHead(float scale, bool reset = true)
        {
            _groundedObjectToScale.StartScaleTween(scale, reset);
        }

        public bool IsGroundedPlantHeadTweening()
        {
            return _groundedObjectToScale.IsTweening;
        }

        public void TweenFloatingPlantHead(float scale, bool reset = true)
        {
            _floatingObjectToScale.StartScaleTween(scale, reset);
        }

        public bool IsFloatingPlantHeadTweening()
        {
            return _floatingObjectToScale.IsTweening;
        }

        // Enable the base game object of the floating head plant
        public void EnableFloatingHeadRoot()
        {
            FloatingAnimator.SetBool(FlowerAnimatorConsts.Floating_IsFloatingLabel, true);
            _floatingAnimator.Play(FlowerAnimatorConsts.Grounded_PlantWitherStateLabel);
        }

        // Enable the visual of the floating head plant
        public void SwapDisplayedHead()
        {
            _groundedHead.SetActive(false);
            _floatingHead.SetActive(true);
        }

        public void PlayPopFX()
        {
            _bustParticles.Play();
            _popStingerAudio.Play();
            _scatterAudio.Play();

            PlayRandomAudioClip(_popFXAudio, _popSFXList);
        }

        public void PlayTapFX()
        {
            _tapSparks.Play();
            _currentTapAudio = Mathf.Clamp(_currentTapAudio, 0, _tapSFXList.Count - 1);
            _tapAudio.clip = _tapSFXList[_currentTapAudio];
            _tapAudio.Play();
            _currentTapAudio++;
        }

        public void PlayFloatSFX()
        {
            PlayRandomAudioClip(_floatAudio, _floatSFXList);
        }

        public void PlaySwellSFX()
        {
            PlayRandomAudioClip(_swellAudio, _swellSFXList);
        }

        public void PlaySpinSFX()
        {
            _spinAudio.Play();
        }

        public void PlayInitialGrowSFX()
        {
            _initialGrowAudio.Play();
        }

        public void PlayPluckSFX()
        {
            PlayRandomAudioClip(_pluckAudio, _pluckSFXList);
        }

        // Pick a random audio clip from the provided list and play on the provided audio source
        private void PlayRandomAudioClip(AudioSource audioSource, List<AudioClip> audioClips)
        {
            if (audioClips != null && audioClips.Count > 0)
            {
                AudioClip randomClip = audioClips[Random.Range(0, audioClips.Count)];
                audioSource.clip = randomClip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Gives the size of the byte[] that will be returned by SerializeFlowerState.
        /// This is used when we want to pre-allocate space for multiple flower's serialzied states.
        /// </summary>
        /// <returns>Size of the byte[] returned from SerializeFlowerState</returns>
        public static int SizeOfFlowerBuffer()
        {
            return 2 * sizeof(int) + 2 * sizeof(float) + sizeof(int) + sizeof(long);
        }

        public byte[] SerializeFlowerState()
        {
            var stateInfo0 = GroundedAnimator.GetCurrentAnimatorStateInfo(0);
            var stateInfo1 = FloatingAnimator.GetCurrentAnimatorStateInfo(0);
            var data = new byte[SizeOfFlowerBuffer()];
            unsafe
            {
                fixed (byte* root = data)
                {
                    var stateHashPtr0 = (int*)(root);
                    *stateHashPtr0 = stateInfo0.fullPathHash;

                    var stateHashPtr1 = (int*)(root + sizeof(int));
                    *stateHashPtr1 = stateInfo1.fullPathHash;

                    var normTimePtr0 = (float*)(root + 2 * sizeof(int));
                    *normTimePtr0 = stateInfo0.normalizedTime;

                    var normTimePtr1 = (float*)(root + 2 * sizeof(int) + sizeof(float));
                    *normTimePtr1 = stateInfo1.normalizedTime;

                    var plantStatePtr = (int*)(root + 2 * sizeof(int) + 2 * sizeof(float));
                    *plantStatePtr = _plantState;

                    var timestampPtr = (long*)(root + 2 * sizeof(int) + 2 * sizeof(float) + sizeof(int));
                    *timestampPtr = _lastTimestamp;
                }
            }

            return data;
        }

        public void DeserializeFlowerState
        (
            byte[] data,
            out int stateHash0,
            out int stateHash1,
            out float normTime0,
            out float normTime1,
            out int plantState,
            out long timestamp
        )
        {
            unsafe
            {
                fixed (byte* root = data)
                {
                    var stateHashPtr0 = (int*)(root);
                    stateHash0 = *stateHashPtr0;

                    var stateHashPtr1 = (int*)(root + sizeof(int));
                    stateHash1 = *stateHashPtr1;

                    var normTimePtr0 = (float*)(root + 2 * sizeof(int));
                    normTime0 = *normTimePtr0;

                    var normTimePtr1 = (float*)(root + 2 * sizeof(int) + sizeof(float));
                    normTime1 = *normTimePtr1;

                    var plantStatePtr = (int*)(root + 2 * sizeof(int) + 2 * sizeof(float));
                    plantState = *plantStatePtr;

                    var timestampPtr = (long*)(root + 2 * sizeof(int) + 2 * sizeof(float) + sizeof(int));
                    timestamp = *timestampPtr;
                }
            }
        }

        private bool _started = false;
        private byte[] _cachedFlowerState = null;

        public void ApplySerializedFlowerState(byte[] data)
        {
            if (!_started)
            {
                _cachedFlowerState = data;
                return;
            }

            DeserializeFlowerState
            (
                data,
                out int stateHash0,
                out int stateHash1,
                out float normTime0,
                out float normTime1,
                out _plantState,
                out _lastTimestamp
            );

            GroundedAnimator.Play(stateHash0, 0, normTime0);
            FloatingAnimator.Play(stateHash1, 0, normTime1);
            GroundedAnimator.SetInteger(FlowerAnimatorConsts.Grounded_PlantLevelLabel, _plantState);

            var isPopping = _plantState == (int)FlowerAnimatorConsts.PlantStates.Popping;
            var isFloating =
                _plantState == (int)FlowerAnimatorConsts.PlantStates.Floating ||
                isPopping;

            _groundedHead.SetActive(!isFloating);
            _floatingHead.SetActive(isFloating);
            FloatingAnimator.SetBool(FlowerAnimatorConsts.Floating_IsFloatingLabel, isFloating);

            if (isPopping)
            {
                FloatingAnimator.SetTrigger(FlowerAnimatorConsts.Floating_PopTriggerLabel);
            }
            else
            {
                FloatingAnimator.ResetTrigger(FlowerAnimatorConsts.Floating_PopTriggerLabel);
            }
        }

        public void PopFinished()
        {
            FloatingAnimator.ResetTrigger(FlowerAnimatorConsts.Floating_PopTriggerLabel);
            GroundedAnimator.SetInteger(FlowerAnimatorConsts.Grounded_PlantLevelLabel, 0);
            GroundedAnimator.Play(FlowerAnimatorConsts.Grounded_SpawnAnimationLabel);

            _floatingHead.SetActive(false);
            FloatingAnimator.SetBool(FlowerAnimatorConsts.Floating_IsFloatingLabel, false);
            _groundedHead.SetActive(true);
            _plantState = 0;
        }

        public void NetworkTap(long timestamp, GameObject tappingPlayer, bool isCurrentClient)
        {
            if (timestamp < (_lastTimestamp + TAP_COOLDOWN))
            {
                if (isCurrentClient)
                {
                    Handheld.Vibrate();
                }
                return;
            }

            if (!isCurrentClient)
            {
                tappingPlayer.GetComponent<PlayerVFX>().PlayTapVFX(1); // TODO: use this flower FX
            }

            _lastTimestamp = timestamp;
            _flowerLookAt.Target = tappingPlayer.transform;

            _plantState += 1;
            if (_plantState > FlowerAnimatorConsts.Grounded_PlantLevelMax)
                _plantState = 0;

            switch ((FlowerAnimatorConsts.PlantStates)_plantState)
            {
                case FlowerAnimatorConsts.PlantStates.Tiny:
                {
                    PopFinished();
                    break;
                }
                case FlowerAnimatorConsts.PlantStates.Medium:
                {
                    GroundedAnimator.SetInteger(FlowerAnimatorConsts.Grounded_PlantLevelLabel, 1);
                    break;
                }
                case FlowerAnimatorConsts.PlantStates.Large:
                {
                    GroundedAnimator.SetInteger(FlowerAnimatorConsts.Grounded_PlantLevelLabel, 2);
                    break;
                }
                case FlowerAnimatorConsts.PlantStates.Floating:
                {
                    GroundedAnimator.SetInteger(FlowerAnimatorConsts.Grounded_PlantLevelLabel, 3);
                    break;
                }
                case FlowerAnimatorConsts.PlantStates.Popping:
                {
                    FloatingAnimator.SetTrigger(FlowerAnimatorConsts.Floating_PopTriggerLabel);
                    break;
                }
            }
        }

        protected void Awake()
        {
            _flowerLookAt = GetComponent<FlowerLookAt>();
        }

        protected void OnEnable()
        {
            _plantState = 0;
            _lastTimestamp = 0;
            _flowerLookAt.Target = null;
            _groundedHead.SetActive(true);
            _floatingHead.SetActive(false);

            _started = true;
            if (_cachedFlowerState != null) {
                ApplySerializedFlowerState(_cachedFlowerState);
            }
        }

        protected void OnDisable()
        {
            _started = false;
            _cachedFlowerState = null;
        }
    }

    internal class FlowerAnimatorConsts
    {
        public const string Grounded_PlantWitherStateLabel = "WitherHeadDetach";
        public const string Grounded_PlantLevelLabel = "PlantLevel";
        public const string Grounded_SpawnAnimationLabel = "SpawnGrow";
        public enum PlantStates
        {
            Tiny,
            Medium,
            Large,
            Floating,
            Popping
        }
        public const int Grounded_PlantLevelMax = (int)PlantStates.Popping;

        public const string Floating_PopTriggerLabel = "Pop";
        public const string Floating_IsFloatingLabel = "Float";
    }
}
