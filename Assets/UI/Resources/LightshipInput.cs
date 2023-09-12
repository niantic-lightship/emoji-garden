//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.6.3
//     from Assets/LocalizationUX/Resources/LightshipInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @LightshipInput: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @LightshipInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""LightshipInput"",
    ""maps"": [
        {
            ""name"": ""Input"",
            ""id"": ""0441efd8-cf4a-4f54-9575-e620f871d873"",
            ""actions"": [
                {
                    ""name"": ""PrimaryTouch"",
                    ""type"": ""Value"",
                    ""id"": ""5587b327-fc71-48bf-ad8d-e78e9560da96"",
                    ""expectedControlType"": ""Touch"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SecondaryTouch"",
                    ""type"": ""Value"",
                    ""id"": ""0e25d142-0aa1-408a-b1c2-d8128da03184"",
                    ""expectedControlType"": ""Touch"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MouseScroll"",
                    ""type"": ""Value"",
                    ""id"": ""999680a4-775f-4be4-85b9-ebe9d80d052b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MousePan"",
                    ""type"": ""Value"",
                    ""id"": ""dbf4bfe6-227d-4c8c-9394-44aab5af9e12"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MouseHold"",
                    ""type"": ""Button"",
                    ""id"": ""7eb15c42-52e1-4803-80ae-e3521cdfcdd4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""8b99124f-47cf-4ca4-90df-4317fefd9b3c"",
                    ""path"": ""<Touchscreen>/touch0"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryTouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""da48dbed-bb28-40bc-a4ef-64b45c4aa545"",
                    ""path"": ""<Touchscreen>/touch1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryTouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9a2527ae-2e1b-4e87-85c4-eebb3539109b"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseScroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""ecf34196-58c5-4614-b0a2-8287b515b50b"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePan"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""813b8f15-fafc-4898-8678-ce51d3f54545"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""b49fa1e5-ecbf-46c1-a2be-a66aade8702f"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""dd2cea80-3525-4cb3-897a-a7c2b2f322de"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseHold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Input
        m_Input = asset.FindActionMap("Input", throwIfNotFound: true);
        m_Input_PrimaryTouch = m_Input.FindAction("PrimaryTouch", throwIfNotFound: true);
        m_Input_SecondaryTouch = m_Input.FindAction("SecondaryTouch", throwIfNotFound: true);
        m_Input_MouseScroll = m_Input.FindAction("MouseScroll", throwIfNotFound: true);
        m_Input_MousePan = m_Input.FindAction("MousePan", throwIfNotFound: true);
        m_Input_MouseHold = m_Input.FindAction("MouseHold", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Input
    private readonly InputActionMap m_Input;
    private List<IInputActions> m_InputActionsCallbackInterfaces = new List<IInputActions>();
    private readonly InputAction m_Input_PrimaryTouch;
    private readonly InputAction m_Input_SecondaryTouch;
    private readonly InputAction m_Input_MouseScroll;
    private readonly InputAction m_Input_MousePan;
    private readonly InputAction m_Input_MouseHold;
    public struct InputActions
    {
        private @LightshipInput m_Wrapper;
        public InputActions(@LightshipInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @PrimaryTouch => m_Wrapper.m_Input_PrimaryTouch;
        public InputAction @SecondaryTouch => m_Wrapper.m_Input_SecondaryTouch;
        public InputAction @MouseScroll => m_Wrapper.m_Input_MouseScroll;
        public InputAction @MousePan => m_Wrapper.m_Input_MousePan;
        public InputAction @MouseHold => m_Wrapper.m_Input_MouseHold;
        public InputActionMap Get() { return m_Wrapper.m_Input; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(InputActions set) { return set.Get(); }
        public void AddCallbacks(IInputActions instance)
        {
            if (instance == null || m_Wrapper.m_InputActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_InputActionsCallbackInterfaces.Add(instance);
            @PrimaryTouch.started += instance.OnPrimaryTouch;
            @PrimaryTouch.performed += instance.OnPrimaryTouch;
            @PrimaryTouch.canceled += instance.OnPrimaryTouch;
            @SecondaryTouch.started += instance.OnSecondaryTouch;
            @SecondaryTouch.performed += instance.OnSecondaryTouch;
            @SecondaryTouch.canceled += instance.OnSecondaryTouch;
            @MouseScroll.started += instance.OnMouseScroll;
            @MouseScroll.performed += instance.OnMouseScroll;
            @MouseScroll.canceled += instance.OnMouseScroll;
            @MousePan.started += instance.OnMousePan;
            @MousePan.performed += instance.OnMousePan;
            @MousePan.canceled += instance.OnMousePan;
            @MouseHold.started += instance.OnMouseHold;
            @MouseHold.performed += instance.OnMouseHold;
            @MouseHold.canceled += instance.OnMouseHold;
        }

        private void UnregisterCallbacks(IInputActions instance)
        {
            @PrimaryTouch.started -= instance.OnPrimaryTouch;
            @PrimaryTouch.performed -= instance.OnPrimaryTouch;
            @PrimaryTouch.canceled -= instance.OnPrimaryTouch;
            @SecondaryTouch.started -= instance.OnSecondaryTouch;
            @SecondaryTouch.performed -= instance.OnSecondaryTouch;
            @SecondaryTouch.canceled -= instance.OnSecondaryTouch;
            @MouseScroll.started -= instance.OnMouseScroll;
            @MouseScroll.performed -= instance.OnMouseScroll;
            @MouseScroll.canceled -= instance.OnMouseScroll;
            @MousePan.started -= instance.OnMousePan;
            @MousePan.performed -= instance.OnMousePan;
            @MousePan.canceled -= instance.OnMousePan;
            @MouseHold.started -= instance.OnMouseHold;
            @MouseHold.performed -= instance.OnMouseHold;
            @MouseHold.canceled -= instance.OnMouseHold;
        }

        public void RemoveCallbacks(IInputActions instance)
        {
            if (m_Wrapper.m_InputActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IInputActions instance)
        {
            foreach (var item in m_Wrapper.m_InputActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_InputActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public InputActions @Input => new InputActions(this);
    public interface IInputActions
    {
        void OnPrimaryTouch(InputAction.CallbackContext context);
        void OnSecondaryTouch(InputAction.CallbackContext context);
        void OnMouseScroll(InputAction.CallbackContext context);
        void OnMousePan(InputAction.CallbackContext context);
        void OnMouseHold(InputAction.CallbackContext context);
    }
}