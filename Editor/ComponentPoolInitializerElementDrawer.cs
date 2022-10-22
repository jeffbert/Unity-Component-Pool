using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

using ComponentPoolInitializerElement = Bert.Pool.ComponentPoolInitializer.PoolInitializerElement;

namespace Bert.Pool.Editor
{
    [CustomPropertyDrawer(typeof(ComponentPoolInitializerElement))]
    internal sealed class ComponentPoolInitializerElementDrawer : PropertyDrawer
    {
        private const float Padding = 4f;
        private const float ButtonWidth = 36f;
        private const float QuantityFieldWidth = 70f;
        private const float DontDestroyFieldWidth = 30f;

        private const float QuantityWidthRatio = QuantityFieldWidth / (QuantityFieldWidth + DontDestroyFieldWidth);

        internal static float GetQuantityFieldWidth(Rect rect)
        {
            return Math.Min(QuantityFieldWidth, rect.width / 3f * QuantityWidthRatio);
        }
        
        internal static float GetDontDestroyFieldWidth(Rect rect)
        {
            return Math.Min(DontDestroyFieldWidth, rect.width / 3f * (1 - QuantityWidthRatio));
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sourceRect = DrawSource(position, property);
            
            position.x += sourceRect.width;

            float quantityFieldWidth = GetQuantityFieldWidth(position);
            Rect quantityRect = new Rect(position)
            {
                width = quantityFieldWidth - Padding
            };
            EditorGUI.PropertyField(quantityRect, property.FindPropertyRelative(nameof(ComponentPoolInitializerElement._quantity)), GUIContent.none);

            position.x += quantityFieldWidth;

            float dontDestroyFieldWidth = GetDontDestroyFieldWidth(position);
            Rect dontDestroyRect = new Rect(position)
            {
                width = dontDestroyFieldWidth
            };
            EditorGUI.PropertyField(dontDestroyRect, property.FindPropertyRelative(nameof(ComponentPoolInitializerElement._dontDestroy)), GUIContent.none);
        }

        private static Rect DrawSource(Rect position, SerializedProperty property)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"{nameof(ComponentPoolInitializerElement)}.{nameof(ComponentPoolInitializerElement._source)}");

            var sourceRect = new Rect(position) {width = position.width - GetQuantityFieldWidth(position) - GetDontDestroyFieldWidth(position)};
            var rect = sourceRect;

            var sourceProperty = property.FindPropertyRelative(nameof(ComponentPoolInitializerElement._source));
            
            var sourceComponent = sourceProperty.objectReferenceValue as Component;
            if (sourceComponent != null)
            {
                Rect buttonRect = new Rect(position) {width = ButtonWidth + Padding};
                if (GUI.Button(buttonRect, EditorGUIUtility.IconContent("CustomTool", "Change source component")))
                {
                    ShowSelectorDropdown(sourceProperty, sourceComponent, sourceRect);
                }

                rect.x += buttonRect.width + 1;
                rect.width -= buttonRect.width + 0.5f;
            }

            rect.width -= Padding;

            sourceProperty.objectReferenceValue = EditorGUI.ObjectField(
                rect,
                GUIContent.none,
                sourceProperty.objectReferenceValue,
                typeof(Component),
                false
            );

            return sourceRect;
        }


        private static void ShowSelectorDropdown(SerializedProperty sourceProperty, Component sourceComponent, Rect rect)
        {
            var selector = new ComponentTypeDropdown(
                sourceComponent.GetComponents(typeof(Component)).Select(c => c.GetType()).ToArray(),
                type =>
                {
                    sourceProperty.objectReferenceValue = sourceComponent.GetComponent(type);
                    sourceProperty.serializedObject.ApplyModifiedProperties();
                },
                "Select Component"
            );
            selector.Show(rect);
        }
    }
}