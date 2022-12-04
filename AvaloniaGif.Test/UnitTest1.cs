
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalUtilities.Utilities;
using Avalonia.Animation;
using NUnit.Framework;
using System.Reflection;
using UnitTest.Base.Apps;
using UnitTest.Base.Utils;
using UnitTest.Utils;

namespace AvaloniaGif.Test
{
    [UseReporter(typeof(DiffReporter))]
    public class UnitTest1
    {
        IDisposable disposable;

        public UnitTest1()
        {
            disposable = App.Start();

            Approvals.RegisterDefaultApprover((w, n, c) => new ImageFileApprover(w, n, c));
        }


        [Test]
        [RunOnUI]
        [TestCase("all_background.gif")]
        [TestCase("all_previous.gif")]
        [TestCase("all_none.gif")]
        [TestCase("firstnone_laterback.gif")]
        [TestCase("firstnone_laterprev.gif")]
        public void Sequencial(string filename)
        {
            var imageStream = Open(filename);
            var imageInstance = new GifInstance(imageStream);

            for (int i = 0; i < imageInstance.GifFrameCount; ++i)
            {
                var img = imageInstance.ProcessFrameIndex(i);

                Approvals.Verify(
                    new ApprovalImageWriter(img, $"{Path.GetFileNameWithoutExtension(filename)}@{i}"),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }
        }

        [Test]
        [RunOnUI]
        [TestCase("all_background.gif")]
        [TestCase("all_previous.gif")]
        [TestCase("all_none.gif")]
        [TestCase("firstnone_laterback.gif")]
        [TestCase("firstnone_laterprev.gif")]
        public void Jump(string filename)
        {
            var imageStream = Open(filename);
            var imageInstance = new GifInstance(imageStream);

            var indics = Concat(
                Enumerable.Range(0, imageInstance.GifFrameCount).Where(i => i % 2 == 0),
                Enumerable.Range(0, imageInstance.GifFrameCount).Where(i => i % 2 == 1),

                Enumerable.Range(0, imageInstance.GifFrameCount).Where(i => i % 3 == 0),
                Enumerable.Range(0, imageInstance.GifFrameCount).Where(i => i % 3 == 1),
                Enumerable.Range(0, imageInstance.GifFrameCount).Where(i => i % 3 == 2)
            );

            foreach (int i in indics)
            {
                var img = imageInstance.ProcessFrameIndex(i);

                Approvals.Verify(
                    new ApprovalImageWriter(img, $"{Path.GetFileNameWithoutExtension(filename)}@{i}"),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }
        }

        [Test]
        [RunOnUI]
        [TestCase("all_none.gif")]
        public void Timespan(string filename)
        {
            var imageStream = Open(filename);
            var imageInstance = new GifInstance(imageStream);
            imageInstance.IterationCount = IterationCount.Infinite;

            var times = new[] {
                TimeSpan.FromTicks(TimeSpan.FromSeconds(0).Ticks+0),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(0).Ticks+1),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(1).Ticks-1),

                TimeSpan.FromTicks(TimeSpan.FromSeconds(1).Ticks+0),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(1).Ticks+1),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(2).Ticks-1),

                TimeSpan.FromTicks(TimeSpan.FromSeconds(2).Ticks+0),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(2).Ticks+1),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(3).Ticks-1),

                TimeSpan.FromTicks(TimeSpan.FromSeconds(3).Ticks+0),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(3).Ticks+1),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(4).Ticks-1),

                TimeSpan.FromTicks(TimeSpan.FromSeconds(4).Ticks+0),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(4).Ticks+1),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(5).Ticks-1),

                TimeSpan.FromTicks(TimeSpan.FromSeconds(5).Ticks+0),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(5).Ticks+1),
                TimeSpan.FromTicks(TimeSpan.FromSeconds(6).Ticks-1),
            };

            foreach (var time in times)
            {
                var img = imageInstance.ProcessFrameTime(time);

                Approvals.Verify(
                    new ApprovalImageWriter(img, $"{Path.GetFileNameWithoutExtension(filename)}@{time.Seconds % 4}"),
                    Approvals.GetDefaultNamer(),
                    new DiffToolReporter(DiffEngine.DiffTool.WinMerge));
            }
        }

        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] arrays)
        {
            foreach (var array in arrays)
                foreach (var element in array)
                    yield return element;
        }


        public static Stream Open(string imagefilename)
        {
            var path = $"AvaloniaGif.Test.Inputs.{imagefilename}";

            return Assembly.GetCallingAssembly().GetManifestResourceStream(path)
                   ?? throw new ArgumentException($"image not found: '{imagefilename}'");
        }
    }
}