namespace CodeCasa.Lights;

public partial record LightParameters
{
    /// <summary>
    /// Gets a template that generates parameters for a relaxing scene (warm color temperature, medium brightness).
    /// </summary>
    public static LightParameters Relax = new() { Brightness = 142, ColorTemp = 500 };

    /// <summary>
    /// Gets a template that generates parameters for a nightlight scene (very warm color temperature, very low brightness).
    /// </summary>
    public static LightParameters NightLight = new() { Brightness = 2, ColorTemp = 500 };

    /// <summary>
    /// Gets a template that generates parameters for a concentrate scene (cool/neutral color temperature, high brightness).
    /// </summary>
    public static LightParameters Concentrate = new() { Brightness = byte.MaxValue, ColorTemp = 233 };

    /// <summary>
    /// Gets a template that generates parameters for a bright scene (warm color temperature, maximum brightness).
    /// </summary>
    public static LightParameters Bright = new() { Brightness = byte.MaxValue, ColorTemp = 366 };

    /// <summary>
    /// Gets a template that generates parameters for a dimmed scene (warm color temperature, low brightness).
    /// </summary>
    public static LightParameters Dimmed = new() { Brightness = 76, ColorTemp = 366 };
}