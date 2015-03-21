namespace Blameless.Configuration {
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    public class BindingSet : MonoBehaviour {

        public static List<BindingSet> allReferenced;
        public static BindingActivator onActivated;

        public delegate void BindingActivator(BindingSet bindingSet);

        void Awake() {
            if (allReferenced == null) {
                allReferenced = new List<BindingSet>();
            }

            if (!allReferenced.Contains(this)) {
                allReferenced.Add(this);
            }

            if (onActivated != null) {
                onActivated(this);
            }
        }

        public Binding[] bindings;
    }
}