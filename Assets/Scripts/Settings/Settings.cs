using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using Blameless.Configuration;

public static class Settings {

    private static Dictionary<string, List<Binding>> bindings;
    private static List<IBindingRefreshable> refreshables;

    private static Configuration conf;

    static Settings() {
        conf = new Configuration("player");
        conf.AddConverter(new ResolutionConverter());

        BindingSet.onActivated = PushBindingSet;
    }

    public static void Initialize() {
        Initialize(conf);
    }

    public static void Initialize(Configuration configuration) {
        bindings = new Dictionary<string, List<Binding>>();
        refreshables = new List<IBindingRefreshable>();

        conf = configuration;

        if (conf.OnChange == null) {
            conf.OnChange = SyncValue;
        }
        conf.Initialize();

        Rebind();

        Debug.Log("Settings has been initialized");
    }

    public static void Set<T>(string property, T value) {
        conf.Set<T>(property, value);
    }

    public static T Get<T>(string property) {
        return conf.Get<T>(property);
    }

    public static void Save() {
        conf.Save();
    }

    public static void Revert() {
        conf.Revert();
        TriggerRefreshables();
    }

    public static void Revert(string property) {
        conf.Revert(property);
        TriggerRefreshables();
    }

    public static bool HasChanged() {
        return conf.HasChanged();
    }

    public static bool HasChanged(string property) {
        return conf.HasChanged(property);
    }

    public static void KeepChanged(string property) {
        conf.KeepChanged(property);
    }

    public static void Reset() {
        conf.Reset();
        Rebind();
    }

    public static Configuration Conf {
        get { return conf; }
    }

    private static void PushBindingSet(BindingSet bindingSet) {
        foreach (Binding binding in bindingSet.bindings) {
            if (!bindings.ContainsKey(binding.source)) {
                bindings.Add(binding.source, new List<Binding>());
            }

            bindings[binding.source].Add(binding);

            if (binding.component is IBindingRefreshable && !refreshables.Contains((IBindingRefreshable)binding.component)) {
                refreshables.Add((IBindingRefreshable)binding.component);
				refreshables.Last<IBindingRefreshable>().Refresh();
            }

			SyncValue(binding.source, conf.Get<string>(binding.source));
        }
    }

    private static void Rebind() {
        bindings.Clear();
        refreshables.Clear();

		if (BindingSet.allReferenced == null){
			return; // there are no active bindings in the current scene
		}

        foreach (BindingSet bindingSet in BindingSet.allReferenced) {
            PushBindingSet(bindingSet);
        }
    }

    private static void SyncValue(string property, string value) {
        if (bindings.ContainsKey(property)) {
            foreach (Binding b in bindings[property]) {
                if (b.BindingType != null) {
					try {
                    	b.SetValue(conf.GetConverter(b.BindingType).ConvertTo(value));
					} catch(Exception e) {
						Debug.LogWarning(string.Format("Converter of type {0} was not found in the configuration. Cannot sync value [{1}={2}].", b.BindingType, property, value));
					}
                } else {
                    Debug.LogWarning(string.Format("Binding type and the corresponding target is not set for binding source {0} and component {1} of game object {2}", property, b.component, b.component.gameObject));
                }
            }
        } else {
            Debug.LogWarning("Binding not found for \"" + property + "\". Sync skipped.");
        }
    }

    private static void TriggerRefreshables() {
        foreach (IBindingRefreshable refreshable in refreshables) {
            refreshable.Refresh();
        }
    }
}
public class ResolutionConverter : Converter<Resolution> {
    protected override string DoConvertFrom(Resolution input) {
        return string.Format("{0}x{1}", input.width, input.height);
    }

    protected override Resolution DoConvertTo(string input) {
        Resolution r = new Resolution();
        string[] res = input.Split('x');
        r.width = int.Parse(res[0]);
        r.height = int.Parse(res[1]);
        return r;
    }
}