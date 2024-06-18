// Copyright 2022-2024 Niantic.

using UnityEditor;

namespace Niantic.Lightship.AR.Samples
{
#if UNITY_EDITOR
  [CustomEditor(typeof(LightshipButton))]
  public class ExtendedButtonEditor: Editor
  {
  }

  [CustomEditor(typeof(LightshipCircleButton))]
  public class CircledButtonEditor: ExtendedButtonEditor
  {
  }
#endif
}