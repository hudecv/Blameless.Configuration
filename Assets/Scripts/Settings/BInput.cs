using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Blameless.Configuration;

public static class BInput {

    private static string[] invalid = { "Mouse", "Joystick", "F" };
    private static KeyCode[] validKeyCodes;

    private static Configuration conf;

    private static Axis vAxis;
    private static Axis hAxis;

    public static void Initialize() {
        conf = new Configuration("mappings");
        conf.AddConverter(new KeyCodeConverter());
        conf.Initialize();

        vAxis = new Axis();
        hAxis = new Axis();

        validKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        validKeyCodes = validKeyCodes.Where(k => !invalid.Any(inv => k.ToString().StartsWith(inv))).ToArray<KeyCode>();

        Debug.Log("BInput has been initialized");
    }

    public static bool GetActionUp(string action) {
        return Input.GetKeyUp(GetKeyCode(action));
    }

    public static bool GetActionDown(string action) {
        return Input.GetKeyDown(GetKeyCode(action));
    }

    public static bool GetAction(string action) {
        return Input.GetKey(GetKeyCode(action));
    }

    public static float GetAxis(string axis) {
        if (axis.Equals("Vertical")) {
            return vAxis.GetAmount(GetAction("FORWARD"), GetAction("BACKWARD"));
        } else if (axis.Equals("Horizontal")) {
            return hAxis.GetAmount(GetAction("RIGHT"), GetAction("LEFT"));
        }

        return 0;
    }

    public static float GetHorizontalAxis() {
        if (GetAction("RIGHT")) {
            return 1f;
        } else if (GetAction("LEFT")) {
            return -1f;
        }

        return 0;
    }

    public static void SetValue(string property, string value) {
        if (conf.Get<string>(property).Equals(value)) {
            conf.KeepChanged(property);
        }

        conf.Set(property, value);
    }

    public static string GetValue(string property) {
        return conf.Get<string>(property);
    }

    public static void ResetToDefaults() {
        conf.Reset();
    }

    public static bool HasChanged() {
        return conf.HasChanged();
    }

    public static string[] InvalidInputGroup {
        get { return invalid; }
        set { invalid = value; }
    }

    public static KeyCode[] ValidKeyCodes {
        get { return validKeyCodes; }
    }

    public static bool Initialized {
        get { return conf != null; }
    }

    public static Configuration Conf {
        get { return conf; }
    }

    private static KeyCode GetKeyCode(string action) {
        if (conf.ContainsKey(action)) {
            try {
                return conf.Get<KeyCode>(action);
            } catch (ArgumentException) {
                Debug.LogWarning(string.Format("BInput action \"{0}\" is not mapped to any key.", action));
                conf.Set(action, KeyCode.None);
            }
        }

        return KeyCode.None;
    }

    class Axis {
        private float amount = 0;
        private int dir;
        private float acc = 0;
        private float multiplier = 2f;

        public float GetAmount(bool positive, bool negative) {
            if (positive) {
                if (dir < 1) {
                    amount = 0;
                }
                dir = 1;
                acc = 1;
            } else if (negative) {
                if (dir > 1) {
                    amount = 0;
                }
                dir = -1;
                acc = 1;
            } else {
                acc = -1;
            }

            amount += (float)acc * Time.deltaTime * multiplier;
            amount = Mathf.Max(Mathf.Min(amount, 1f), 0f);

            return amount * (float)dir;
        }
    }
}

public class KeyCodeConverter : Converter<KeyCode> {
    protected override string DoConvertFrom(KeyCode input) {
        return input.ToString();
    }

    protected override KeyCode DoConvertTo(string input) {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), input);
    }
}