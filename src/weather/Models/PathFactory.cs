namespace weather.Models;

public static class PathFactory
{
    public static string GetImagePath(WeatherState state) =>
        state switch
        {
            WeatherState.Cloudy => @"/Assets/weather-states/cloudy.svg",
            WeatherState.Foggy => @"/Assets/weather-states/foggy.svg",
            WeatherState.Overcast => @"/Assets/weather-states/overcast.svg",
            WeatherState.Raining => @"/Assets/weather-states/raining.svg",
            WeatherState.Snowy => @"/Assets/weather-states/snowy.svg",
            WeatherState.Sunshine => @"/Assets/weather-states/sunshine.svg",
            WeatherState.Thunder => @"/Assets\weather-states/thunder.svg",
            WeatherState.PartlyCloudy => @"/Assets/weather-states/partlyCloudy.svg",
            _ => throw new ArgumentOutOfRangeException(nameof(state), "Unknown weather state")
        };
}