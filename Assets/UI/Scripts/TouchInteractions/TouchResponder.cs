// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public class TouchResponder : MonoBehaviour
    {
        public Action<TouchResponder> DidTouchUpEvent;

        public void DidTouchDown(){}

        public void DidTouchUp()
        {
            DidTouchUpEvent?.Invoke(this);
        }

        public void DidTouchMove(){}
    }
}
