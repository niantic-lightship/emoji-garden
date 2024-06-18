// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    //Uncomment this line if you want to create custom modal data
    //[CreateAssetMenu(fileName = "TwoButtonWithImageModalData", menuName = "Lightship UX Toolkit/Scriptable Objects/Modal Data/One Button With Image Modal Data")]
    public class OneButtonWithImageModalData : OneButtonModalData
    {
        [SerializeField]
        private Texture2D image;

        [SerializeField]
        private RuntimeAnimatorController animatorController;
       
        public Texture2D ModalImage => image;
        public RuntimeAnimatorController AnimatorController => animatorController;
    }  
}