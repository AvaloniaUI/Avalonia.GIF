using Avalonia;
using Avalonia.Markup.Xaml;

namespace Avalonia.Gif.Demo
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            
            window.Show();
            base.OnFrameworkInitializationCompleted();
        }
    }
}