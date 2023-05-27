using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using ReactiveUI;
using weather.Context.ContextManager;
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
            });
        });
        MapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        HideElements();
    }

    private void HideElements()
    {
        ProgressRing.IsVisible = false;
        InfoBorder.Child.IsVisible = false;
    }
}