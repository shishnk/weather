using System.Reactive.Disposables;
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
            this.WhenAnyValue(t => t.ViewModel!.WeatherState).WhereNotNull().Subscribe(weatherState =>
                {
                    // StateImage.Source =
                    // new Bitmap(Environment.CurrentDirectory + PathFactory.GetImagePath(weatherState.State));
                    // StateImage.InvalidateVisual();
                    SvgImage.Path = Environment.CurrentDirectory + PathFactory.GetImagePath(weatherState.State);
                    SvgImage.InvalidateVisual();
                })
                .DisposeWith(disposables));
    }
}