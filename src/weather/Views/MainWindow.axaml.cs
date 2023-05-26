using System.Reactive.Disposables;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using ReactiveUI;
using weather.Models;
using weather.ViewModels;

namespace weather.Views;

public partial class MainWindow : ReactiveWindow<SearchViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(t => t.ViewModel!.WeatherState).WhereNotNull().Subscribe(weatherState =>
                {
                    Border.Background = new ImageBrush
                    {
                        Source = new Bitmap(Environment.CurrentDirectory +
                                            PathFactory.GetImagePath(weatherState.State)
                                                .BackgroundPath),
                        Stretch = Stretch.UniformToFill
                    };
                    Border.InvalidateVisual();
                    SvgImage.Path = Environment.CurrentDirectory +
                                    PathFactory.GetImagePath(weatherState.State).ForegroundPath;
                    SvgImage.InvalidateVisual();
                })
                .DisposeWith(disposables);
            ViewModel!.UpdateWeather.IsExecuting.Subscribe(isExecuting =>
            {
                if (isExecuting)
                {
                    NoInformationTextBlock.IsVisible = !isExecuting;
                    InfoBorder.Child.IsVisible = !isExecuting;
                    ProgressRing.IsVisible = isExecuting;
                }
                else
                {
                    ProgressRing.IsVisible = isExecuting;
                }
            }).DisposeWith(disposables);
            ViewModel.UpdateWeather.Subscribe(_ => InfoBorder.Child.IsVisible = true).DisposeWith(disposables);
        });
        MapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        ProgressRing.IsVisible = false;
        InfoBorder.Child.IsVisible = false;
    }
}