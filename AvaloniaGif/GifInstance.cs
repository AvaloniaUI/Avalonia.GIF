using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaGif.Decoding;
using JetBrains.Annotations;

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

        private WriteableBitmap _targetBitmap;
        private TimeSpan _totalTime;
        private readonly List<TimeSpan> _frameTimes;
        private uint _iterationCount;
        private int _currentFrameIndex;
        private uint _totalFrameCount;

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
            // _bgWorker = new GifBackgroundWorker(_gifDecoder, CurrentCts.Token);
            var pixSize = new PixelSize(_gifDecoder.Header.Dimensions.Width, _gifDecoder.Header.Dimensions.Height);

            _targetBitmap = new WriteableBitmap(pixSize, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            // _bgWorker.CurrentFrameChanged += FrameChanged;
            GifPixelSize = pixSize;

            _totalTime = TimeSpan.Zero;

            _frameTimes = _gifDecoder.Frames.Select(frame =>
            {
                _totalTime = _totalTime.Add(frame.FrameDelay);
                return _totalTime;
            }).ToList();

            _gifDecoder.RenderFrame(0, _targetBitmap);
            
            if (!currentStream.CanSeek)
                throw new InvalidDataException("The provided stream is not seekable.");
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

            // _bgWorker?.SendCommand(BgWorkerCommand.Dispose);
            _targetBitmap?.Dispose();
        }
        
        [CanBeNull]
        public WriteableBitmap ProcessFrameTime(TimeSpan stopwatchElapsed)
        {
            if (!IterationCount.IsInfinite & _iterationCount > IterationCount.Value)
            {
                // _state = BgWorkerState.Complete;
                return null;
            }

            var timeModulus = TimeSpan.FromTicks(stopwatchElapsed.Ticks % _totalTime.Ticks);
            var targetFrame = _frameTimes.LastOrDefault(x => x <= timeModulus);

            var currentFrame = _frameTimes.IndexOf(targetFrame);

            if (currentFrame == -1) currentFrame = 0;

 
            if (_currentFrameIndex != currentFrame)
            {
                _currentFrameIndex = currentFrame;

                _gifDecoder.RenderFrame(_currentFrameIndex, _targetBitmap);

                _totalFrameCount++;

                if (!IterationCount.IsInfinite && _totalFrameCount % _frameTimes.Count == 0)
                    _iterationCount++;

            }


            return _targetBitmap;

            // _currentFrameIndex = (_currentFrameIndex + 1) % _gifDecoder.Frames.Count;

            // CurrentFrameChanged?.Invoke();
            //
            // var targetDelay = _gifDecoder.Frames[_currentIndex].FrameDelay;
            //
            // var t1 = _timer.Elapsed;
            //
            // _gifDecoder.RenderFrame(_currentIndex);
            //
            // var t2 = _timer.Elapsed;
            // var delta = t2 - t1;
            //
            // if (delta > targetDelay) return;
            // Thread.Sleep(targetDelay - delta);
        }
    }
}