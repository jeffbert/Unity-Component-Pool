using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Bert.Pool.Editor
{
    [CustomEditor(typeof(ComponentPoolInitializer))]
    internal sealed class ComponentPoolInitializerEditor : UnityEditor.Editor
    {
        private const float HeaderOffset = 14f;
        private ReorderableList _list;

        private void OnEnable()
        {
            _list = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(ComponentPoolInitializer._elements)))
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        
        private static void DrawHeader(Rect rect)
        {
            rect.x += HeaderOffset;
            rect.width -= HeaderOffset;

            float quantityFieldWidth = ComponentPoolInitializerElementDrawer.GetQuantityFieldWidth(rect);
            float dontDestroyFieldWidth = ComponentPoolInitializerElementDrawer.GetDontDestroyFieldWidth(rect);
            
            Rect sourceRect = new Rect(rect) {width = rect.width - quantityFieldWidth - dontDestroyFieldWidth};
            GUI.Label(sourceRect, new GUIContent("Source", "Source component to pool."));

            rect.x += sourceRect.width;

            Rect quantityRect = new Rect(rect) {width = quantityFieldWidth};
            GUI.Label(quantityRect, new GUIContent("Quantity", "Amount of instances to create in the pool."));
            
            rect.x += quantityRect.width;
            
            Rect dontDestroyRect = new Rect(rect) {width = dontDestroyFieldWidth};
            GUI.Label(dontDestroyRect, new GUIContent("DD", "(Don't destroy) Created instances won't be destroyed when loading a new scene."));
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.PropertyField(rect, _list.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none);
        }
    }
}