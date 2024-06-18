// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    //Uncomment this line if you want to create custom modal data
    //[CreateAssetMenu(fileName = "TwoButtonWithImageModalData", menuName = "Lightship UX Toolkit/Scriptable Objects/Modal Data/Two Button Modal Data")]
    public class TwoButtonModalData : OneButtonModalData
    {
        [SerializeField]
        private string secondaryText;

        public string SecondaryText => secondaryText;
    }
}