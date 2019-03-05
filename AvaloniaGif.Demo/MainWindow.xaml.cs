using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;

namespace AvaloniaGif.Demo
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = new MainWindowViewModel();
        }
    }
}