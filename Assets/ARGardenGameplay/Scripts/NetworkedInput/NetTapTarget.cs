// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Used for communicating a tap events from the collidable part of the flower to the flower class.
    /// This is attached to the PlantHeadMesh within the flower object heirachy.
    /// </summary>
    public class NetTapTarget : MonoBehaviour
    {
        [SerializeField]
        private NetTapManager _tapManager;

        [SerializeField]
        [HideInInspector]
        private string _strGuid = Guid.NewGuid().ToString();

        public Guid UniqueNetworkId 
        {
            get => new(_strGuid);
        }

        public void OnEnable()
        {
            if (_tapManager)
            {
                _tapManager.AddTapTarget(this, GetComponentInParent<INetTapListener>());
            }
        }

        public void OnDisable()
        {
            if (_tapManager)
            {
                _tapManager.RemoveTapTarget(this);
            }
        }
    }
}
