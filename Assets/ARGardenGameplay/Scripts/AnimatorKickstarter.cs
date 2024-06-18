// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Used to make sure the animation starts correctly when enabled rather than showing the game object early
    /// </summary>
    public class AnimatorKickstarter : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        private void OnEnable()
        {
            _animator.Update(0);
        }
    }
}
