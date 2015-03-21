namespace Blameless.Configuration {
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System;

    [CustomPropertyDrawer(typeof(Binding))]
    public sealed class BindingDrawer : PropertyDrawer {

        private static Type[] bindingTypes;
        private int selectedType = 0;

        private Component[] components;
        private int selectedComponent = 0;

        private List<MemberInfo> fields = new List<MemberInfo>();
        private int selectedField = 0;

        private GameObject gameObject;

        static BindingDrawer() {
            bindingTypes = Assembly.GetAssembly(typeof(IConverter)).GetTypes()
                .Where(t => typeof(IConverter).IsAssignableFrom(t))
                .Where(t => !t.IsGenericType)
                .Where(t => !t.IsInterface)
                .Select(t => GetConverterType(t))
                .OrderBy(t => t.FullName)
                .ToArray<Type>();

        }

        private static Type GetConverterType(System.Type t) {
            return ((IConverter)Activator.CreateInstance(t)).GetConverterType();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return DrawerHeight(property, label) * 5 + 10;
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            gameObject = ((Component)prop.serializedObject.targetObject).gameObject;

            SerializedProperty source = prop.FindPropertyRelative("source");
            SerializedProperty component = prop.FindPropertyRelative("component");
            SerializedProperty field = prop.FindPropertyRelative("field");
            SerializedProperty type = prop.FindPropertyRelative("type");

            source.stringValue = EditorGUI.TextField(new Rect(pos.x, pos.y, pos.width, 20), "Source", source.stringValue);

            components = gameObject.GetComponents<Component>();
            selectedComponent = Array.FindIndex(components, c => c.Equals(component.objectReferenceValue));
            selectedComponent = Mathf.Max(0, EditorGUI.Popup(new Rect(pos.x, pos.y + 20, pos.width, 20), "Component", selectedComponent, components.Select(c => c.GetType().ToString()).ToArray<string>()));
            component.objectReferenceValue = components[selectedComponent];

			selectedType = Array.FindIndex(bindingTypes, c => c.AssemblyQualifiedName.Equals(type.stringValue));
            selectedType = EditorGUI.Popup(new Rect(pos.x, pos.y + 40, pos.width, 20), "Binding Type", selectedType, bindingTypes.Select(t => t.FullName).ToArray<string>());

            if (selectedType >= 0) {
                type.stringValue = bindingTypes[selectedType].AssemblyQualifiedName;

                fields.Clear();
                fields.AddRange(components[selectedComponent].GetType().GetFields().Where(i => i.FieldType.Equals(bindingTypes[selectedType])).ToArray<FieldInfo>());
                fields.AddRange(components[selectedComponent].GetType().GetProperties().Where(i => i.PropertyType.Equals(bindingTypes[selectedType])).ToArray<PropertyInfo>());
                if (fields.Count > 0) {
                    selectedField = fields.FindIndex(c => c.Name.Equals(field.stringValue));
                    selectedField = Mathf.Max(0, EditorGUI.Popup(new Rect(pos.x, pos.y + 60, pos.width, 20), "Field/Property", selectedField, fields.Select(i => i.Name).ToArray<string>()));
                    field.stringValue = fields[selectedField].Name;
                }
            }
        }

        private float DrawerHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label);
        }
    }
}