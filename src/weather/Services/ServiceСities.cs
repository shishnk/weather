using System;
using System.Collections.Generic;
using System.IO;
using weather.Logging;
using weather.Models;

namespace weather.Services;

public static class Service
{
    private const string FilePath = "Assets/csv/worldcities.csv";

    public static async IAsyncEnumerable<City> SearchCity(string cityName)
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