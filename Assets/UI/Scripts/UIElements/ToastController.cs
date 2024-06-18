// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class ToastController : MonoBehaviour
    {
        [SerializeField]
        private LightshipToast toastPrefab;

        //UI
        private Transform targetTransform;
        private LightshipToast activeToast;

        public Transform TargetTransform
        {
            get => targetTransform;
            set => targetTransform = value;
        }

        public void DisplayToast(string toastMessage)
        { 
            SetToastMessage(toastMessage);
        }

        public void DisplayToast(string toastMessage, float timeoutInSeconds)
        {
            DisplayToast(toastMessage);
            activeToast.HideAfterWait(timeoutInSeconds, HideToast);
        }

        public void HideToast()
        {
            Action resetToast = () =>
            {
                if (activeToast != null)
                {
                    Destroy(activeToast.gameObject);
                }

                activeToast = null;
            };
            
            if (activeToast != null)
            {
                activeToast.HideToast(resetToast);
            }
        }

        private void SetToastMessage(string toastMessage)
        {
            if (activeToast != null)
            {
                activeToast.ChangeToastText(toastMessage);
            }
            else
            {
                activeToast = Instantiate(toastPrefab, targetTransform);
                activeToast.DisplayToastWithText(toastMessage);
            }
        }
    }
}
