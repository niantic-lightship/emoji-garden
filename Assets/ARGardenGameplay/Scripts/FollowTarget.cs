// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// This game object will always follow the position of the target transform.
    /// </summary>
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        private void Update()
        {
            transform.position = _target.transform.position;
        }
    }
}
