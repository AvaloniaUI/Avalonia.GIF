using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaGif.Decoding;

namespace AvaloniaGif
{
    internal sealed class GifBackgroundWorker
    {
        private static readonly Stopwatch _timer = Stopwatch.StartNew();
        private GifDecoder _gifDecoder;

        private Task _bgThread;
        private BgWorkerState _state;
        private readonly object _lockObj;
        private readonly Queue<BgWorkerCommand> _cmdQueue;
        private readonly List<ulong> _colorTableIDList;
        private volatile bool _shouldStop;
        private int _iterationCount;

        private GifRepeatBehavior _repeatBehavior;
        public GifRepeatBehavior IterationCount
        {
            get => _repeatBehavior;
            set
            {
                lock (_lockObj)
                {
                    InternalSeek(0, true);
                    ResetPlayVars();
                    _state = BgWorkerState.Paused;
                    _repeatBehavior = value;
                }
            }
        }

        public Action CurrentFrameChanged;
        private int _currentIndex;
        private readonly CancellationToken _ctoken;

        public int CurrentFrameIndex
        {
            get => _currentIndex;
            set
            {
                if (value != _currentIndex)
                    lock (_lockObj)
                        InternalSeek(value, true);
            }
        }

        private void ResetPlayVars()
        {
            _iterationCount = 0;
            CurrentFrameIndex = -1;
        }

        private void RefreshColorTableCache()
        {
            foreach (var cacheId in _colorTableIDList)
                GifDecoder.GlobalColorTableCache.TryGetValue(cacheId, out var _);
        }

        private void InternalSeek(int value, bool isManual)
        {
            var lowerBound = 0;

            // Skip already rendered frames if the seek position is above the previous frame index.
            if (isManual & value > _currentIndex)
            {
                // Only render the new seeked frame if the delta
                // seek position is just 1 frame.
                if (value - _currentIndex == 1)
                {
                    _gifDecoder.RenderFrame(value);
                    SetIndexVal(value, isManual);
                    return;
                }
                lowerBound = _currentIndex;
            }

            for (var fI = lowerBound; fI <= value; fI++)
            {
                var targetFrame = _gifDecoder.Frames[fI];

                // Ignore frames with restore disposal method except the current one.
                if (fI != value & targetFrame.FrameDisposalMethod == FrameDisposal.Restore)
                    continue;

                _gifDecoder.RenderFrame(fI);
            }

            SetIndexVal(value, isManual);
        }

        private void SetIndexVal(int value, bool isManual)
        {
            _currentIndex = value;

            if (isManual)
            {
                if (_state == BgWorkerState.Complete)
                {
                    _state = BgWorkerState.Paused;
                    _iterationCount = 0;
                }

                CurrentFrameChanged?.Invoke();
            }
        }

        public GifBackgroundWorker(GifDecoder gifDecode, CancellationToken cancellationToken)
        {
            _gifDecoder = gifDecode;
            _lockObj = new object();
            _repeatBehavior = new GifRepeatBehavior { LoopForever = true };
            _cmdQueue = new Queue<BgWorkerCommand>();
            _ctoken = cancellationToken;

            // Save the color table cache ID's to refresh them on cache while
            // the image is either stopped/paused.
            _colorTableIDList = _gifDecoder.Frames
                                          .Where(p => p.IsLocalColorTableUsed)
                                          .Select(p => p.LocalColorTableCacheID)
                                          .ToList();

            if (_gifDecoder.Header.HasGlobalColorTable)
                _colorTableIDList.Add(_gifDecoder.Header.GlobalColorTableCacheID);

            ResetPlayVars();

            _bgThread = Task.Factory.StartNew(MainLoop, cancellationToken, TaskCreationOptions.LongRunning,
                TaskScheduler.Current);
        }

        public void SendCommand(BgWorkerCommand cmd)
        {
            lock (_lockObj)
                _cmdQueue.Enqueue(cmd);
        }

        public BgWorkerState GetState()
        {
            lock (_lockObj)
            {
                var ret = _state;
                return ret;
            }
        }

        private void MainLoop()
        {
            while (true)
            {
                if (_shouldStop || _ctoken.IsCancellationRequested)
                {
                    DoDispose();
                    break;
                }

                CheckCommands();
                DoStates();
            }
        }

        private void DoStates()
        {
            switch (_state)
            {
                case BgWorkerState.Null:
                    Thread.Sleep(40);
                    break;
                case BgWorkerState.Paused:
                    RefreshColorTableCache();
                    Thread.Sleep(60);
                    break;
                case BgWorkerState.Start:
                    _state = BgWorkerState.Running;
                    break;
                case BgWorkerState.Running:
                    WaitAndRenderNext();
                    break;
                case BgWorkerState.Complete:
                    RefreshColorTableCache();
                    Thread.Sleep(60);
                    break;
            }
        }

        private void CheckCommands()
        {
            BgWorkerCommand cmd;

            lock (_lockObj)
            {
                if (_cmdQueue.Count <= 0) return;
                cmd = _cmdQueue.Dequeue();
            }

            switch (cmd)
            {
                case BgWorkerCommand.Dispose:
                    DoDispose();
                    break;
                case BgWorkerCommand.Play:
                    switch (_state)
                    {
                        case BgWorkerState.Null:
                            _state = BgWorkerState.Start;
                            break;
                        case BgWorkerState.Paused:
                            _state = BgWorkerState.Running;
                            break;
                        case BgWorkerState.Complete:
                            ResetPlayVars();
                            _state = BgWorkerState.Start;
                            break;
                    }
                    break;
                case BgWorkerCommand.Pause:
                    switch (_state)
                    {
                        case BgWorkerState.Running:
                            _state = BgWorkerState.Paused;
                            break;
                    }
                    break;
            }

        }

        private void DoDispose()
        {
            _state = BgWorkerState.Dispose;
            _shouldStop = true;
            _gifDecoder.Dispose();
        }

        private void ShowFirstFrame()
        {
            if (_shouldStop) return;
            _gifDecoder.RenderFrame(0);
        }

        private void WaitAndRenderNext()
        {
            if (!IterationCount.LoopForever & _iterationCount > IterationCount.Count)
            {
                _state = BgWorkerState.Complete;
                return;
            }

            _currentIndex = (_currentIndex + 1) % _gifDecoder.Frames.Count;

            CurrentFrameChanged?.Invoke();

            var targetDelay = _gifDecoder.Frames[_currentIndex].FrameDelay;

            var t1 = _timer.Elapsed;

            _gifDecoder.RenderFrame(_currentIndex);

            var t2 = _timer.Elapsed;
            var delta = t2 - t1;

            if (delta > targetDelay) return;
            Thread.Sleep(targetDelay - delta);

            if (!IterationCount.LoopForever & _currentIndex == 0)
                _iterationCount++;
        }

        ~GifBackgroundWorker()
        {
            DoDispose();
        }
    }
}