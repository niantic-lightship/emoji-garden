// Copyright 2022-2024 Niantic.
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
    // Enforces a max size on canvas elements. Used primarily in the modals to keep them from stretching too far on
    // wider devices
    [ExecuteInEditMode]
    public class EnforceMaxSize : MonoBehaviour
    {
        [SerializeField]
        private LayoutElement layoutElement;
        private CanvasScaler canvasScaler;

        public float maxWidth;

        private void Start()
        {
            canvasScaler = GetComponentInParent<CanvasScaler>(); // Find the CanvasScaler in the parent hierarchy
        }
        
        private void Update()
        {
            float displayWidth = Screen.width;
            float maxWidthRatio = 1f;
            if (canvasScaler != null)
            {
                // Get the reference resolution from the Canvas Scaler
               displayWidth =  canvasScaler.referenceResolution.x;
               maxWidthRatio = displayWidth / Screen.width; //adjust our maxwidth by the ratio of canvas scaler to screen width
            }

            // Calculate the desired width based on screen resolution or other criteria
            float desiredWidth = displayWidth * 0.95f;

            // Enforce the maximum width
            desiredWidth = Mathf.Min(desiredWidth, maxWidth * maxWidthRatio);

            // Set the width in the Layout Element component
            layoutElement.preferredWidth = desiredWidth;
        }
    }
}