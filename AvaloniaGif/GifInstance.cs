using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaGif.Decoding;

namespace AvaloniaGif
{
    internal class GifInstance : IDisposable
    {
        private readonly Stream currentStream;
        public IterationCount IterationCount { get; set; }
        public bool AutoStart { get; private set; } = true;
        public Progress<int> Progress { get; private set; }

        bool _streamCanDispose;
        private GifDecoder _gifDecoder;
        private GifBackgroundWorker _bgWorker;
        private WriteableBitmap _targetBitmap;
        private bool _hasNewFrame;

        public CancellationTokenSource CurrentCts { get; private set; }

        public GifInstance(object newValue)
        {
            CurrentCts = new CancellationTokenSource();

            currentStream = newValue switch
            {
                Stream s => s,
                Uri u => GetStreamFromUri(u),
                string str => GetStreamFromString(str),
                _ => throw new InvalidDataException("Unsupported source object")
            };

            if (!currentStream.CanRead)
            {
                throw new InvalidOperationException("Can't read the stream provided.");
            }

            if (currentStream.CanSeek)
            {
                currentStream.Seek(0, SeekOrigin.Begin);
            }

            _gifDecoder = new GifDecoder(currentStream, CurrentCts.Token);
            _bgWorker = new GifBackgroundWorker(_gifDecoder, CurrentCts.Token);
            var pixSize = new PixelSize(_gifDecoder.Header.Dimensions.Width, _gifDecoder.Header.Dimensions.Height);

            _targetBitmap = new WriteableBitmap(pixSize, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            _bgWorker.CurrentFrameChanged += FrameChanged;
            GifPixelSize = pixSize;
            Run();
        }

        private Stream GetStreamFromString(string str)
        {
            if (!Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var res))
            {
                throw new InvalidCastException("The string provided can't be converted to URI.");
            }

            return GetStreamFromUri(res);
        }

        private Stream GetStreamFromUri(Uri uri)
        {
            _streamCanDispose = true;

            Progress = new Progress<int>();

            var uriString = uri.OriginalString.Trim();

            if (!uriString.StartsWith("resm") && !uriString.StartsWith("avares"))
                throw new InvalidDataException(
                    "The URI provided is not currently supported.");

            var assetLocator = AvaloniaLocator.Current.GetService<IAssetLoader>();

            if (assetLocator is null)
                throw new InvalidDataException(
                    "The resource URI was not found in the current assembly.");

            return assetLocator.Open(uri);
        }

        public PixelSize GifPixelSize { get; private set; }

        public WriteableBitmap GetBitmap()
        {
            if (!_hasNewFrame) return null;
            _hasNewFrame = false;
            return _targetBitmap;
        }

        private void FrameChanged()
        {
            if (CurrentCts.IsCancellationRequested)
            {
                CurrentCts.Dispose();
                return;
            }

            _hasNewFrame = true;

            using (var lockedBitmap = _targetBitmap?.Lock())
                _gifDecoder?.WriteBackBufToFb(lockedBitmap.Address);
        }

        private void Run()
        {
            if (!currentStream.CanSeek)
                throw new ArgumentException("The stream is not seekable");

            _bgWorker?.SendCommand(BgWorkerCommand.Play);
        }

        public void IterationCountChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var newVal = (IterationCount) e.NewValue;
            IterationCount = newVal;
        }

        public void AutoStartChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var newVal = (bool) e.NewValue;
            AutoStart = newVal;
        }

        public void Dispose()
        {
            CurrentCts.Cancel();

            if (_streamCanDispose)
            {
                currentStream.Dispose();
            }

            _bgWorker?.SendCommand(BgWorkerCommand.Dispose);
            _targetBitmap?.Dispose();
        }
    }
}