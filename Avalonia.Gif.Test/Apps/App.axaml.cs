using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;

namespace UnitTest.Base.Apps
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Avalonia.Controls.Window();
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static IDisposable Start()
        {
            var starter = new AppStarter();

            var th = new Thread(starter.Start);
            th.Start();

            return starter;
        }
    }

    internal class AppStarter : IDisposable
    {
        private ClassicDesktopStyleApplicationLifetime? _lifetime;

        public void Start()
        {
            var builder = AppBuilder.Configure<App>();
            builder.UsePlatformDetect();

            _lifetime = new ClassicDesktopStyleApplicationLifetime()
            {
                Args = Array.Empty<string>(),
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            builder.SetupWithLifetime(_lifetime);

            while (true)
            {
                Dispatcher.UIThread.RunJobs();
            }
        }

        public void Dispose()
        {
            try { _lifetime?.Shutdown(); }
            finally { _lifetime?.Dispose(); }
        }
    }
}
