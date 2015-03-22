# Configuration System for Unity Games

This asset adds an easy to use configuration system for Unity games that require keeping player settings in external files.

It is solely written in C#, but can naturally be used by UnityScript or Boo scripts too.

[Download from Github](https://github.com/hudecv/Blameless.Configuration)

Created by [Vaclav Hudec](http://xoxco.com) for [Blameless](http://blamelessgame.com)

## Why use Blameless Configuration

The Blameless Configuration system is mainly targeted to users who want a quick and easy solution to add Settings features in their game. In this package I include a configuration utility which can be incorporated in any existing system, plus on top I provide two practical examples of usage for [Settings](https://github.com/hudecv/Blameless.Configuration/#settings-example) and custom [Input manager](https://github.com/hudecv/Blameless.Configuration/#binput-example) that [Blameless game](http://blamelessgame.com) is using.

## Base Configuration Class

The `Configuration` class acts as an intermediary that can access data from a configuration file (eg. `player.cfg`) and represent them as key-value pairs.

#### Default configuration templates

In case the configuration file does not exist in the specified location, one can be automatically created from its default template from the Resources folder. This allows for functionality such as resetting to the default settings from the in-game settings menu. In the **production build** the default configuration templates are concealed and cannot be directly tampered with by the player in any way.

To use a default template, place a file `player.txt` in the `Resources/Settings/` folder (by default). This will be copied as the configuraton file.  
**Note:** The default template **must** use the `.txt` extention, regardless of the actual configuraton file extention `.cfg`.

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

## Settings example

To be documented

## BInput example

To be documented

## Advanced

The configuration object exposes some methods and properties that allow for extended flexibility. The user can add extra functionality by defining converters ~~or custom configuration readers/writers.~~

#### Converters

The configuration object always stores the values as `string`. However, they can also be automatically converted into any target type.

```c#
string mouseSensitivity = config.Get<string>("MOUSE_SENSITIVITY"); // "4.5"
float mouseSensitivity = config.Get<float>("MOUSE_SENSITIVITY"); // 4.5f
```

By default, the configuration is capable of reading `string`, `float`, `int` and `bool`. **Note:** Booleans assume "1" for `true` and "0" for `false`.

To handle another (custom) type, you can add a `Converter` to the configuration object. Each converter mush either implement `IConverter` interface or inherit from `Converter<T>` class. Here is an example of a simplified `Color` converter:

```c#
public class ColorConverter : Converter<Color> {

    // return string for RGB color: eq. Color.green -> (0, 1, 0)
    protected override string DoConvertFrom (Color input) {
        return string.Format("({0}, {1}, {2})", input.r, input.g, input.b);
    }

    // return Color instance eq. (0, 1, 0) -> Color.green
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

###### Note:
C# can automatically figure out what generic type is being passed to the `Set<T> method, so these two lines are equivalent and can be applied to all supported types:

```c#
// set Color through the converter
config.Set<Color>("THEME_COLOR", Color.red);
// set Color through the converter
config.Set("THEME_COLOR", Color.red);
```
