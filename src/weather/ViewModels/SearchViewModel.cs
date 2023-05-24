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
    private bool _isImageLoaded;
    private readonly ObservableAsPropertyHelper<string> _imagePath;

    [Reactive] public string? SearchBar { get; set; }
    [Reactive] public City? SelectedCity { get; set; }
    public IWeatherService WeatherService { get; }
    public IImageService ImageService { get; }

    public string ImagePath
    {
        get
        {
            if (!_isImageLoaded) ContextManager.Context.Logger.Error("Image is not loaded");
            return _imagePath.Value;
        }
    }

    public ObservableCollection<City> FoundCities { get; }
    public ReactiveCommand<string, Unit> Search { get; }
    public ReactiveCommand<City, string> UpdateImage { get; }

    public SearchViewModel(IWeatherService? weatherService = null, IImageService? imageService = null)
    {
        WeatherService = weatherService ?? Locator.Current.GetService<IWeatherService>()!;
        ImageService = imageService ?? Locator.Current.GetService<IImageService>()!;
        FoundCities = new();

        Search = ReactiveCommand.CreateFromTask<string, Unit>(async name =>
                await Task.Run(async () =>
                {
                    FoundCities.Clear();
                    await foreach (var city in WeatherService.SearchCity(name)) FoundCities.Add(city);
                    ContextManager.Context.Logger.Debug(
                        $"Command search city is executed. Cities found: {FoundCities.Count}");
                    return Unit.Default;
                }),
            canExecute: this.WhenAnyValue(t => t.SearchBar)
                .Select(result => !string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result)));
        Search.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));

        UpdateImage = ReactiveCommand.CreateFromTask<City, string>(
            async city => await Task.Run(async () => await ImageService.SaveImage(city)),
            canExecute: this.WhenAnyValue(t => t.SelectedCity).Select(city => city is not null));
        UpdateImage.Subscribe(_ => _isImageLoaded = true);
        UpdateImage.ToProperty(this, t => t.ImagePath, out _imagePath, () => string.Empty);
        UpdateImage.ThrownExceptions.Subscribe(ex =>
        {
            _isImageLoaded = false;
            ContextManager.Context.Logger.Error(ex.Message);
        });

        this.WhenAnyValue(t => t.SelectedCity).WhereNotNull().Subscribe(city =>
        {
            _isImageLoaded = false;
            UpdateImage.Execute(city).Subscribe();
        });
    }
}