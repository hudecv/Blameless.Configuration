namespace Blameless.Configuration {
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Linq;
    using System;

    public sealed class Configuration {

        public static bool debug = false;
		private static char[] slash = new char[]{'/'};

        public delegate void ConfigurationChange(string property, string value);

        private string configFolder;
        private string configDefaultFolder;
        private string configName;
        private string configExtention;

        private ConfigurationChange onChange;

        private Dictionary<string, string> settings;
        private Dictionary<string, string> changed;

        private Dictionary<Type, IConverter> converters;

        public Configuration(string configName) {
            this.configName = configName;

            configDefaultFolder = "Settings";
            configFolder = Application.dataPath;
            configExtention = "cfg";

            converters = new Dictionary<Type, IConverter>();
            AddConverter(new IntegerConverter());
            AddConverter(new FloatConverter());
            AddConverter(new BooleanConverter());
			AddConverter(new StringConverter());
        }

        public void Initialize() {
            settings = new Dictionary<string, string>();
            changed = new Dictionary<string, string>();

            if (!ConfigExists() || debug) {
                LoadDefault();
            }

            ReadSettings();
        }

        public string ConfigName {
            get { return configName; }
            set { configName = Normalize(value); }
        }

        public string ConfigExtention {
            get { return configExtention; }
			set { configExtention = Normalize(value); }
		}
		
		public string ConfigFolder {
            get { return configFolder; }
			set { configFolder = Normalize(value); }
		}
		
		/// <summary>
        /// Gets or sets the folder path where the default configuration file resides. Defaults to "Settings"
        /// </summary>
        public string ConfigDefaultFolder {
            get { return configDefaultFolder; }
            set { configDefaultFolder = Normalize(value); }
        }

        public ConfigurationChange OnChange {
            get { return onChange; }
            set { onChange = value; }
        }

        public void AddConverter(IConverter converter) {
            Type type = converter.GetConverterType();
            if (converters.ContainsKey(type)) {
                Debug.LogWarning(string.Format("Overriding existing converter {0} with {1}", converters[type], converter));
                converters[type] = converter;
            } else {
                converters.Add(type, converter);
            }
        }

        public T Get<T>(string property) {
            if (typeof(T) == typeof(string)) {
                return (T)Convert.ChangeType(GetValue(property), typeof(T));
            }

            if (!converters.ContainsKey(typeof(T))) {
                throw new Exception("Configuration converter missing for requested type " + typeof(T) + " during Get<T> operation");
            }

            return
                (T)Convert.ChangeType(
                    converters[typeof(T)].ConvertTo(GetValue(property)),
                    typeof(T)
                );
        }

        public void Set<T>(string property, T value) {
            if (typeof(T) == typeof(string)) {
                SetValue(property, (string)Convert.ChangeType(value, typeof(string)));
                return;
            }

            if (!converters.ContainsKey(typeof(T))) {
                throw new Exception("Configuration converter missing for requested type " + typeof(T) + " during Set<T> operation");
            }

            SetValue(property, converters[typeof(T)].ConvertFrom(value));
        }

        public IConverter GetConverter(Type t) {
            return converters[t];
        }

        public void Save() {
            if (HasChanged()) {
                WriteSettings();
            }
        }

        public void Reset() {
            LoadDefault();
            ReadSettings();
        }

        public void Revert() {
            foreach (string property in changed.Keys) {
                settings[property] = changed[property];
                if (onChange != null) {
                    onChange(property, changed[property]);
                }
            }

            changed.Clear();
        }

        public void Revert(string property) {
            if (changed.ContainsKey(property)) {
                SetValue(property, changed[property]);
            }
        }

        public bool HasChanged() {
            return changed.Count > 0;
        }

        public bool HasChanged(string property) {
            return changed.ContainsKey(property);
        }

        public bool ContainsValue(string value) {
            return settings.ContainsValue(value);
        }

        public bool ContainsKey(string property) {
            return settings.ContainsKey(property);
        }

        public string KeyForValue(string value) {
            return settings.First(v => v.Value.Equals(value)).Key;
        }

        public void KeepChanged(string property) {
            if (changed.ContainsKey(property)) {
                changed.Remove(property);
            }
        }

        public void Clear(string property) {
            if (settings.ContainsKey(property)) {
                settings[property] = "";
            }
        }

        private string ConfigPath {
			get { return string.Format("{0}/{1}", configDefaultFolder, configName); }
        }
        private string BuildDestination {
            get { return string.Format("{0}/{1}.{2}", configFolder, configName, configExtention); }
        }

		private string Normalize(string path) {
			return path.Trim(slash);
		}

        private void SetValue(string property, string value) {
            if (settings.ContainsKey(property)) {
                if (settings[property] != value) {
                    if (changed.ContainsKey(property)) {
                        if (changed[property].Equals(value)) {
                            changed.Remove(property);
                        }
                    } else {
                        changed.Add(property, settings[property]);
                    }

                    settings[property] = value;

                    if (onChange != null) {
                        OnChange(property, value);
                    }
                }
            }
        }

        private string GetValue(string property) {
            if (settings.ContainsKey(property)) {
                return settings[property];
            }

            return null;
        }

        private void ReadSettings() {
            settings.Clear();
            changed.Clear();

            string line = "";
            string[] kv;

            using (FileStream stream = File.Open(BuildDestination, FileMode.Open, FileAccess.Read)) {
                using (TextReader reader = new StreamReader(stream)) {
                    while ((line = reader.ReadLine()) != null) {
                        kv = line.Split('=');
                        if (kv.Length == 2) {
                            settings.Add(kv[0].Trim(), kv[1].Trim());
                        }
                    }
                }
            }
        }

        private void WriteSettings() {
            IDictionaryEnumerator enumerator = settings.GetEnumerator();

            using (FileStream stream = File.Open(BuildDestination, FileMode.Create, FileAccess.Write)) {
                string line = "";

                using (TextWriter writer = new StreamWriter(stream)) {
                    while (enumerator.MoveNext()) {
                        line = string.Format("{0}\t=\t{1}", enumerator.Key, enumerator.Value);
                        writer.WriteLine(line);
                    }
                }
            }

            changed.Clear();
        }

        private void LoadDefault() {
            TextAsset configFile = Resources.Load<TextAsset>(ConfigPath);

			if (configFile == null) {
				Debug.LogWarning(string.Format("Default config file \"{0}.txt\" not found on specified location {1}", configName, ConfigPath));
				return;
			}

            using (FileStream stream = File.Open(BuildDestination, FileMode.Create, FileAccess.Write)) {
                using (TextWriter writer = new StreamWriter(stream)) {
                    writer.Write(configFile.text);
                }
            }
        }

        private bool ConfigExists() {
            return File.Exists(BuildDestination);
        }
    }
}