// Copyright 2022-2024 Niantic.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
    public class CutoutMaskUI: Image
    {
        // Start is called before the first frame update
        public override Material materialForRendering
        {
            get
            {
                Material material = new Material(base.materialForRendering);
                material.SetInt("_SteniclComp", (int)CompareFunction.NotEqual);
                return material;
            }
        }
    }
}
