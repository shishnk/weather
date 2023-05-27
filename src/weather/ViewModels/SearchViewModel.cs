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
    private IReadOnlyDictionary<(string, string), City>? _citiesDictionary;

    [Reactive] public string? SearchBar { get; set; }
    [Reactive] public City? SelectedCity { get; set; }
    [ObservableAsProperty] public WeatherDescriptor? WeatherDescriptor { get; }
    [Reactive] public IWeatherState? WeatherState { get; private set; }
    public ICityService CityService { get; }
    public IWeatherService WeatherService { get; }
    public IImageService ImageService { get; }
    public ObservableCollection<City> FoundCities { get; }
    public ReactiveCommand<string, Unit> Search { get; }
    public ReactiveCommand<City, WeatherDescriptor> UpdateWeather { get; }
    public ReactiveCommand<Unit, Unit> SaveFile { get; }
    public ReactiveCommand<(string, City), Unit> SaveWeather { get; }
    public Interaction<Unit, (string, City)> SaveFileDialog { get; }

    public SearchViewModel(IWeatherService? weatherService = null, ICityService? cityService = null,
        IImageService? imageService = null)
    {
        WeatherService = weatherService ?? Locator.Current.GetService<IWeatherService>()!;
        CityService = cityService ?? Locator.Current.GetService<ICityService>()!;
        ImageService = imageService ?? Locator.Current.GetService<IImageService>()!;
        FoundCities = new();
        SaveFileDialog = new();
        _citiesDictionary ??= CityService.CreateAndFillCitiesDictionary().Result;

        Search = ReactiveCommand.CreateFromTask<string>(async name => await Task.Run(() =>
        {
            FoundCities.Clear();

            foreach (var city in _citiesDictionary)
            {
                if (city.Key.Item1.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    FoundCities.Add(city.Value);
                }
            }

            if (!FoundCities.Any())
            {
                ContextManager.Context.Logger.Debug(
                    $"Command search city is executed. City by name \"{name}\" not found.");
                return Task.CompletedTask;
            }

            ContextManager.Context.Logger.Debug(
                $"Command search city is executed. Cities found: {FoundCities.Count}");

            return Task.CompletedTask;
        }), canExecute: this.WhenAnyValue(t => t.SearchBar)
            .Select(result => !string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result)));
        Search.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));

        UpdateWeather = ReactiveCommand.CreateFromTask<City, WeatherDescriptor>(
            async city => await Task.Run(async () => await WeatherService.UpdateWeather(city)),
            canExecute: this.WhenAnyValue(t => t.SelectedCity).Select(city => city is not null));
        UpdateWeather.ToPropertyEx(this, t => t.WeatherDescriptor);

        SaveFile = ReactiveCommand.CreateFromTask(SaveFileImpl);
        SaveWeather = ReactiveCommand.CreateFromTask<(string, City)>(async parameters =>
            await Task.Run(async () => await ImageService.SaveImage(parameters.Item1, parameters.Item2)));
        SaveFile.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));
        SaveWeather.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));

        this.WhenAnyValue(t => t.SelectedCity)
            .WhereNotNull()
            .Subscribe(city => UpdateWeather.Execute(city).Subscribe());
        this.WhenAnyValue(t => t.WeatherDescriptor)
            .WhereNotNull()
            .Subscribe(desc => WeatherState = WeatherStateFactory.GetWeatherStateByAlias(desc.WeatherStateAlias));
        this.WhenAnyValue(t => t.SearchBar)
            .Throttle(TimeSpan.FromSeconds(0.4), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .InvokeCommand(Search!);
    }

    private async Task SaveFileImpl()
    {
        var (filePath, city) = await SaveFileDialog.Handle(Unit.Default);
        SaveWeather.Execute((filePath, city)).Subscribe();
    }
}