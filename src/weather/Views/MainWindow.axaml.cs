using System.Reactive.Disposables;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using ReactiveUI;
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
                    Image.Source = new Bitmap(Environment.CurrentDirectory + weatherState.ImagePath);
                    Image.InvalidateVisual();
                })
                .DisposeWith(disposables));
    }
}