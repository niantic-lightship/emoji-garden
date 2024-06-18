// Copyright 2022-2024 Niantic.
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
    public class OneButtonWithImageModalView : ModalView
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private Animator animator;

        public override void SetupModal(ModalDescription modalDescription)
        {
            Sprite sprite = Sprite.Create
            (
                modalDescription.image,
                new Rect(0, 0, modalDescription.image.width, modalDescription.image.height),
                new Vector2(0.5f, 0.5f)
            );
            image.sprite = sprite;

            if (modalDescription.animatorController != null)
            {
                animator.runtimeAnimatorController = modalDescription.animatorController;
            }
            base.SetupModal(modalDescription);
        }
    }
}