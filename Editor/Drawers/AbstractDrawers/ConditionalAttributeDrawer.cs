using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using System.Runtime.CompilerServices;

namespace YoukaiFox.Inspector
{
    public abstract class ConditionalAttributeDrawer<T> : YoukaiAttributeDrawer where T : ConditionalAttribute
    {
        public enum PropertyDrawing
        {
            Show, Hide, Disable
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            if (!IsComparisonValid(property))
                return;

            PropertyDrawing drawing = GetPropertyDrawing();

            switch (drawing)
            {
                case PropertyDrawing.Show:
                    EditorGUI.PropertyField(position, property);
                    break;
                case PropertyDrawing.Hide:
                    break;
                case PropertyDrawing.Disable:
                    GUI.enabled = false;
                    EditorGUI.PropertyField(position, property);
                    GUI.enabled = true;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsComparisonValid(property))
            {
                if (GetPropertyDrawing() == PropertyDrawing.Hide)
                    return 0f;
            }
            else
            {
                if (GetPropertyDrawing() == PropertyDrawing.Show)
                    return 0f;
            }

            return base.GetPropertyHeight(property, label);
        }

        protected abstract PropertyDrawing GetPropertyDrawing();

        private bool IsComparisonValid(SerializedProperty property)
        {
            System.Object objectInstance = property.GetTargetObjectWithProperty();
            var comparisonAttribute = attribute as ConditionalAttribute;
            FieldInfo field = objectInstance.GetField(comparisonAttribute.PropertyName);
            PropertyInfo hiddenProperty = objectInstance.GetProperty(comparisonAttribute.PropertyName);

            var objectValue = field != null ? field.GetValue(objectInstance) : 
                                        hiddenProperty.GetValue(objectInstance);

            if (!objectValue.ToBool(out bool memberValue))
                Debug.LogError($"Value {objectValue} is not a boolean");

            if (comparisonAttribute.TargetConditionValue == null)
                return memberValue;

            if (!comparisonAttribute.TargetConditionValue.ToBool(out bool targetConditionValue))
                Debug.LogError($"Value {comparisonAttribute.TargetConditionValue} is not a boolean");

            return memberValue == targetConditionValue;
        }
    }
}