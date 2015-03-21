namespace Blameless.Configuration {
    using UnityEngine;
    using System.Collections;
    using System;

    public interface IConverter {
        string ConvertFrom(object input);

        object ConvertTo(string input);

        Type GetConverterType();
    }
}