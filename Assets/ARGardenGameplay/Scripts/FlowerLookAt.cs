// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Behavior for turning the flower to face the player (where the player is the camera).
    /// </summary>
    public class FlowerLookAt : MonoBehaviour
    {
        [SerializeField]
        private float _maxSpeed = 100.0f;

        [SerializeField]
        private float _minSpeed = 10.0f;

        [SerializeField]
        private float _maxAngle = 160.0f;

        [SerializeField]
        private float _minAngle = 5.0f;

        public Transform Target { get; set; }

        // Rotate the direction of the game object towards the camera
        private void Update()
        {
            if (Target == null)
            {
                transform.localRotation = Quaternion.identity;
                return;
            }

            // Find the direction vector between the game object and the main camera
            // Then convert it to 2D as viewed from above (using only the X and Z values, as we don't want the object to look up or down)
            Vector3 targetDirection = new Vector3(Target.position.x, transform.position.y, Target.position.z) - transform.position;
            Vector2 targetDirection2D = new(targetDirection.x, targetDirection.z);
            Vector2 forward2D = new(transform.forward.x, transform.forward.z);

            // Calculate the total required rotation to reach the target direction
            float totalRotation = Vector2.Angle(forward2D, targetDirection2D);

            // Calculate the rotation angle as a percentage of the total rotation allowed as defined by minAngle and maxAngle
            float normalizedAngle = Mathf.Clamp(totalRotation, _minAngle, _maxAngle) / (_maxAngle - _minAngle);

            // Calculate the speed that the object should rotate at based on the minSpeed and maxSpeed
            float currentSpeed = Mathf.Lerp(_minSpeed, _maxSpeed, normalizedAngle);
            float singleStep = Mathf.Deg2Rad * currentSpeed * Time.deltaTime;

            // Rotate towards the targetDirection by singleStep
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }
}
