using System.Linq;
using UnityEditor;
using UnityEngine;

using ComponentPoolInitializerElement = Bert.Pool.ComponentPoolInitializer.PoolInitializerElement;

namespace Bert.Pool.Editor
{
    [CustomPropertyDrawer(typeof(ComponentPoolInitializerElement))]
    internal sealed class ComponentPoolInitializerElementDrawer : PropertyDrawer
    {
        private const float Padding = 2f;
        private const float ButtonWidth = 36f;
        internal const float QuantityFieldWidth = 60f;
        internal const float SourceRectPercent = 0.55f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sourceRect = DrawSource(position, property);

            position.x += sourceRect.width;
            position.width -= sourceRect.width;

            float quantityFieldWidth = Mathf.Min(QuantityFieldWidth, position.width / 2f);
            Rect quantityRect = new Rect(position) {width = quantityFieldWidth - Padding};
            EditorGUI.PropertyField(quantityRect, property.FindPropertyRelative(nameof(ComponentPoolInitializerElement._quantity)), GUIContent.none);

            position.x += quantityFieldWidth;
            
            Rect parentRect = new Rect(position) {width = position.width - quantityFieldWidth};
            EditorGUI.PropertyField(parentRect, property.FindPropertyRelative(nameof(ComponentPoolInitializerElement._parent)), GUIContent.none);
        }

        private static Rect DrawSource(Rect position, SerializedProperty property)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"{nameof(ComponentPoolInitializerElement)}.{nameof(ComponentPoolInitializerElement._source)}");

            var sourceRect = new Rect(position) {width = position.width * SourceRectPercent};
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
            var selector = new TypeDropdown(
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