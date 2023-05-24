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
            ViewModel!.UpdateImage.Subscribe(path =>
                {
                    if (string.IsNullOrEmpty(path)) return;
                    Image.Source = new Bitmap(Environment.CurrentDirectory + path);
                    Image.InvalidateVisual();
                })
                .DisposeWith(disposables));
    }
}