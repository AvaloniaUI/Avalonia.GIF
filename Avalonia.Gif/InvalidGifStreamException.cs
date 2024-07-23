using System;

namespace Avalonia.Gif;

[Serializable]
internal class InvalidGifStreamException : Exception
{
    public override string Message { get; }

    public InvalidGifStreamException(string message)
    {
        Message = message;
    }
}