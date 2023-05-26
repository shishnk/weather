using Newtonsoft.Json;
using weather.Context.ContextManager;
using weather.Models;

namespace weather.Services;

public interface IService
{
}

public interface ICityService : IService
{
    public IAsyncEnumerable<City> SearchCity(string cityName);
}

public interface IWeatherService : IService
{
    public Task<WeatherDescriptor> UpdateWeather(City city);
}

public class CityService : ICityService
{
    private const string FilePath = "Assets/csv/worldcities.csv";

    public async IAsyncEnumerable<City> SearchCity(string cityName)
    {
        if (!File.Exists(FilePath))
        {
            throw new FileNotFoundException($"File with path \"{FilePath}\" doesnt exist");
        }

        using var sr = new StreamReader(FilePath);

        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync();

            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), "Error with reading line from file");
            }

            if (line.Split(',')[0].Contains(cityName, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return City.Parse(line);
            }
        }
    }
}

public class WeatherService : IWeatherService, IDisposable
{
    private const string Url = @"https://wttr.in/";
    private readonly HttpClient _client = new();

    public async Task<WeatherDescriptor> UpdateWeather(City city)
    {
        var url = Path.Combine(Url, city.Name + "?format=j1");

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        ContextManager.Context.Logger.Info("Command update image is executed");
        return JsonConvert.DeserializeObject<WeatherDescriptor>(json) ??
               throw new JsonSerializationException("Bad deserialization weatherPreview description");
    }

    public void Dispose() => _client.Dispose();
}