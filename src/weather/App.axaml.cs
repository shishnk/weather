using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Splat;
using weather.Context.Bootstrapper;
using weather.ViewModels;
using weather.Views;

namespace weather;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // ReSharper disable once UnusedVariable
        var bootstrapper = new AppBootstrapper();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                ViewModel = Locator.Current.GetService<SearchViewModel>() 
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}