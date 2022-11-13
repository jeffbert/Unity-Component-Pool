using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Bert.Pool.Editor
{
    /// <summary>
    /// Used to create a simple <see cref="Type"/> dropdown menu.
    /// </summary>
    public class ComponentTypeDropdown : AdvancedDropdown
    {
        private readonly IReadOnlyList<Type> _types;
        private readonly Action<Type> _typeSelectedCallback;
        private readonly string _title;
        
        /// <inheritdoc cref="ComponentTypeDropdown"/>
        /// <param name="types">Search for all types derived from this type.</param>
        /// <param name="typeSelectedCallback">Action to invoke when a type is selected.</param>
        /// <param name="title">Dropdown title.</param>
        public ComponentTypeDropdown(IReadOnlyList<Type> types, Action<Type> typeSelectedCallback, string title = null)
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
                    icon = EditorIconUtility.GetTypeIcon(type)
                };

                root.AddChild(item);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _typeSelectedCallback?.Invoke(_types[item.id]);
        }
    }
}