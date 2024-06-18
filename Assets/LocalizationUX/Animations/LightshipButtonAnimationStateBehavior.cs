// Copyright 2022-2024 Niantic.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    
    public class LightshipButtonAnimationStateBehavior : StateMachineBehaviour
    {
        LightshipButton _button;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _button = animator.gameObject.GetComponent<LightshipButton>();
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _button.OnClickAnimationFinish();
        }
    }

}