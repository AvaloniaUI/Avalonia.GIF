using System;
using System.Collections.Generic;
using Avalonia.Media;
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
            AvailableGifs = new List<Uri>
            {
                new Uri("resm:AvaloniaGif.Demo.Images.laundry.gif"),
                new Uri("resm:AvaloniaGif.Demo.Images.earth.gif"),
                new Uri("resm:AvaloniaGif.Demo.Images.rainbow.gif"),
                new Uri("resm:AvaloniaGif.Demo.Images.newton-cradle.gif"),
                
                // Great shots by Vitaly Silkin, free to use:
                // https://dribbble.com/colder/projects/219798-Loaders
                new Uri("resm:AvaloniaGif.Demo.Images.loader.gif"), 
                new Uri("resm:AvaloniaGif.Demo.Images.evitare-loader.gif"), 
                new Uri("resm:AvaloniaGif.Demo.Images.c-loader.gif") 
            };
        }

        private IReadOnlyList<Uri> _availableGifs;
        public IReadOnlyList<Uri> AvailableGifs
        {
            get => _availableGifs;
            set => this.RaiseAndSetIfChanged(ref _availableGifs, value);
        }

        private Uri _selectedGif;
        public Uri SelectedGif
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
