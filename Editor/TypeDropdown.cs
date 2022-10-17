using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Bert.Pool.Editor
{
    /// <summary>
    /// Used to create a simple <see cref="Type"/> dropdown menu.
    /// </summary>
    public class TypeDropdown : AdvancedDropdown
    {
        private readonly IReadOnlyList<Type> _types;
        private readonly Action<Type> _typeSelectedCallback;
        private readonly string _title;

        /// <inheritdoc cref="TypeDropdown"/>
        /// <param name="types">Search for all types derived from this type.</param>
        /// <param name="typeSelectedCallback">Action to invoke when a type is selected.</param>
        /// <param name="title">Dropdown title.</param>
        public TypeDropdown(IReadOnlyList<Type> types, Action<Type> typeSelectedCallback, string title = null)
            : base(new AdvancedDropdownState())
        {
            _types = types ?? throw new ArgumentNullException(nameof(types));
            _typeSelectedCallback = typeSelectedCallback ?? throw new ArgumentNullException(nameof(typeSelectedCallback));
            _title = title ?? "Select Type";
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_title);
            for (int i = 0; i < _types.Count; i++)
            {
                var type = _types[i];
                var item = new AdvancedDropdownItem(type.Name)
                {
                    id = i,
                    icon = GetTypeIcon(type)
                };
                
                root.AddChild(item);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _typeSelectedCallback?.Invoke(_types[item.id]);
        }
        
        private static Texture2D GetTypeIcon(Type type)
        {   
            var texture = EditorGUIUtility.ObjectContent(null, type).image;
            if (texture == null)
            {
                texture = EditorGUIUtility.IconContent("cs Script Icon").image;
                if (texture == null)
                    texture = new Texture2D(0, 0);
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