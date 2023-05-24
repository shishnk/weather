using weather.Context.ContextManager;
using weather.Models;

namespace weather.Services;

public interface IService
{
}

public interface IWeatherService : IService
{
    public IAsyncEnumerable<City> SearchCity(string cityName);
}

public interface IImageService : IService
{
    public Task<string> SaveImage(City city);
}

public class WeatherService : IWeatherService
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

        ContextManager.Context.Logger.Debug($"Command search city is executed. City by name \"{cityName}\" not found.");
    }
}

public class ImageService : IImageService, IDisposable
{
    private const string SavePath = @"\Assets\images\city.png";
    private const string Url = @"https://wttr.in/";
    private const string ImageFormat = ".png";

    private readonly HttpClient _client = new();

    public async Task<string> SaveImage(City city)
    {
        var imageUrl = Path.Combine(Url, city.Name + ImageFormat);

        var response = await _client.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();

        await using var imageStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream =
            File.Create(Environment.CurrentDirectory + SavePath);
        await imageStream.CopyToAsync(fileStream);

        ContextManager.Context.Logger.Info("Image saved");

        return SavePath;

        // var url = $"https://wttr.in/{Uri.EscapeDataString("Novosibirsk")}?format=%C+%t+%w";
        //
        // using var client = new HttpClient();
        // var response = await client.GetAsync(url);
        // response.EnsureSuccessStatusCode();
        //
        // var weatherData = await response.Content.ReadAsStringAsync();
        // return weatherData;
    }

    public void Dispose() => _client.Dispose();
}