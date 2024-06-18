// Copyright 2022-2024 Niantic.
using UnityEngine;

// For android permissions
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

// For ios plugin calls
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Niantic.Lightship.AR.Samples
{
    public static class PermissionCommunicator
    {
        private const string AndroidLocationSettingString = "android.settings.LOCATION_SOURCE_SETTINGS";
        private const string AndroidCameraSettingString = "android.settings.CAMERA_SETTINGS";
        
        public enum PermissionStatus
        {
            NotDetermined = 0,
            Restricted = 1,
            Denied = 2,
            AuthorizedAlways = 3,
            AuthorizedInUse = 4,
            Unknown = -1
        }
        
        #if UNITY_IOS && !UNITY_EDITOR
            [DllImport("__Internal")]
            private static extern int _CameraPermissionStatus();
                
            [DllImport("__Internal")] 
            private static extern int _LocationPermissionStatus();
        
            [DllImport("__Internal")]
            private static extern void _RequestCameraPermission();
        #endif
        
        public static void OpenSettings(PermissionsManager.PermissionType permissionType)
        {
            #if UNITY_IOS && !UNITY_EDITOR 
                OpenIOSSettings();
            #elif UNITY_ANDROID && !UNITY_EDTOR
                var permissionString = "";
                switch ( permissionType)
                {
                    case PermissionsManager.PermissionType.camera :
                        permissionString = AndroidCameraSettingString;
                        break;
                    case PermissionsManager.PermissionType.location :
                        permissionString = AndroidLocationSettingString;
                        break;
                }
                OpenAndroidSettings(permissionString);
            #endif
        }

        public static bool IsPermissionGranted(PermissionsManager.PermissionType permissionType)
        {
            var retVal = true;
            switch (permissionType)
            {
                case (PermissionsManager.PermissionType.location):
                    retVal = PermissionCommunicator.IsLocationPermissionGranted();
                    break;
                case (PermissionsManager.PermissionType.camera):
                    retVal = PermissionCommunicator.IsCameraPermissionGranted();
                    break;
            }

            return retVal;
        }
        
        public static void RequestCameraPermission() 
        {
            #if UNITY_IOS && !UNITY_EDITOR
                _RequestCameraPermission();
            #else
                Debug.LogWarning("Platform does not support this method to request camera permission.");
            #endif
        }
        
        public static int CameraInFlight()
        {
            int status = (int)PermissionStatus.Restricted;
            #if UNITY_IOS && !UNITY_EDITOR
                status = _CameraPermissionStatus();
            #endif
            return status;
        }

        private static bool IsLocationPermissionGranted()
        {
            bool isGranted = true;
            #if UNITY_IOS && !UNITY_EDITOR
                int status = _LocationPermissionStatus();
                 if ((status != (int)PermissionStatus.AuthorizedAlways) && (status != (int)PermissionStatus.AuthorizedInUse)) {
                    isGranted = false;
                } 
            #elif UNITY_ANDROID && !UNITY_EDITOR
                isGranted = Permission.HasUserAuthorizedPermission(Permission.FineLocation);
            #endif
            return isGranted;
        }

        private static bool IsCameraPermissionGranted()
        {
            bool isGranted = true;

            #if UNITY_IOS && !UNITY_EDITOR
                int status = _CameraPermissionStatus();
                if ((status != (int)PermissionStatus.AuthorizedAlways) && (status != (int)PermissionStatus.AuthorizedInUse)) {
                    isGranted = false;
                }
            #elif UNITY_ANDROID && !UNITY_EDITOR
                isGranted = Permission.HasUserAuthorizedPermission(Permission.Camera);
            #endif
            return isGranted;
        }


        private static void OpenIOSSettings()
        {
            Application.OpenURL("app-settings:");
        }

        private static void OpenAndroidSettings(string permission)
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string packageName = currentActivityObject.Call<string>("getPackageName");
 
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
        }

        private static bool CallAndroidPermissionMethod(string methodName)
        {
            using (AndroidJavaClass permissionChecker =
                new AndroidJavaClass("com.nianticlabs.permissionsmanager.PermissionsManager"))
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity =
                        unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        return permissionChecker.CallStatic<bool>(methodName, currentActivity);
                    }
                }
            }
        }
    }
}
