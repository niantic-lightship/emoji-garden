// Copyright 2022-2024 Niantic.
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// Used for AR gameplay
#if !UNITY_EDITOR
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
#endif

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// The Garden initializes the rotation of the overall flowerbed, the location name on the sign, and passes tap information to each flower
    /// </summary>
    public class Garden : MonoBehaviour
    {
        [Header("In Scene References")]
        [SerializeField]
        private ARPlaneManager _planeManager;

        [SerializeField]
        private Camera _camera;

        [Header("Prefab References")]
        [SerializeField]
        private List<TextMeshPro> textMeshes = new();

        [SerializeField]
        private Flower _flower1;

        [SerializeField]
        private Flower _flower2;

        [SerializeField]
        private Flower _flower3;

        [SerializeField]
        private GameObject _gardenRoot;

        [SerializeField]
        private EmojiGardenUI _ui;

        [SerializeField]
        private float _cmFromGround = 15.0f;

        [SerializeField]
        private float _timeToWaitUntilShowingPlaneHelperInSec = 2.0f;

        [SerializeField]
        private float _planeHelperTimerIncrement = 3.0f;

        // Disable warning about unused variable.
        // The value is used in code that is not run in the editor, which triggers a warning in editor mode.
#pragma warning disable 414
        [SerializeField]
        private float _initalRayFurthestDistance = 3.33333f;

        [SerializeField]
        private int _backupRayCount = 4;
#pragma warning restore 414

        private bool _updateContentPosition = false;
        private Vector3 _contentTarget = Vector3.zero;
        private float _waitingForPlanesTimer = 0.0f;
        private float _timeToWaitForPlanes = 0.0f;

        private Transform _originalParent;

        public Action OnPlacementFinished;

        // Local client placement logic

        public void BeginPlacement(Transform origin)
        {
            SetOriginAsParent(origin);
            _ui.HideWaitingForPlacement();
            _planeManager.enabled = true;
            _gardenRoot.transform.position = _camera.transform.position;

            _timeToWaitForPlanes = _timeToWaitUntilShowingPlaneHelperInSec;

            _ui.InitialPlacement += OnInitialPlacement;
            _ui.RetryPlacement += OnRetryPlacement;
            _ui.ConfirmPlacement += OnConfirmPlacement;

            // Begin casting onto planes (on be default), show garden, show UI
            _updateContentPosition = true;

            _flower1.gameObject.SetActive(false);
            _flower2.gameObject.SetActive(false);
            _flower3.gameObject.SetActive(false);
        }

        private void OnInitialPlacement()
        {
            _updateContentPosition = false;
        }

        private void OnRetryPlacement()
        {
            _updateContentPosition = true;
        }

        private void OnConfirmPlacement()
        {
            _planeManager.enabled = false;

            if (_ui != null)
            {
                _ui.InitialPlacement -= OnInitialPlacement;
                _ui.RetryPlacement -= OnRetryPlacement;
                _ui.ConfirmPlacement -= OnConfirmPlacement;
                _ui.HideGardenPlacement();
                _ui.ShowPlayerCounter();
            }

            OnPlacementFinished?.Invoke();
        }

        private void RaycastForPlacement()
        {
            var cam = _camera.transform;

#if !UNITY_EDITOR
            var tempHits = _planeManager.Raycast(
                new Ray(cam.position, cam.forward), TrackableType.Planes, Allocator.Temp);

            // First attempt,
            // Raycast directly out from camera forward. Intersect with planes. Place at offset from
            // plane.

            if (tempHits.Length > 0)
            {
                var hit = tempHits[0];
                if (Vector3.Distance(cam.position, hit.pose.position) <= _initalRayFurthestDistance)
                {
                    UpdatePlacement(hit.pose.position, cam);
                    return;
                }
            }

            // Second attempt,
            // Find `totalRays` points along the previous raycast. Raycast straight down from each
            // point. Use the point that is closest to the garden's current position. Don't use the
            // camera's position as one of these points.
            //
            // Garden starts hidden on top of the camera until the first candidate
            // placement is found.

            // + 1 prevents garden placement directly under camera
            float increment = _initalRayFurthestDistance / (_backupRayCount + 1);
            float closestHit = float.MaxValue;
            Vector3 closestHitPos = Vector3.zero;

            for (int i = 0; i < _backupRayCount; ++i)
            {
                var ray = new Ray(
                    cam.position + cam.forward * (_initalRayFurthestDistance - increment * i),
                    Vector3.down);
                var secondHits = _planeManager.Raycast(
                    ray,
                    TrackableType.Planes,
                    Allocator.Temp);
                if (secondHits.Length > 0)
                {
                    var hit = secondHits[0];
                    var dist = Vector3.Distance(_gardenRoot.transform.position, hit.pose.position);
                    if (closestHit > dist)
                    {
                        closestHit = dist;
                        closestHitPos = hit.pose.position;
                    }
                }
            }

            if (closestHit < float.MaxValue)
            {
                UpdatePlacement(closestHitPos, cam);
                return;
            }
#else
            _gardenRoot.SetActive(true);
            _ui.ShowGardenPlacement(true);
            _gardenRoot.transform.position = cam.position + cam.forward * 2;
            var ld = cam.position - _gardenRoot.transform.position;
            _gardenRoot.transform.rotation =
                Quaternion.LookRotation(ld, Vector3.up);
#endif
        }

        private void UpdatePlacement(Vector3 position, Transform cam)
        {
            _gardenRoot.SetActive(true);
            _ui.ShowGardenPlacement(true);

            _contentTarget = position + Vector3.up * _cmFromGround * 0.01f;

            // Rotation facing placer camera
            // Face garden towards player, but also aligned flat with vertical planes
            var lookDirection = cam.position - _gardenRoot.transform.position;
            lookDirection.y = 0.0f;

            _gardenRoot.transform.rotation =
                Quaternion.LookRotation(lookDirection, Vector3.up);
        }

        // Networked placement logic

        public void WaitForPlacement(Transform origin)
        {
            _planeManager.enabled = false;
            SetOriginAsParent(origin);
            _ui.ShowGardenPlacement(false);

            _flower1.gameObject.SetActive(false);
            _flower2.gameObject.SetActive(false);
            _flower3.gameObject.SetActive(false);
        }

        public void NetworkPlacement(byte[] networkMessage)
        {
            _ui.HideGardenPlacement();
            _ui.ShowPlayerCounter();

            DeserializeGardenInit
            (
                networkMessage,
                out var localPosition,
                out var localRotation,
                out var flower1State,
                out var flower2State,
                out var flower3State
            );

            _gardenRoot.transform.localPosition = localPosition;
            _gardenRoot.transform.localRotation = localRotation;
            _gardenRoot.SetActive(true);

            if (flower1State != null)
            {
                _flower1.ApplySerializedFlowerState(flower1State);
            }
            if (flower2State != null)
            {
                _flower2.ApplySerializedFlowerState(flower2State);
            }
            if (flower3State != null)
            {
                _flower3.ApplySerializedFlowerState(flower3State);
            }

            OnPlacementFinished?.Invoke();
        }

        private static int SizeOfGardenPose()
        {
            return 7 * sizeof(float);
        }

        private byte[] SerializeGardenInit
        (
            Vector3 position,
            Quaternion rotation,
            bool withFlowerState
        )
        {
            byte[] byteArray;
            var size = SizeOfGardenPose();
            if (withFlowerState)
            {
                size += 3 * Flower.SizeOfFlowerBuffer();
            }
            byteArray = new byte[size];

            unsafe
            {
                fixed (byte* ptrBytes = byteArray)
                {
                    int walker = 0;
                    walker = PrimativeWriter.Write(ptrBytes, walker, position);
                    walker = PrimativeWriter.Write(ptrBytes, walker, rotation);

                    if (!withFlowerState)
                    {
                        return byteArray;
                    }

                    var flower1State = _flower1.SerializeFlowerState();
                    var flower2State = _flower2.SerializeFlowerState();
                    var flower3State = _flower3.SerializeFlowerState();

                    walker = PrimativeWriter.Write(ptrBytes, walker, flower1State);
                    walker = PrimativeWriter.Write(ptrBytes, walker, flower2State);
                    walker = PrimativeWriter.Write(ptrBytes, walker, flower3State);

                    return byteArray;
                }
            }
        }

        private void DeserializeGardenInit
        (
            byte[] byteArray,
            out Vector3 positionOut,
            out Quaternion rotationOut,
            out byte[] flower1State,
            out byte[] flower2State,
            out byte[] flower3State
        )
        {
            bool hasFlowers = byteArray.Length > SizeOfGardenPose();

            unsafe
            {
                fixed (byte* ptrBytes = byteArray)
                {
                    int walker = 0;
                    walker = PrimativeReader.Read(ptrBytes, walker, out positionOut);
                    walker = PrimativeReader.Read(ptrBytes, walker, out rotationOut);

                    if (!hasFlowers)
                    {
                        flower1State = null;
                        flower2State = null;
                        flower3State = null;
                        return;
                    }

                    int fStateSize = Flower.SizeOfFlowerBuffer();

                    flower1State = new byte[fStateSize];
                    flower2State = new byte[fStateSize];
                    flower3State = new byte[fStateSize];

                    walker = PrimativeReader.Read(ptrBytes, walker, out flower1State, fStateSize);
                    walker = PrimativeReader.Read(ptrBytes, walker, out flower2State, fStateSize);
                    walker = PrimativeReader.Read(ptrBytes, walker, out flower3State, fStateSize);
                }
            }
        }

        // Common logic

        private void StartUpdatingContentAgainAfterPlaneHelperHides()
        {
            _updateContentPosition = true;
            _waitingForPlanesTimer = 0.0f;

            _timeToWaitForPlanes = _timeToWaitUntilShowingPlaneHelperInSec +
                _planeHelperTimerIncrement;

            _ui.PlaneHelperHidden -= StartUpdatingContentAgainAfterPlaneHelperHides;
        }

        protected void Update()
        {
            if (!_updateContentPosition)
            {
                return;
            }

            _waitingForPlanesTimer += Time.deltaTime;

            RaycastForPlacement();

            if (_waitingForPlanesTimer >= _timeToWaitForPlanes && !_gardenRoot.activeSelf)
            {
                _updateContentPosition = false;
                _ui.PlaneHelperHidden += StartUpdatingContentAgainAfterPlaneHelperHides;
                _ui.ShowPlaneHelper();
            }

            float speed = 1.0f;
            var diff = _contentTarget - _gardenRoot.transform.position;
            var mag = diff.magnitude;
            var norm = diff / mag;
            if (mag < speed)
            {
                _gardenRoot.transform.position = _contentTarget;
            }
            else
            {
                _gardenRoot.transform.position += norm * speed;
            }
        }

        public byte[] MakeGardenInitMessageForNewPeer(bool withFlowerState)
        {
            return SerializeGardenInit
            (
                _gardenRoot.transform.localPosition,
                _gardenRoot.transform.localRotation,
                withFlowerState
            );
        }

        // Apply the name of the location/wayspot to the sign
        public void SetLocationText(string locationName)
        {
            foreach (TextMeshPro text in textMeshes)
            {
                text.text = locationName;
            }
        }

        private void SetOriginAsParent(Transform origin)
        {
            if (_originalParent != null)
                return;

            _originalParent = _gardenRoot.transform.parent;
            _gardenRoot.transform.SetParent(origin);
        }

        public void Reset() // Called by MultiplayerController
        {
            _planeManager.enabled = true;
            _gardenRoot.transform.SetParent(_originalParent);
            _originalParent = null;
            _updateContentPosition = false;
        }

        protected void OnDisable()
        {
            _gardenRoot.SetActive(false);
            _ui.HideGardenPlacement();
        }
    }
}
