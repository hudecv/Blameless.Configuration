namespace Blameless.Configuration {
    using UnityEngine;
    using System.Collections;
    using System;

    public abstract class Converter<T> : IConverter {
        public string ConvertFrom(object input) {
            return DoConvertFrom((T)input);
        }

        public object ConvertTo(string input) {
            return DoConvertTo(input);
        }

        public Type GetConverterType() {
            return typeof(T);
        }

        protected abstract string DoConvertFrom(T input);

        protected abstract T DoConvertTo(string input);
    }
}