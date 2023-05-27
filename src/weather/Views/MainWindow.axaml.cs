using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using MessageBox.Avalonia;
using ReactiveUI;
using weather.Context.ContextManager;
using weather.Models;
using weather.ViewModels;
using Point = NetTopologySuite.Geometries.Point;

namespace weather.Views;

public partial class MainWindow : ReactiveWindow<SearchViewModel>
{
    private readonly GenericCollectionLayer<List<IFeature>> _layer;

    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(t => t.ViewModel!.WeatherState)
                .WhereNotNull()
                .Subscribe(weatherState =>
                {
                    var (backgroundPath, foregroundPath) = PathFactory.GetImagePath(weatherState.State);
                    Border.Background = new ImageBrush
                    {
                        Source = new Bitmap(Environment.CurrentDirectory + backgroundPath),
                        Stretch = Stretch.UniformToFill
                    };
                    Border.InvalidateVisual();
                    SvgImage.Path = Environment.CurrentDirectory + foregroundPath;
                    SvgImage.InvalidateVisual();
                })
                .DisposeWith(disposables);
            this.WhenAnyValue(t => t.ViewModel!.SelectedCity)
                .WhereNotNull().Subscribe(selectedCity =>
                {
                    _layer?.Features.Clear();
                    var (x, y) = SphericalMercator.FromLonLat(selectedCity.Longitude, selectedCity.Latitude);
                    _layer?.Features.Add(new GeometryFeature
                    {
                        Geometry = new Point(x, y)
                    });
                    _layer?.DataHasChanged();
                    MapControl.Map.Home = navigator =>
                        navigator.CenterOnAndZoomTo(new(x, y), navigator.Resolutions[10]); // for first render
                    MapControl.Map.Navigator.CenterOnAndZoomTo(new(x, y), MapControl.Map.Navigator.Resolutions[10]);
                }).DisposeWith(disposables);
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
            ViewModel.SaveFileDialog.RegisterHandler(async interaction =>
            {
                var filter = new FileDialogFilter
                {
                    Name = "PNG Files (.png)",
                    Extensions = new() { "png" }
                };
                var dialog = new SaveFileDialog
                {
                    Filters = new() { filter }
                };

                var messageBox1 =
                    MessageBoxManager.GetMessageBoxStandardWindow("Warning message",
                        "File path is empty");
                var messageBox2 =
                    MessageBoxManager.GetMessageBoxStandardWindow("Warning message",
                        "City not selected");

                if (ViewModel.SelectedCity is null)
                {
                    await messageBox2.Show();
                    return;
                }

                var result = await dialog.ShowAsync(this);

                if (result is null)
                {
                    await messageBox1.Show();
                    return;
                }

                interaction.SetOutput((result, ViewModel.SelectedCity));
            }).DisposeWith(disposables);
            ViewModel.UpdateWeather.ThrownExceptions.Subscribe(ex =>
            {
                NoInformationTextBlock.IsVisible = true;
                HideElements();
                ContextManager.Context.Logger.Error(ex.Message);
            }).DisposeWith(disposables);
        });

        MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        _layer = new()
        {
            Style = SymbolStyles.CreatePinStyle()
        };
        MapControl.Map.Layers.Add(_layer);
        MapControl.Map.Widgets.Add(new ScaleBarWidget(MapControl.Map)
        {
            TextAlignment = Alignment.Center,
            HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center,
            VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top
        });
        MapControl.Map.BackColor = Mapsui.Styles.Color.Black;
        HideElements();
    }

    private void HideElements()
    {
        ProgressRing.IsVisible = false;
        InfoBorder.Child.IsVisible = false;
    }
}