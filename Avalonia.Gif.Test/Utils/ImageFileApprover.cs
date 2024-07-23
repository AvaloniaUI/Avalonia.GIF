using ApprovalTests.Approvers;
using ApprovalTests.Core;
using ApprovalTests.Core.Exceptions;
using Avalonia;

namespace UnitTest.Utils
{
    public class ImageFileApprover : FileApprover
    {
        public ImageFileApprover(IApprovalWriter writer, IApprovalNamer namer,
            bool normalizeLineEndingsForTextFiles = false)
            : base(writer, namer, normalizeLineEndingsForTextFiles)
        {
        }

        public override ApprovalException Approve(string approvedPath, string receivedPath)
        {
            if (Path.GetExtension(approvedPath) != ".png")
                return base.Approve(approvedPath, receivedPath);

            if (!File.Exists(approvedPath))
            {
                return new ApprovalMissingException(receivedPath, approvedPath);
            }

            // FIXME: I have no idea to compare bitmap with Avalonia.Media.Imaging
            //        This logic use System.Drawing, So only run on Windows.


            var approvedByte = GetImagePixels(approvedPath);
            var receivedByte = GetImagePixels(receivedPath);

            return !Compare(receivedByte, approvedByte)
                ? new ApprovalMismatchException(receivedPath, approvedPath)
                : null;
        }

        public unsafe byte[] GetImagePixels(string imagePath)
        {
            // Load the image
            using var bitmap = new Avalonia.Media.Imaging.Bitmap(imagePath);

            // Get the pixel data
            var pixelSize = bitmap.PixelSize;
            var pixels = new byte[pixelSize.Width * pixelSize.Height * 4]; // 4 bytes per pixel (RGBA)

            // Copy the pixel data to our array
            fixed (byte* ptr = &pixels[0])
                bitmap.CopyPixels(new PixelRect(pixelSize), (IntPtr)ptr, pixels.Length, pixelSize.Width * 4);

            return pixels;
        }


        private new static bool Compare(ICollection<byte> bytes1, ICollection<byte> bytes2)
        {
            if (bytes1.Count != bytes2.Count)
            {
                return false;
            }

            var e1 = bytes1.GetEnumerator();
            var e2 = bytes2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                if (e1.Current != e2.Current)
                {
                    return false;
                }
            }

            return true;
        }
    }
}