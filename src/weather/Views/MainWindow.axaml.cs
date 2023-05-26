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
                .DisposeWith(disposables));
    }
}