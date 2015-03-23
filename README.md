# Configuration System for Unity Games

This asset adds an easy to use configuration system for Unity games that require keeping player settings in external files.

It is solely written in C#, but can naturally be used by UnityScript or Boo scripts too.

[Download from Github](https://github.com/hudecv/Blameless.Configuration)

Created by Vaclav Hudec for [Blameless](http://blamelessgame.com)

## Rationale

The Blameless Configuration system is targeted to users who want a quick and easy solution to add Settings features in their game. It comes from my own requirement to set the public fields on components from an external file which holds the player's configuration.

![Blameless Configuration](screenshots/Screenshots/sheme.png "Blameless Configuration")

###### The problem:

How does Unity know that some value from the configuration file should set a particular **field** on a particular **component** being attached to a particular **game object**?

In this package I give the answer to this problem. I include a configuration utility which can be incorporated into any existing system, plus on top I provide **two practical examples** of usage for [Settings](#settings-example) and custom [Input manager](#binput-example) that [Blameless](http://blamelessgame.com) game is using.

## Settings example

`Settings` class provides a system that is capable of binding the values from the configuration file to the component fields.

By default, the configuration file `player.cfg` should be placed in the `Assets` folder and resemble the following syntax:

```
MOUSE_INTENSITY = 5
MOUSE_SMOOTHNESS = 1
MOUSE_INVERTED = 0
...
```

**Note:** See below for [customizations](#customizations).

First, initialize `Settings`. In the acompanying project this is done in the [Initializer script](Assets/Scripts/Initializer.cs) attached to the root game object.

```c#
using Blameless.Configuration;

public class Initializer : MonoBehaviour {

    void Start () {
#if UNITY_EDITOR
        // always override cfg file in development environment
        Configuration.debug = true; 
#endif

        // initialize settings to load the configuration file
        Settings.Initialize();
    }
}
```

For full Settings code see [the implementation](Assets/Scripts/Settings/Settings.cs).

### Data Binding

`Settings` class implements very useful binding system to keep values in sync between your configuration file and component fields.

Attach `BindingSet` component to your desired game object and add an array of `Binding` objects. `Binding` object will require following to be set:

**Source:** The name of the key in the configuration file  
**Component:** The `Component` that contains the field/property you'll be  binding to  
**Binding type:** The `Type` of the field/property to bind to  
**Field/Property:** The field/property from the component to bind to

Binding objects come with a custom property drawer, so setting them up is as easy as selecting from dropdowns:

![Setup a binding object](screenshots/Screenshots/settingsSystem_1.jpg "Setup a binding object")
![Setup a binding object](screenshots/Screenshots/settingsSystem_2.jpg "Setup a binding object")

To take full advantage of the binding for an in-game settings menu:

```c#
// set cfg for mouse intensity
Settings.Set("MOUSE_INTENSITY", 10);

// save back into cfg file
Settings.Save();
```

Setting the `"MOUSE_INTENSITY"` key like this will trigger the binding synchronization. The corresponding field (let's say `public float intensity` field on the `MouseLook` component) will then be auto-set to 10.

### Customizations

To customize the behaviour, access the configuration object through the `Conf` property:

```c#
Configuration config = Settings.Conf;
config.ConfigName = "settings"
config.ConfigExtention = "ini"
config.ConfigDefaultFolder = "Settings/Default"

// need to re-initialize to load the correct file
Settings.Initialize();
```

For more customizations and additional info on the configuration object, consult the [advanced section](#base-configuration-class) below.

## BInput example

`BInput` class provides greatly simplified input manager that reads the key mappings from a `mappings.cfg` file. Pressed key is checked through the built-in `Input` class.

This can serve as a reference for your project. For full commented code, check [the implementation](Assets/Scripts/Settings/BInput.cs) in the repo.

```c#
using Blameless.Configuration;

public static class BInput {

    private static Configuration conf;

    public static void Initialize() {
        conf = new Configuration("mappings");
        conf.AddConverter(new KeyCodeConverter());
        conf.Initialize();

        // ...

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

    // ...
}

public class KeyCodeConverter : Converter<KeyCode> {

    // eg. KeyCode.W -> "W"
    protected override string DoConvertFrom(KeyCode input) {
        return input.ToString();
    }

    // eg. "W" -> KeyCode.W
    protected override KeyCode DoConvertTo(string input) {
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), input);
    }
}
```

Sample from `mappings.cgf`:  
```
FORWARD = W
BACKWARD = S
LEFT = A
RIGHT = D
...
```

## Advanced

Both the above examples use a `Configuration` object under the hood. It exposes some methods and properties that allow for extended flexibility. The user can add extra functionality by defining [converters](#converters) ~~or custom configuration readers/writers.~~

### Base Configuration Class

The `Configuration` class acts as an intermediary that can access data from a configuration file (eg. `player.cfg`) and represent them as key-value pairs.

#### Default configuration templates

If the configuration file does not exist, one can be automatically created from its default template. This allows for functionality such as resetting to the default settings from the in-game settings menu. In the **production build** the default configuration templates are concealed from the player.

To use a default template, place a file `player.txt` in the `Resources/Settings/` folder (by default). This will be copied as the configuraton file the first time the game runs.   
**Note:** The default template **must** use the `.txt` extention, regardless of the actual configuraton file extention (eg. `.cfg`, `.ini`, etc).

#### Configuration object

To start using the system, create the configuration object

```c#
Configuration config = new Configuration("player");
```
    
Initialize it to load the data from the corresponding file `player.cfg`. By default this file should be structured into a list of ***"key = value"*** pairs. These will be afterwards accessable through the configuration object.

```c#
config.Initialize();
```

To customize the file paths, use the following properties (given are default):

```c#
config.ConfigExtention = "cfg"
// path in filesystem where the actual config file should reside
config.ConfigFolder = Application.dataPath
// path relative to Resources folder where the default template resides
config.ConfigDefaultFolder = "Settings"
```
    
To access a value through the configuration object, use the accessor methods:

```c#
// get some data from config
string playerName = config.Get<string>("PLAYER_NAME");
float mouseSensitivity = config.Get<float>("MOUSE_SENSITIVITY");

mouseSensitivity = 4.5;

// set some updated data
config.Set<float>("MOUSE_SENSITIVITY", mouseSensitivity);
```
    
To ensure the current data persist in the configuration file, save it:

```c#
config.Save();
```

The file `player.cfg` would then look like

```
PLAYER_NAME = DefaultPlayer
MOUSE_SENSITIVITY = 4.5
```

### Converters

The configuration object always stores the values as `string`. However, they can also be automatically converted into any target type.

```c#
config.Set("MOUSE_INVERTED", "0");

string mouseInverted = config.Get<string>("MOUSE_INVERTED"); // "0"
float mouseInverted = config.Get<float>("MOUSE_INVERTED"); // 0f
int mouseInverted = config.Get<int>("MOUSE_INVERTED"); // 0
bool mouseInverted = config.Get<bool>("MOUSE_INVERTED"); // false
```

By default, the configuration is capable of reading `string`, `float`, `int` and `bool`. **Note:** Booleans assume "1" for `true` and "0" for `false`.

To handle another (custom) type, you can add a `Converter` to the configuration object. Each converter must either implement `IConverter` interface or inherit from `Converter<T>` class. Here is an example of a simplified `Color` converter:

```c#
public class ColorConverter : Converter<Color> {

    // return string for RGB color: eg. Color.green -> (0, 1, 0)
    protected override string DoConvertFrom (Color input) {
        return string.Format("({0}, {1}, {2})", input.r, input.g, input.b);
    }

    // return Color instance eg. (0, 1, 0) -> Color.green
    protected override Color DoConvertTo (string input) {
        string[] rgb = input.Trim(new char[]{'(', ')'}).Split(',');
        return new Color(
            float.Parse(rgb[0]),
            float.Parse(rgb[1]),
            float.Parse(rgb[2]));
    }
}
```

To use this converter with the configuration object:

```c#
// add converter
config.AddConverter(new ColorConverter());

// set Color through the converter
config.Set<Color>("THEME_COLOR", Color.red); // will write (1, 0, 0)

// get Color through the converter
Color color = config.Get<Color>("THEME_COLOR"); // will read Color(1, 0, 0)
```

**Note:** C# can automatically figure out what generic type is being passed to the `Set<T>` method, so these two lines are equivalent and can be applied to all supported types:

```c#
// set Color through the converter
config.Set<Color>("THEME_COLOR", Color.red);
// set Color through the converter
config.Set("THEME_COLOR", Color.red);
```

## Author

> I wrote this system for my upcoming title [Blameless](http://blamelessgame.com). I certainly hope it will be useful for somebody, newbies or even experienced Unity users.
> In case there are any issues, don't hesitate to contact me or use the issue tracker.
