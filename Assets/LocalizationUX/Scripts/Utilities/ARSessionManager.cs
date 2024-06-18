// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Niantic.Lightship.AR.Samples
{
    public class ARSessionManager : MonoBehaviour
    {
        [SerializeField]
        private ARSession _session;

        [SerializeField]
        private PermissionsManager _permissionsManager;

        public Action<bool> ARSessionStatus;

        public void OnDisable()
        {
            _permissionsManager.cameraGrantStatus -= CameraGrantReturn;
        }

        public void EnableARSession()
        {
            _permissionsManager.cameraGrantStatus += CameraGrantReturn;
            _permissionsManager.AskForPermission(PermissionsManager.PermissionType.camera);
        }

        public void DisableARSession()
        {
            _session.enabled = false;
        }

        private void CameraGrantReturn(bool cameraGranted)
        {
            if (cameraGranted)
            {
                StartCoroutine(DelayedEnabled()); //short delay on restarting the AR session to avoid issues with ARFoundation
            }
            else
            {
                ARSessionStatus?.Invoke(false);
            }
        }

        /*
        * When returning from granting permission, some devices can have issues when enabling the ARSession as soon
        * as the app regains focus. These slight delays allow session to enable appropriately
        */
        private IEnumerator DelayedEnabled()
        {
            _session.Reset();
            yield return new WaitForSeconds(0.1f);
            _session.enabled = true;
            yield return new WaitForSeconds(0.1f);
            ARSessionStatus?.Invoke(true);
        }
    }
}
