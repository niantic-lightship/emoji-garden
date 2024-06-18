// Copyright 2022-2024 Niantic.
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples {
    public class ModalView : MonoBehaviour
    {
        [SerializeField]
        protected LightshipUITransitionDescriptor transitionDescriptor;

        [SerializeField]
        protected TMP_Text headerText;

        [SerializeField]
        protected TMP_Text bodyText;

        [SerializeField]
        protected LightshipButton primaryButton;

        protected List<LightshipButton> _modalButtons = new List<LightshipButton>(); 
        protected Action modalConfirmedCallback;
        protected bool hasChosen;
        
        public event Action ModalConfirmed;

        public virtual void SetupModal(ModalDescription modalDescription)
        {
            if (headerText != null)
            {
                headerText.text = modalDescription.header;
            }

            bodyText.text = modalDescription.body;
            primaryButton.SetText(modalDescription.primaryButtonText);
            modalConfirmedCallback = modalDescription.primaryButtonCallback;
            ModalConfirmed += modalConfirmedCallback;
            
            _modalButtons.Add(primaryButton);
        }

        public virtual void onConfirm()
        {
            if (hasChosen == false)
            {
                ModalConfirmed?.Invoke();
                transitionDescriptor.TransitionOut(ModelDidHide);
                hasChosen = true;
                SetButtonInteractable(false);
            }
        }

        public void DisplayModal()
        {
            transitionDescriptor.TransitionIn(null);
        }

        public void HideModal()
        {
            transitionDescriptor.TransitionOut(ModelDidHide);
        } 

        private void ModelDidHide()
        {
            {
                Destroy(gameObject);
            }
        }

        protected void SetButtonInteractable(bool state)
        {
            foreach (var button in _modalButtons)
            {
                button.interactable = state;
            }
        }
    }
}
