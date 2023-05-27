using weather.Models;
using weather.Services;

namespace Weather.Tests;

public class ServiceTests
{
    private const string Dir = "/Test/";
    private const string ImageFormat = ".png";

    private static readonly ICityService _service = new CityService();
    private static readonly IWeatherService _weatherService = new WeatherService();
    private static readonly IImageService _imageService = new ImageService();
    private static IReadOnlyDictionary<(string, string), City>? _citiesDictionary;

    public static IEnumerable<object[]> GetCitiesNamesAndId()
    {
        _citiesDictionary ??= _service.CreateAndFillCitiesDictionary().Result;

        return new[]
        {
            new object[] { "Novosibirsk", 1643399240 },
            new object[] { "Moscow", 1840019868 },
            new object[] { "New York", 1840034016 },
            new object[] { "Los Angeles", 1840020491 },
            new object[] { "Sydney", 1036074917 },
            new object[] { "Tokyo", 1392685764 },
            new object[] { "Caloocan City", 1608897690 },
        };
    }

    [Theory]
    [MemberData(nameof(GetCitiesNamesAndId))]
    public void FindCity(string cityName, int cityId) =>
        Assert.Contains(_citiesDictionary!.Keys,
            key => key.Item1.Equals(cityName) && key.Item2.Equals(cityId.ToString()));

    [Theory]
    [MemberData(nameof(GetCitiesNamesAndId))]
    public async void GetWeather(string cityName, string cityId) =>
        Assert.NotNull(await _weatherService.UpdateWeather(_citiesDictionary![(cityName, cityId)]));

    [Theory]
    [MemberData(nameof(GetCitiesNamesAndId))]
    public async void SaveWeather(string cityName, string cityId)
    {
        var path = Environment.CurrentDirectory + Dir;
        var directory = Directory.CreateDirectory(path);
        var fullPath = directory.FullName + cityId;
        await _imageService.SaveImage(fullPath, _citiesDictionary![(cityName, cityId)]);

        Assert.True(File.Exists(fullPath + ImageFormat));
        directory.Delete(true);
    }
}