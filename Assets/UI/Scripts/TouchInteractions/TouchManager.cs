// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Niantic.Lightship.AR.Samples {
    public class TouchManager : MonoBehaviour
    {
        [Serializable]
        public class FloatEvent : UnityEvent<float>
        {
        };

        [Serializable]
        public class Vector2Event : UnityEvent<Vector2>
        {
        };

        [SerializeField]
        private float _pinchScrollSpeed;

        [SerializeField]
        private float _mouseScrollSpeed;

        [SerializeField]
        private float _touchPanSpeed;

        [SerializeField]
        private float _mousePanSpeed;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        public FloatEvent PinchEvent = new FloatEvent();

        [SerializeField]
        public Vector2Event MoveEvent = new Vector2Event();

        [SerializeField]
        public UnityEvent PanEnded = new UnityEvent();

        [SerializeField]
        public Vector2Event TouchUpEvent = new Vector2Event();

        private LightshipInput _lightshipInput;
        private bool _isPointerDown;
        private float _mouseDragDeadzoneRadius = 0.01f;
        private bool _IsPanActive = false;
        private bool _isOverUI = false;
        private float _lastPinchDistance = 0f;
        private bool _resetPinchDistance = false;
        private Coroutine activePinchCoroutine = null;

        private bool IsPointerDown
        {
            get => _isPointerDown;
            set
            {
                _isPointerDown = value;
                if ((value == false) && (_IsPanActive))
                {
                    _IsPanActive = false;
                    PanEnded?.Invoke();
                }
            }
        }

        private void Awake()
        {
            _lightshipInput = new LightshipInput();
            EnhancedTouchSupport.Enable();
        }

        private void OnEnable()
        {
            _lightshipInput.Enable();
        }

        private void OnDisable()
        {
            _lightshipInput.Disable();
        }

        private void Start()
        {
            _lightshipInput.Input.MouseScroll.performed += ctx => MouseScrollCallback(ctx);
            _lightshipInput.Input.MousePan.performed += ctx => MousePanCallback(ctx);
            _lightshipInput.Input.MouseHold.started += ctx => MouseHoldCallback(ctx);
            _lightshipInput.Input.MouseHold.canceled += ctx => MouseHoldCallback(ctx);
        }

        /// Mouse response

        private void MouseHoldCallback(InputAction.CallbackContext ctx)
        {
            if (_isOverUI)
            {
                return;
            }

            switch (ctx.phase)
            {
                case InputActionPhase.Started:
                    IsPointerDown = true;
                    break;
                case InputActionPhase.Canceled:
                    if (!_IsPanActive)
                    {
                        var point = new Vector2(Mouse.current.position.x.ReadValue(),
                            Mouse.current.position.y.ReadValue());

                        var didHitResponder = PreformTouchCast(point);
                        if (!didHitResponder)
                        {
                            TouchUpEvent?.Invoke(point);
                        }
                    }

                    IsPointerDown = false;

                    break;
            }
        }

        private void MousePanCallback(InputAction.CallbackContext ctx)
        {
            if (_isOverUI)
            {
                return;
            }

            var mousepan = ctx.ReadValue<Vector2>();
            if (ctx.phase == InputActionPhase.Performed)
            {
                _IsPanActive = true;
            }

            ProcessMove(mousepan * _mousePanSpeed);
        }

        private void MouseScrollCallback(InputAction.CallbackContext ctx)
        {

            var mousecroll = ctx.ReadValue<Vector2>();
            ProcessScrollZoom(mousecroll);
        }

        private float _openDebugMenuTimer = 0.0f;

        private void Update()
        {
            _isOverUI = EventSystem.current.IsPointerOverGameObject();

            // If the touch is over a UI element, we don't want to process a gesture
            var activeTouches = Touch.activeTouches;

            if (activeTouches.Count >= 3)
            {
                _openDebugMenuTimer += Time.deltaTime;
                if (_openDebugMenuTimer >= 1.5f)
                {
                    FindObjectOfType<ScrollingLog>().ShowLog();
                    _openDebugMenuTimer = 0.0f;
                }
                return;
            }
            else
            {
                _openDebugMenuTimer = 0.0f;
            }

            foreach (var touch in activeTouches)
            {
                var isOverUI = EventSystem.current.IsPointerOverGameObject(touch.touchId);

                if (isOverUI)
                {
                    return;
                }
            }

            if (activeTouches.Count > 1) // Process multitouch
            {
                if (activePinchCoroutine == null)
                {
                    activePinchCoroutine = StartCoroutine(ProcessPinch(activeTouches));
                }

                return; // We processed multitouch don't worry about going on
            }

            // We no longer an in a multi touch state, but our pinch coroutine is still running
            if (activePinchCoroutine != null)
            {
                StopCoroutine(activePinchCoroutine);
                activePinchCoroutine = null;
                return; // We just finished a multitouch
            }

            // Do we have any touches?
            if (activeTouches.Count == 1)
            {
                ProcessTouch(activeTouches[0]);
                return;
            }

            // We have no touches
            // Automatically runs the pan ending logic.
            if (activeTouches.Count == 0)
            {
                if (IsPointerDown)
                {
                    IsPointerDown = false;
                }
            }
        }

        private void ProcessMove(Vector2 delta)
        {
            MoveEvent?.Invoke(delta);
        }

        private IEnumerator ProcessPinch(ReadOnlyArray<Touch> activeTouches)
        {
            _resetPinchDistance = true;
            while (true)
            {
                yield return null;

                var touchOne = activeTouches[0];
                var touchTwo = activeTouches[1];

                var pinchDistance = Vector2.Distance(touchOne.screenPosition, touchTwo.screenPosition);
                if (_resetPinchDistance)
                {
                    _lastPinchDistance = pinchDistance;
                    _resetPinchDistance = false;
                }

                float pinchDelta = (pinchDistance - _lastPinchDistance);
                _lastPinchDistance = pinchDistance;

                PinchEvent?.Invoke(pinchDelta * _pinchScrollSpeed);
            }
        }

        private void ProcessScrollZoom(Vector2 MouseScrollDelta)
        {
            var yScrollDelta = MouseScrollDelta.y * _mouseScrollSpeed;
            PinchEvent?.Invoke(yScrollDelta);
        }

        private void ProcessTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    IsPointerDown = true;
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                    if (_IsPanActive)
                    {
                        break;
                    }
                    var didHitResponder = PreformTouchCast(touch.screenPosition);
                    if (!didHitResponder)
                    {
                        TouchUpEvent?.Invoke(touch.screenPosition);
                    }
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                    if (touch.delta.sqrMagnitude > _mouseDragDeadzoneRadius*_mouseDragDeadzoneRadius)
                    {
                        _IsPanActive = true;
                        ProcessMove(touch.delta * _touchPanSpeed);
                    }
                    break;
            }
        }

        private bool PreformTouchCast(Vector2 touchPosition)
        {
            Ray ray = _camera.ScreenPointToRay(touchPosition);

            // If we didn't intersect with anything, resize the selected marker if it exists
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var hitResponder = hit.collider.GetComponent<TouchResponder>();
                if (hitResponder != null)
                {
                    hitResponder.DidTouchUp();
                    return true;
                }
            }

            return false;
        }
    }
}
