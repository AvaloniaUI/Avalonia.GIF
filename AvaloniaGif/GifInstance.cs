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
        bool _streamCanDispose;
        private GifDecoder _gifDecoder;
        private GifBackgroundWorker _bgWorker;
        private WriteableBitmap _targetBitmap;
        private bool _hasNewFrame;
        private bool _isDisposed;
        private readonly object _bitmapSync = new object();
        private static readonly object _globalUIThreadUpdateLock = new object();

        public void SetSource(object newValue)
        {
            var sourceUri = newValue as Uri;
            var sourceStr = newValue as Stream;

            Stream stream = null;

            if (sourceUri != null)
            {
                _streamCanDispose = true;
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
            this._gifDecoder = new GifDecoder(Stream);
            this._bgWorker = new GifBackgroundWorker(_gifDecoder);
            var pixSize = new PixelSize(_gifDecoder.Header.Dimensions.Width, _gifDecoder.Header.Dimensions.Height);
            this._targetBitmap = new WriteableBitmap(pixSize, new Vector(96, 96), PixelFormat.Bgra8888);

            TargetControl.Source = _targetBitmap;
            //TargetControl.DetachedFromVisualTree += delegate { this.Dispose(); };
            _bgWorker.CurrentFrameChanged += FrameChanged;

            Run();
        }

        private void RenderTick(TimeSpan time)
        {
            if (_isDisposed | !_hasNewFrame) return;
            lock (_globalUIThreadUpdateLock)
                lock (_bitmapSync)
                {
                    TargetControl?.InvalidateVisual();
                    _hasNewFrame = false;
                }
        }

        private void FrameChanged()
        {
            lock (_bitmapSync)
            {
                if (_isDisposed) return;
                _hasNewFrame = true;
                using (var lockedBitmap = _targetBitmap?.Lock())
                    _gifDecoder?.WriteBackBufToFb(lockedBitmap.Address);
            }
        }

        private void Run()
        {
            if (!Stream.CanSeek)
                throw new ArgumentException("The stream is not seekable");

            AvaloniaLocator.Current.GetService<IRenderTimer>().Tick += RenderTick;
            this._bgWorker?.SendCommand(BgWorkerCommand.Play);
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

        public void Dispose()
        {
            _isDisposed = true;
            AvaloniaLocator.Current.GetService<IRenderTimer>().Tick -= RenderTick;
            this._bgWorker?.SendCommand(BgWorkerCommand.Dispose);
            _targetBitmap?.Dispose();
        }
    }
}