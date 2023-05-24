using System.Composition;
using System.Composition.Hosting;
using weather.Context.ContextManager;

namespace weather.Models;

public enum WeatherState
{
    Cloudy,
    Foggy,
    Overcast,
    PartlyCloudy,
    Raining,
    Snowy,
    Sunshine,
    Thunder,
    Windy,
    None
}

// ReSharper disable once ClassNeverInstantiated.Global
public class WeatherStateMetadata
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public WeatherState State { get; set; }
}

public static class WeatherStateFactory
{
    private class ImportInfo
    {
        [ImportMany]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<Lazy<IWeatherState, WeatherStateMetadata>> WeatherStates { get; set; } =
            Enumerable.Empty<Lazy<IWeatherState, WeatherStateMetadata>>();
    }

    private static readonly ImportInfo _info = new();

    static WeatherStateFactory()
    {
        var assemblies = new[] { typeof(IWeatherState).Assembly };
        var configuration = new ContainerConfiguration();
        try
        {
            configuration = configuration.WithAssemblies(assemblies);
        }
        catch (Exception)
        {
            // ignored           
        }

        var container = configuration.CreateContainer();
        container.SatisfyImports(_info);
    }

    public static IWeatherState? GetWeatherState(WeatherState state)
    {
        var weatherState =
            _info.WeatherStates.FirstOrDefault(@object => @object.Metadata.State == state);

        if (weatherState is not null) return weatherState.Value;

        ContextManager.Context.Logger.Error($"Weather state \"{state}\" doesnt exist");
        return null;
    }
}