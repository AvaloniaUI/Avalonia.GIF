using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using ReactiveUI;

namespace AvaloniaGif.Demo
{
    public class MainWindowViewModel : ReactiveObject
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

            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

            AvailableGifs = assetLoader.GetAssets(new Uri("avares://AvaloniaGif.Demo/Images/"), null)
                .Select(x => x).ToList();
        }

        private IReadOnlyList<Uri> _availableGifs;

        public IReadOnlyList<Uri> AvailableGifs
        {
            get => _availableGifs;
            set => this.RaiseAndSetIfChanged(ref _availableGifs, value);
        }

        private string _selectedGif;

        public string SelectedGif
        {
            get => _selectedGif;
            set => this.RaiseAndSetIfChanged(ref _selectedGif, value);
        }

        private IReadOnlyList<Stretch> _stretches;

        public IReadOnlyList<Stretch> Stretches
        {
            get => _stretches;
            set => this.RaiseAndSetIfChanged(ref _stretches, value);
        }

        private Stretch _stretch = Stretch.None;

        public Stretch Stretch
        {
            get => _stretch;
            set => this.RaiseAndSetIfChanged(ref _stretch, value);
        }
    }
}