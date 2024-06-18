// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
  [RequireComponent(typeof(RectTransform))]
  public abstract class LightshipUIShapeDescriptor: MonoBehaviour
  {
    protected RectTransform _rectTransform;

    public virtual void Init()
    {
      _rectTransform = GetComponent<RectTransform>();
    }

    public abstract void SetShape();
  }
}
