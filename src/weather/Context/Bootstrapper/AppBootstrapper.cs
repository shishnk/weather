using Splat;
using weather.Services;
using weather.ViewModels;

namespace weather.Context.Bootstrapper;

public class AppBootstrapper
{
    public AppBootstrapper()
    {
        Locator.CurrentMutable.RegisterConstant(new WeatherService(), typeof(IWeatherService));
        Locator.CurrentMutable.RegisterConstant(new ImageService(), typeof(IImageService));
        Locator.CurrentMutable.RegisterConstant(new SearchViewModel());
    }
}