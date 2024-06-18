// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class TwoButtonModalView : ModalView
    {
        [SerializeField]
        private LightshipButton secondaryButton;
        
        private Action modalCanceledCallback;
        public event Action ModalCanceled;

        public override void SetupModal(ModalDescription modalDescription)
        {
            secondaryButton.SetText(modalDescription.secondaryButtonText);
            modalCanceledCallback = modalDescription.secondaryButtonCallback;
            ModalCanceled += modalCanceledCallback;
            _modalButtons.Add(secondaryButton);
            base.SetupModal(modalDescription);
        }
        
        public void OnCancel()
        {
            if (hasChosen == false)
            {
                ModalCanceled?.Invoke();
                HideModal();
                hasChosen = true;
                SetButtonInteractable(false);
            }
        }
    }
}
