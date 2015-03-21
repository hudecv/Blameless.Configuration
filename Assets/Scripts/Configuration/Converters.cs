namespace Blameless.Configuration {
    using UnityEngine;
    using System.Collections;

    public class StringConverter : Converter<string> {
        protected override string DoConvertFrom(string input) {
            return input.Trim();
        }

        protected override string DoConvertTo(string input) {
            return input.Trim();
        }
    }

    public class IntegerConverter : Converter<int> {
        protected override string DoConvertFrom(int input) {
            return input.ToString();
        }

        protected override int DoConvertTo(string input) {
            return int.Parse(input);
        }
    }

    public class FloatConverter : Converter<float> {
        protected override string DoConvertFrom(float input) {
            return input.ToString();
        }

        protected override float DoConvertTo(string input) {
            return float.Parse(input);
        }
    }

    public class BooleanConverter : Converter<bool> {
        protected override string DoConvertFrom(bool input) {
            return input ? "1" : "0";
        }

        protected override bool DoConvertTo(string input) {
            return input.Equals("1") ? true : false;
        }
    }
}