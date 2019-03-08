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
using Avalonia.Rendering;
using Avalonia.Logging;

namespace AvaloniaGif
{
    public class GifInstance : IDisposable
    {
        public Image TargetControl { get; set; }
        public Stream Stream { get; private set; }
        public IterationCount IterationCount { get; private set; }
        public bool AutoStart { get; private set; } = true;
        public Progress<int> Progress { get; private set; }
        bool streamCanDispose;
        private GifDecoder gifDecoder;
        private GifBackgroundWorker bgWorker;
        private WriteableBitmap targetBitmap;
        private bool hasNewFrame;
        private readonly object bitmapSync = new object();
        public void Dispose()
        {
            return;
            // AvaloniaLocator.Current.GetService<IRenderTimer>().Tick -= RenderTick;
            // this.bgWorker?.SendCommand(BgWorkerCommand.Dispose);
            // targetBitmap?.Dispose();
        }

        public void SetSource(object newValue)
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
                    var assetLocator = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    stream = assetLocator.Open(sourceUri);
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

            Stream = stream;
            this.gifDecoder = new GifDecoder(Stream);
            this.bgWorker = new GifBackgroundWorker(gifDecoder);
            var pixSize = new PixelSize(gifDecoder.Header.Dimensions.Width, gifDecoder.Header.Dimensions.Height);
            this.targetBitmap = new WriteableBitmap(pixSize, new Vector(96, 96), PixelFormat.Bgra8888);

            TargetControl.Source = targetBitmap;
           // TargetControl.DetachedFromVisualTree += delegate { this.Dispose(); };
            bgWorker.CurrentFrameChanged += FrameChanged;

            Run();
        }

        private void RenderTick(TimeSpan time)
        {
            lock (bitmapSync)
            {
                if (!hasNewFrame) return;
                TargetControl?.InvalidateVisual();
                hasNewFrame = false;
            }
        }

        private void FrameChanged()
        {
            lock (bitmapSync)
            {
                try
                {
                    hasNewFrame = true;
                    using (var lockedBitmap = targetBitmap?.Lock())
                        gifDecoder?.WriteBackBufToFb(lockedBitmap.Address);
                }
                catch (Exception e)
                {
                    AvaloniaLocator.Current.GetService<ILogSink>().Log(LogEventLevel.Error, "GIF", this, e.Message);
                }
            }
        }

        private void Run()
        {
            if (!Stream.CanSeek)
                throw new ArgumentException("The stream is not seekable");

            AvaloniaLocator.Current.GetService<IRenderTimer>().Tick += RenderTick;
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