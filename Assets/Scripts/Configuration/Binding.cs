namespace Blameless.Configuration {
    using UnityEngine;
    using System.Collections;
    using System.Reflection;
    using System;

    [System.Serializable]
    public class Binding : ISerializationCallbackReceiver {

        public string source;
        public string type;
        public Component component;
        public string field;

        private Type bindingType;

        public void SetValue(object value) {
            if (value != null) {
                Type t = component.GetType();
                MemberInfo member;

                if ((member = t.GetField(field)) != null) {
                    (member as FieldInfo).SetValue(component, value);
                } else if ((member = t.GetProperty(field)) != null) {
                    (member as PropertyInfo).SetValue(component, value, null);
                }
            }
        }

        public void OnAfterDeserialize() {
            bindingType = Type.GetType(type);
        }

        public void OnBeforeSerialize() { }

        public Type BindingType {
            get { return bindingType; }
            set {
                bindingType = value;
				type = value.AssemblyQualifiedName;
			}
		}
	}
}