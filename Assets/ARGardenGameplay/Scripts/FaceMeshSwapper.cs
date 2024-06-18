// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace  Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Behavior for animating the face of the flowers. The _currentIndex field is powered by the
    /// Flower's Animator and this component switches the mesh used for the Flower's face in
    /// LateUpdate.
    /// </summary>
    public class FaceMeshSwapper : MonoBehaviour
    {
        [UnityEngine.Animations.DiscreteEvaluation]
        [SerializeField] private int _currentIndex;
        [SerializeField] Mesh[] _meshArray;
        [SerializeField] MeshFilter _meshFilter;

        // Using LateUpdate to catch _currentIndex changes on the current frame. If we were to use
        // Update instead, we risk being a frame behind because the Animator component's Update ran
        // after this component's Update. Meaning the _meshFilter won't change until the next frame.
        protected void LateUpdate()
        {
            _meshFilter.sharedMesh = _meshArray[_currentIndex];
        }
    }
}
