// Copyright 2022-2024 Niantic.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
    public class PermissionsModalView : ModalView
    {
        [SerializeField]
        private Button secondaryButton;

        [SerializeField]
        private Image image;
        
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
            secondaryButton.GetComponentInChildren<TMP_Text>().text = modalDescription.secondaryButtonText;
            modalCanceledCallback = modalDescription.secondaryButtonCallback;
            ModalCanceled += modalCanceledCallback;
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
