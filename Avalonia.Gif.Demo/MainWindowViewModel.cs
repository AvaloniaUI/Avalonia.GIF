using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Media;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Gif.Demo;

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

            AvailableGifs = AssetLoader.GetAssets(new Uri("avares://Avalonia.Gif.Demo/Images/"), null)
                .Select(x => x).ToList();
        }

    [ObservableProperty] private IReadOnlyList<Uri> _availableGifs;
    [ObservableProperty] private Uri _selectedGif;
    [ObservableProperty] private IReadOnlyList<Stretch> _stretches;
    [ObservableProperty] private Stretch _stretch = Stretch.None;

    [RelayCommand]
    public void HangUi()
    {
            Thread.Sleep(5000);
        }
}