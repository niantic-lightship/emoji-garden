// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class GyroTilt : MonoBehaviour
    {
        private void Start()
        {
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleMouseTilt();
#else
            HandleGyroTilt();
#endif
        }

        private void HandleMouseTilt()
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(0, 0, mouseX);
        }

        private void HandleGyroTilt()
        {
            if (SystemInfo.supportsGyroscope)
            {
                float roll = Input.gyro.attitude.eulerAngles.y;
            
                // Normalizing the pitch to be between -180 to 180 degrees
                if (roll > 180)
                    roll -= 360;

                transform.localEulerAngles = new Vector3(0, 0, roll);
            }
        }
    }
}
