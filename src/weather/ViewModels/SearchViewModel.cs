using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using weather.Logging;
using weather.Models;
using weather.Services;

namespace weather.ViewModels;

public class SearchViewModel : ReactiveObject
{
    [Reactive] public string? SearchBar { get; set; }
    [Reactive] public City? SelectedCity { get; set; }
    public ObservableCollection<City> FoundCities { get; } = new();
    public ReactiveCommand<string, Unit> Search { get; }

    public SearchViewModel()
    {
        Search = ReactiveCommand.CreateFromTask<string, Unit>(async name =>
            await Task.Run(async () =>
            {
                FoundCities.Clear();
                await foreach (var city in Service.SearchCity(name)) FoundCities.Add(city);
                ContextManager.Context.Logger.Debug(
                    $"Command search city is executed. Cities found: {FoundCities.Count}");
                return Unit.Default;
            }));
        Search.ThrownExceptions.Subscribe(ex => ContextManager.Context.Logger.Error(ex.Message));
    }
}