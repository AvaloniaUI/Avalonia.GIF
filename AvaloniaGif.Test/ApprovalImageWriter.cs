using ApprovalTests.Core;
using Avalonia.Media.Imaging;
using System;
using System.IO;

namespace AvaloniaGif.Test
{
    public class ApprovalImageWriter : IApprovalWriter
    {
        public Bitmap Data { get; set; }
        public string Parameter { get; }

        public ApprovalImageWriter(Bitmap image, object parameter)
        {
            Data = image ?? throw new ArgumentNullException(nameof(image));
            Parameter = parameter?.ToString() ?? "null";
        }

        public virtual string GetApprovalFilename(string basename)
        {
            return $"{basename}#{Parameter}.approved.png";
        }

        public virtual string GetReceivedFilename(string basename)
        {
            return $"{basename}#{Parameter}.received.png";
        }

        public string WriteReceivedFile(string received)
        {
            var dir = Path.GetDirectoryName(received);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            Data.Save(received);
            return received;
        }
    }
}
