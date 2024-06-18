// Copyright 2022-2024 Niantic.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    [CustomEditor(typeof(LocationBasedExperienceController))]
    public class LocationBasedExperienceControllerEditor : Editor
    {
        private SerializedProperty _interfaceImplementerProp;
        private SerializedProperty _rootProp;

        private void OnEnable()
        {
            // Link the serializedProperty to the actual serialized field using the property's name
            _interfaceImplementerProp = serializedObject.FindProperty("_appSerializedField");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_interfaceImplementerProp, new GUIContent("ILocationBasedExperience"));

            if (_interfaceImplementerProp.objectReferenceValue != null && !(_interfaceImplementerProp.objectReferenceValue is ILocationBasedExperience))
            {
                // This as cast is guranteed to not be null because the "_appRef" member is
                // a MonoBehaviour type
                var referenceAsMonobehaviour =_interfaceImplementerProp.objectReferenceValue as MonoBehaviour;
                var go = referenceAsMonobehaviour.gameObject;

                // If the interface isn't found in this step, GetComponent returns null and
                // we clear out the referenceValue which is the desired behavior
                var interfaceFoundOnObject = go.GetComponent<ILocationBasedExperience>() as MonoBehaviour;

                _interfaceImplementerProp.objectReferenceValue = interfaceFoundOnObject;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
