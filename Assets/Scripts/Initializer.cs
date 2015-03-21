using UnityEngine;
using System.Collections;
using Blameless.Configuration;

public class Initializer : MonoBehaviour {

	void Start () {
#if UNITY_EDITOR
		Configuration.debug = true;
#endif

		Settings.Conf.AddConverter(new ColorConverter());
		Settings.Initialize();
	}
}

public class ColorConverter : Converter<Color> {

	protected override string DoConvertFrom (Color input) {
		return string.Format("({0}, {1}, {2})", input.r, input.g, input.b);
	}

	protected override Color DoConvertTo (string input) {
		string[] rgb = input.Trim(new char[]{'(', ')'}).Split(',');
		return new Color(
			float.Parse(rgb[0]),
			float.Parse(rgb[1]),
			float.Parse(rgb[2]));
	}
}