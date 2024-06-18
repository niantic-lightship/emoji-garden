// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    //Uncomment this line if you want to create custom modal data
    //[CreateAssetMenu(fileName = "TwoButtonWithImageModalData", menuName = "Lightship UX Toolkit/Scriptable Objects/Modal Data/Two Button With Image Modal Data")] 
    public class TwoButtonWithImageModalData : OneButtonModalData
    {
        [SerializeField]
        private Texture2D image;

        [SerializeField]
        private string secondaryText;

        [SerializeField]
        private RuntimeAnimatorController animatorController;
        
        public Texture2D ModalImage => image;
        public string SecondaryText => secondaryText;
        public RuntimeAnimatorController AnimatorController => animatorController;
    }
}
