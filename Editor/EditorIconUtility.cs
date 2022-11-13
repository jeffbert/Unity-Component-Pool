using System;
using UnityEditor;
using UnityEngine;

namespace Bert.Pool.Editor
{
    internal static class EditorIconUtility
    {
        public static Texture2D GetEditorIcon(string iconName, string text = "")
        {
            return CreateExternalTexture(EditorGUIUtility.IconContent(iconName, text).image);
        }
        
        public static Texture2D GetTypeIcon(Type type)
        {
            return CreateExternalTexture(EditorGUIUtility.ObjectContent(null, type).image);
        }
        
        private static Texture2D CreateExternalTexture(Texture texture)
        {
            if (texture == null)
            {
                texture = EditorGUIUtility.IconContent("cs Script Icon").image;
                
                if (texture == null)
                    return new Texture2D(0, 0);
            }
            
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.ARGB32,
                false,
                false,
                texture.GetNativeTexturePtr()
            );
        }
    }
}