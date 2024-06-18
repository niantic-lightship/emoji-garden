// Copyright 2022-2024 Niantic.
using System;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public abstract class LightshipUITransitionDescriptor: MonoBehaviour
    {
        public float transitionLength;

        public abstract void TransitionIn(Action callback);

        public abstract void TransitionOut(Action callback);
    }
}