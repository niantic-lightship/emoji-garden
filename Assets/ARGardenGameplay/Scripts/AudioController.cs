// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Sequence music audioclip loop to play after stinger intro audioclip.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _musicIntro;

        [SerializeField]
        private AudioSource _musicLoop;

        private void OnEnable()
        {
            _musicLoop.PlayDelayed(_musicIntro.clip.length);
        }
    }
}
