using System;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace Avalonia.Gif;

internal class GifCustomVisualHandler : CompositionCustomVisualHandler
{
    private TimeSpan _animationElapsed;
    private TimeSpan? _lastServerTime;
    private GifInstance? _currentInstance;
    private bool _running;

    public static readonly object StopMessage = new(), StartMessage = new();

    public override void OnMessage(object message)
    {
        if (message == StartMessage)
        {
            _running = true;
            _lastServerTime = null;
            RegisterForNextAnimationFrameUpdate();
        }
        else if (message == StopMessage)
        {
            _running = false;
            _currentInstance?.Dispose();
            _currentInstance = null;
        }
        else if (message is GifInstance instance)
        {
            _currentInstance?.Dispose();
            _currentInstance = instance;
        }
    }

    public override void OnAnimationFrameUpdate()
    {
        if (!_running) return;
        Invalidate();
        RegisterForNextAnimationFrameUpdate();
    }

    public override void OnRender(ImmediateDrawingContext drawingContext)
    {
        if (_running)
        {
            if (_lastServerTime.HasValue) _animationElapsed += CompositionNow - _lastServerTime.Value;
            _lastServerTime = CompositionNow;
        }

        try
        {
            if (_currentInstance is null || _currentInstance.IsDisposed) return;

            var bitmap = _currentInstance.ProcessFrameTime(_animationElapsed);
            if (bitmap is not null)
            {
                drawingContext.DrawBitmap(bitmap, new Rect(_currentInstance.GifPixelSize.ToSize(1)),
                    GetRenderBounds());
            }
        }
        catch (Exception e)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "GifImage Renderer ", this, e.ToString());
        }
    }
}