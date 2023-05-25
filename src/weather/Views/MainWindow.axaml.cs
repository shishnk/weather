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
            ViewModel!.UpdateImage.Subscribe(bitmap =>
                {
                    Image.Source = bitmap;
                    Image.InvalidateVisual();
                })
                .DisposeWith(disposables));
    }
}