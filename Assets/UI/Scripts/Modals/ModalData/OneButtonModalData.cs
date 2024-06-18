// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    //Uncomment this line if you want to create custom modal data
    //[CreateAssetMenu(fileName = "TwoButtonWithImageModalData", menuName = "Lightship UX Toolkit/Scriptable Objects/Modal Data/One Button Modal Data")]
    public class OneButtonModalData : ScriptableObject
    {
        [SerializeField]
        private string headerText;

        [SerializeField]
        private string bodyText;

        [SerializeField]
        private string primaryText;
        
        public string HeaderText => headerText;
        public string BodyText => bodyText;
        public string PrimaryText => primaryText;
    }  
}
