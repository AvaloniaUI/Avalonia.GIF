using AvaloniaGif.Decoding;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Animation;
using System.Threading;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvaloniaGif
{
    public class GifInstance : IDisposable
    {
        public Image Image { get; set; }
        public GifDecoder Renderer { get; private set; }
        public IClock Clock { get; private set; }

        public int FrameCount { get; private set; }
        public Stream Stream { get; private set; }
        public IterationCount IterationCount { get; private set; }
        public bool AutoStart { get; private set; } = true;
        public Progress<int> Progress { get; private set; }

        internal CancellationTokenSource _rendererSignal = new CancellationTokenSource();

        TimeSpan _prevTime, _delta;
        int CurrentFrame;
        IDisposable sub1;
        bool streamCanDispose, _isFirstRun;

        private GifDecoder gifDecoder;
        private GifBackgroundWorker bgWorker;
        private WriteableBitmap targetBitmap;

        public void Dispose()
        {
            this.bgWorker?.SendCommand(BgWorkerCommand.Dispose);
            targetBitmap?.Dispose();
        }

        public async void SetSource(object newValue)
        {
            var sourceUri = newValue as Uri;
            var sourceStr = newValue as Stream;

            Stream stream = null;

            if (sourceUri != null)
            {
                streamCanDispose = true;
                this.Progress = new Progress<int>();

                if (sourceUri.OriginalString.Trim().StartsWith("resm"))
                {
                    
                }

            }
            else if (sourceStr != null)
            {
                stream = sourceStr;
            }
            else
            {
                throw new InvalidDataException("Missing valid URI or Stream.");
            }

            this.gifDecoder = new GifDecoder(stream);
            this.bgWorker = new GifBackgroundWorker(gifDecoder);
            var pixSize = new PixelSize(gifDecoder.Header.Dimensions.Width, gifDecoder.Header.Dimensions.Height);
            this.targetBitmap = new WriteableBitmap(pixSize, new Vector(96, 96), PixelFormat.Bgra8888);

            Image.DetachedFromVisualTree += delegate
            {
                Dispose();
            };

            bgWorker.CurrentFrameChanged += () => Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(FrameChanged);

            Run();
        }

        private void FrameChanged()
        {
            using (var lockedBitmap = targetBitmap.Lock())
            {
                gifDecoder.WriteBackBufToFb(lockedBitmap.Address);
            }
            Image.InvalidateVisual();
        }

        private void Run()
        {
            if (!Stream.CanSeek)
                throw new ArgumentException("The stream is not seekable");

            this.bgWorker?.SendCommand(BgWorkerCommand.Play);
        }

        public void IterationCountChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var newVal = (IterationCount)e.NewValue;
            this.IterationCount = newVal;
        }

        public void AutoStartChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var newVal = (bool)e.NewValue;
            this.AutoStart = newVal;
        }
    }
}