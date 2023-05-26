using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using weather.Context.ContextManager;

namespace weather.Models;

public class WeatherDescriptorJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var mainToken = JObject.Load(reader);

        var humidity = mainToken.SelectToken("current_condition[0].humidity")?.Value<int>() ??
                       throw new JsonSerializationException("No humidity property");
        var temperature = mainToken.SelectToken("current_condition[0].temp_C")?.Value<int>() ??
                          throw new JsonSerializationException("No temperature property");
        var weatherState = mainToken.SelectToken("current_condition[0].weatherDesc[0].value")?.Value<string>() ??
                           throw new JsonSerializationException("No weatherPreview state property");
        var feelTemperuate = mainToken.SelectToken("current_condition[0].FeelsLikeC")?.Value<int>() ??
                             throw new JsonSerializationException("No FeelsLikeC property");
        var pressure = mainToken.SelectToken("current_condition[0].pressureInches")?.Value<int>() ??
                       throw new JsonSerializationException("No pressureInches property");
        var visibilityMiles = mainToken.SelectToken("current_condition[0].visibilityMiles")?.Value<int>() ??
                              throw new JsonSerializationException("No visibilityMiles property");
        var wind = mainToken.SelectToken("current_condition[0].windspeedKmph")?.Value<int>() ??
                   throw new JsonSerializationException("No windspeedKmph property");
        var uvIndex = mainToken.SelectToken("current_condition[0].uvIndex")?.Value<int>() ??
                      throw new JsonSerializationException("No uvIndex property");

        ContextManager.Context.Logger.Info("Recieve weatherPreview state: " + weatherState);

        return new WeatherDescriptor
        {
            Humidity = humidity,
            Temperature = temperature,
            FeelTemperature = feelTemperuate,
            Pressure = pressure,
            Visibility = visibilityMiles,
            Wind = wind,
            UvIndex = uvIndex,
            WeatherStateAlias = weatherState
        };
    }

    public override bool CanConvert(Type objectType)
        => objectType == typeof(WeatherDescriptor);
}

[JsonConverter(typeof(WeatherDescriptorJsonConverter))]
public class WeatherDescriptor
{
    public required int Temperature { get; init; }
    public required int FeelTemperature { get; init; }
    public required int Humidity { get; init; }
    public required int Pressure { get; init; }
    public required int Visibility { get; init; }
    public required int Wind { get; init; }
    public required int UvIndex { get; init; }
    public required string WeatherStateAlias { get; init; }
}