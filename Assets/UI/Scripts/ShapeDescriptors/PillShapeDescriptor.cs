// Copyright 2022-2024 Niantic.
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
    [RequireComponent(typeof(Image))]
    public class PillShapeDescriptor: LightshipUIShapeDescriptor
    {
        private Image _image;
        private float _imageSize = 186;

        public override void Init()
        {
            this._image = GetComponent<Image>();
            base.Init();
        }

        public override void SetShape()
        {
            float currentHeight = _rectTransform.rect.height;
            _image.pixelsPerUnitMultiplier = _imageSize / currentHeight;
        }
    }
}