// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
    public class TwoButtonWithImageModalView : ModalView
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private LightshipButton secondaryButton;

        [SerializeField]
        private Animator animator;

        private Action modalCanceledCallback;
        public event Action ModalCanceled;
        
        public override void SetupModal(ModalDescription modalDescription)
        {
            Sprite sprite = Sprite.Create
            (
                modalDescription.image,
                new Rect(0, 0, modalDescription.image.width, modalDescription.image.height),
                new Vector2(0.5f, 0.5f)
            );
            image.sprite = sprite;
            secondaryButton.SetText(modalDescription.secondaryButtonText);
            modalCanceledCallback = modalDescription.secondaryButtonCallback;
            ModalCanceled += modalCanceledCallback;
            
            if (modalDescription.animatorController != null)
            {
                animator.runtimeAnimatorController = modalDescription.animatorController;
            }
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