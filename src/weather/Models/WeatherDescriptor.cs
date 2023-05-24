using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                           throw new JsonSerializationException("No weather state property");

        var resultState = WeatherState.None;

        if (Enum.GetNames(typeof(WeatherState)).Contains(weatherState))
        {
            resultState = (WeatherState)Enum.Parse(typeof(WeatherState), weatherState);
        }

        return new WeatherDescriptor
        {
            Humidity = humidity,
            Temperature = temperature,
            State = resultState
        };
    }

    public override bool CanConvert(Type objectType)
        => objectType == typeof(WeatherDescriptor);
}

[JsonConverter(typeof(WeatherDescriptorJsonConverter))]
public class WeatherDescriptor
{
    public required int Temperature { get; init; }
    public required int Humidity { get; init; }
    public required WeatherState State { get; init; }
}