
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalUtilities.Utilities;
using Avalonia.Animation;
using NUnit.Framework;
using System.Reflection;
using UnitTest.Base.Apps;
using UnitTest.Base.Utils;
using UnitTest.Utils;
using static ApprovalTests.Scrubber.PdfScrubber;

namespace Avalonia.Gif.Test
{
    [UseReporter(typeof(DiffReporter))]
    public class UnitTest1
    {
        IDisposable _disposable;

        public UnitTest1()
        {
            _disposable = App.Start();

            Approvals.RegisterDefaultApprover((w, n, c) => new ImageFileApprover(w, n, c));
        }


        [Test]
        [RunOnUi]
        [TestCase("all_background.gif")]
        [TestCase("all_previous.gif")]
        [TestCase("all_none.gif")]
        [TestCase("firstnone_laterback.gif")]
        [TestCase("firstnone_laterprev.gif")]
        [TestCase("jagging_back_prev.gif")]
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
        [RunOnUi]
        [TestCase("all_background.gif")]
        [TestCase("all_previous.gif")]
        [TestCase("all_none.gif")]
        [TestCase("firstnone_laterback.gif")]
        [TestCase("firstnone_laterprev.gif")]
        [TestCase("jagging_back_prev.gif")]
        public void Jump(string filename)
        {
            var imageStream = Open(filename);
            var imageInstance = new GifInstance(imageStream);

            var indics = new List<int>();

            foreach (var step in Enumerable.Range(1, imageInstance.GifFrameCount))
            {
                indics.Add(0);

                for (int start = 1; start < imageInstance.GifFrameCount; ++start)
                    for (int idx = start; idx < imageInstance.GifFrameCount; idx += step)
                        indics.Add(idx);
            }

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
        [RunOnUi]
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

        public static Stream Open(string imagefilename)
        {
            var path = $"Avalonia.Gif.Test.Inputs.{imagefilename}";

            return Assembly.GetCallingAssembly().GetManifestResourceStream(path)
                   ?? throw new ArgumentException($"image not found: '{imagefilename}'");
        }
    }
}