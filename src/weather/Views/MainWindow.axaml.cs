using Avalonia.ReactiveUI;
using weather.ViewModels;

namespace weather.Views;

public partial class MainWindow : ReactiveWindow<SearchViewModel>
{
    public MainWindow()
    {
        ViewModel = new();
        InitializeComponent();
    }
}