using System.Globalization;

namespace weather.Models;

public class City
{
    public string Name { get; init; }
    public string Capital { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    // ReSharper disable once MemberCanBePrivate.Global
    public City(string name, string capital, double latitude, double longitude)
    {
        Name = name;
        Capital = capital;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static City Parse(string line)
    {
        const int nameKey = 0;
        const int latKey = 2;
        const int longKey = 3;
        const int capitalKey = 4;

        var parts = line.Split(',');
        return new(parts[nameKey], parts[capitalKey], double.Parse(parts[latKey], CultureInfo.InvariantCulture),
            double.Parse(parts[longKey], CultureInfo.InvariantCulture));
    }
}