using System.Composition;

// ReSharper disable UnusedType.Global

namespace weather.Models;

public interface IWeatherState
{
    public string ImagePath { get; }
    public WeatherState State { get; }
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Cloudy)]
public class Cloudy : IWeatherState
{
    public string ImagePath => @"\Assets\images\cloudy.png";
    public WeatherState State => WeatherState.Cloudy;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Foggy)]
public class Foggy : IWeatherState
{
    public string ImagePath => @"\Assets\images\foggy.png";
    public WeatherState State => WeatherState.Foggy;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Overcast)]
public class Overcast : IWeatherState
{
    public string ImagePath => @"\Assets\images\overcast.png";
    public WeatherState State => WeatherState.Overcast;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.PartlyCloudy)]
public class PartlyCloudy : IWeatherState
{
    public string ImagePath => @"\Assets\images\partlyCloudy.png";
    public WeatherState State => WeatherState.PartlyCloudy;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Raining)]
public class Raining : IWeatherState
{
    public string ImagePath => @"\Assets\images\raining.png";
    public WeatherState State => WeatherState.Raining;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Snowy)]
public class Snowy : IWeatherState
{
    public string ImagePath => @"\Assets\images\snowy.png";
    public WeatherState State => WeatherState.Snowy;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Sunshine)]
public class Sunshine : IWeatherState
{
    public string ImagePath => @"\Assets\weather-states\sunshine.png";
    public WeatherState State => WeatherState.Sunshine;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Thunder)]
public class Thunder : IWeatherState
{
    public string ImagePath => @"\Assets\weather-states\thunder.png";
    public WeatherState State => WeatherState.Thunder;
}

[Export(typeof(IWeatherState))]
[ExportMetadata(nameof(State), WeatherState.Windy)]
public class Windy : IWeatherState
{
    public string ImagePath => @"\Assets\images\windy.png";
    public WeatherState State => WeatherState.Windy;
}