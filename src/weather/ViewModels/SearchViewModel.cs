using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using weather.Context.ContextManager;
using weather.Models;
using weather.Services;

namespace weather.ViewModels;

public class SearchViewModel : ReactiveObject
{
    [Reactive] public string? SearchBar { get; set; }
    [Reactive] public City? SelectedCity { get; set; }
    [ObservableAsProperty] public WeatherDescriptor? WeatherDescriptor { get; }
    [Reactive] public IWeatherState? WeatherState { get; private set; }
    public ICityService CityService { get; }
    public IWeatherService WeatherService { get; }
    public ObservableCollection<City> FoundCities { get; }
    public ReactiveCommand<string, Unit> Search { get; }
    public ReactiveCommand<City, WeatherDescriptor> UpdateWeather { get; }

    public SearchViewModel(IWeatherService? weatherService = null, ICityService? cityService = null)
    {
        WeatherService = weatherService ?? Locator.Current.GetService<IWeatherService>()!;
        CityService = cityService ?? Locator.Current.GetService<ICityService>()!;
        FoundCities = new();

        Search = ReactiveCommand.CreateFromTask<string, Unit>(async name =>
                await Task.Run(async () =>
                {
                    FoundCities.Clear();

                    await foreach (var city in CityService.SearchCity(name)) FoundCities.Add(city);

                    if (!FoundCities.Any())
                    {
                        ContextManager.Context.Logger.Debug(
                            $"Command search city is executed. City by name \"{name}\" not found.");
                    }

                    ContextManager.Context.Logger.Debug(
                        $"Command search city is executed. Cities found: {FoundCities.Count}");

                    return Unit.Default;
                }),
            canExecute: this.WhenAnyValue(t => t.SearchBar)
                .Select(result => !string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result)));
        Search.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));

        UpdateWeather = ReactiveCommand.CreateFromTask<City, WeatherDescriptor>(
            async city => await Task.Run(async () => await WeatherService.UpdateWeather(city)),
            canExecute: this.WhenAnyValue(t => t.SelectedCity).Select(city => city is not null));
        UpdateWeather.ToPropertyEx(this, t => t.WeatherDescriptor);
        UpdateWeather.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));

        this.WhenAnyValue(t => t.SelectedCity).WhereNotNull()
            .Subscribe(city => UpdateWeather.Execute(city).Subscribe());
        this.WhenAnyValue(t => t.WeatherDescriptor).WhereNotNull()
            .Subscribe(desc => WeatherState = WeatherStateFactory.GetWeatherStateByAlias(desc.WeatherStateAlias));
    }
}