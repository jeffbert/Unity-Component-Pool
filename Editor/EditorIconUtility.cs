using System;
using UnityEditor;
using UnityEngine;

namespace Bert.Pool.Editor
{
    internal static class EditorIconUtility
    {
        // Icons are loaded and cached by the editor, so they can be used directly. They're not copied into new textures, since that
        // would create a texture (and sync with the render thread) on every GUI event.
        public static GUIContent GetEditorIcon(string iconName, string tooltip = "") => EditorGUIUtility.IconContent(iconName, tooltip);

        public static Texture2D GetTypeIcon(Type type)
        {
            if (EditorGUIUtility.ObjectContent(null, type).image is Texture2D icon)
            {
                return icon;
            }

            return EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
        }
    }
}
