using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
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
    private Bitmap? _bitmap;
    private readonly GenericCollectionLayer<List<IFeature>> _layer;

    public ReactiveCommand<Unit, Unit> ShowMultiDayWeather { get; }
    public ReactiveCommand<Unit, Unit> ShowOnlyOneDayWeather { get; }

    public MainWindow()
    {
        ShowMultiDayWeather = ReactiveCommand.CreateFromTask(async () =>
        {
            HideContextMenu(MultiDayWeatherContextMenu);
            _bitmap ??= await ViewModel!.SaveWeather.Execute((null, ViewModel!.SelectedCity!));
            MultiWeatherImage.Source = _bitmap;
            MultiWeatherImage.IsVisible = true;
            MultiWeatherImage.InvalidateVisual();
        });
        ShowOnlyOneDayWeather = ReactiveCommand.Create(() =>
        {
            HideContextMenu(OnlyOneDayWeatherContextMenu);
            InfoBorder.Child!.IsVisible = true;
            MultiWeatherImage.IsVisible = false;
        });
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(t => t.ViewModel!.WeatherState).WhereNotNull().Subscribe(weatherState =>
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
            this.WhenAnyValue(t => t.ViewModel!.SelectedCity).WhereNotNull().Subscribe(selectedCity =>
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
                DisposeBitmap();
            }).DisposeWith(disposables);
            ViewModel!.UpdateWeather.IsExecuting.Subscribe(IsExecutingLogic).DisposeWith(disposables);
            ShowMultiDayWeather.IsExecuting.Subscribe(IsExecutingLogic).DisposeWith(disposables);
            ViewModel.UpdateWeather.Subscribe(_ => InfoBorder.Child!.IsVisible = true).DisposeWith(disposables);
            ViewModel.SaveFileDialog.RegisterHandler(async interaction =>
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandardWindow("Warning message", "City not selected");

                if (ViewModel.SelectedCity is null)
                {
                    await messageBox.ShowDialog(this);
                    return;
                }

                var storageFile = await StorageProvider.SaveFilePickerAsync(new()
                {
                    DefaultExtension = ".png",
                    ShowOverwritePrompt = true,
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType(null)
                        {
                            Patterns = new[] { "*.png" }
                        }
                    }
                });

                // if (_bitmap is not null) TODO -> save png if bitmap is exist
                // {
                // }
                
                interaction.SetOutput((storageFile!.Path.AbsolutePath, ViewModel.SelectedCity));
            }).DisposeWith(disposables);
            ViewModel.UpdateWeather.ThrownExceptions.Subscribe(HandleExceptions).DisposeWith(disposables);
            ShowMultiDayWeather.ThrownExceptions.Subscribe(HandleExceptions).DisposeWith(disposables);
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
        InfoBorder.Child!.IsVisible = false;
    }

    private void IsExecutingLogic(bool isExecuting)
    {
        if (isExecuting)
        {
            NoInformationTextBlock.IsVisible = !isExecuting;
            InfoBorder.Child!.IsVisible = !isExecuting;
            ProgressRing.IsVisible = isExecuting;
        }
        else
        {
            ProgressRing.IsVisible = isExecuting;
        }
    }

    private void DisposeBitmap()
    {
        MultiWeatherImage.Source = null;
        _bitmap = null;
    }

    private void HandleExceptions(Exception ex)
    {
        NoInformationTextBlock.IsVisible = true;
        HideElements();
        ContextManager.Context.Logger.Error(ex.Message);
    }

    private static void HideContextMenu(ContextMenu contextMenu) => contextMenu.Close();
}