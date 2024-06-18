// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    // Class used to populate a lightship modal with strings and callbacks
    public class ModalDescription
    {
        public Texture2D image;
        public string header;
        public string body;
        public string primaryButtonText;
        public Action primaryButtonCallback;
        public string secondaryButtonText;
        public Action secondaryButtonCallback;
        public RuntimeAnimatorController animatorController;
    }
    
    // LightshipModalManager is a singleton that handles displaying and hiding of lightship modals.
    // It provides a single point of entry for displaying and hiding lightship modals from anywhere in your application. 
    public class LightshipModalManager : MonoBehaviour
    {
        [SerializeField]
        private TwoButtonWithImageModalView _twoButtonWithImageModalView;

        [SerializeField]
        private OneButtonWithImageModalView _oneButtonWithImageModalView;

        [SerializeField]
        private TwoButtonModalView _twoButtonModalView;

        [SerializeField]
        private PermissionsModalView _permissionsModalView;

        [SerializeField]
        private ModalView _modalView;

        private static LightshipModalManager instance;
        private ModalView _activeModal = null;

        public enum ModalType
        {
            OneButtonModal,
            PermissionsModal,
            TwoButtonModal,
            OneButtonWithImageModal,
            TwoButtonWithImageModal
        }

        // Public property to access the Singleton instance.
        public static LightshipModalManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // If no instance exists, try to find an existing one in the scene.
                    instance = FindObjectOfType<LightshipModalManager>();

                    // If no instance is found, create a new GameObject and attach the script to it.
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(LightshipModalManager).Name);
                        instance = singletonObject.AddComponent<LightshipModalManager>();
                    }
                }

                return instance;
            }
        }

        // Displays a modal
        // If another modal is currently visible, will first hide that modal
        public void DisplayModal(ModalType modalType, ModalDescription modalDescription, Canvas targetCanvas)
        {
            HideModal();
            ModalView modalToInstantiate = null;
            switch (modalType)
            {
                case (ModalType.OneButtonModal):
                {
                    modalToInstantiate = _modalView;
                    break;
                }
                case (ModalType.PermissionsModal):
                {
                    modalToInstantiate = _permissionsModalView;
                    break;
                }
                case (ModalType.TwoButtonModal):
                {
                    modalToInstantiate = _twoButtonModalView;
                    break;
                }
                case (ModalType.OneButtonWithImageModal):
                {
                    modalToInstantiate = _oneButtonWithImageModalView;
                    break;
                }
                case (ModalType.TwoButtonWithImageModal) :
                {
                    modalToInstantiate = _twoButtonWithImageModalView;
                    break;
                }
            }
            
            if (modalToInstantiate != null)
            {
                _activeModal = Instantiate(modalToInstantiate, targetCanvas.transform);
                _activeModal.SetupModal(modalDescription);
                _activeModal.DisplayModal();
            }
        }

        public void HideModal()
        {
            if (_activeModal != null)
            {
                _activeModal.HideModal();
                _activeModal = null;
            }
        }

        private void OnEnable()
        {
            // Ensure there's only one instance. If another instance exists, destroy this one.
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}