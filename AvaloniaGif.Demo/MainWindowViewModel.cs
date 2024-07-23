using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaGif.Demo
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            Stretches = new List<Stretch>
            {
                Stretch.None,
                Stretch.Fill,
                Stretch.Uniform,
                Stretch.UniformToFill
            };

            AvailableGifs = AssetLoader.GetAssets(new Uri("avares://AvaloniaGif.Demo/Images/"), null)
                .Select(x => x).ToList();
        }

        [ObservableProperty] private IReadOnlyList<Uri> _availableGifs;
        [ObservableProperty] private string _selectedGif;
        [ObservableProperty] private IReadOnlyList<Stretch> _stretches;
        [ObservableProperty] private Stretch _stretch = Stretch.None;
    }
}