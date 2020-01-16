using System;
using System.IO;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;

namespace AvaloniaGif
{
    public class GifImage
    {
        static GifImage()
        {
            SourceUriRawProperty.Changed.Subscribe(SourceChanged);
            SourceUriProperty.Changed.Subscribe(SourceChanged);
            SourceStreamProperty.Changed.Subscribe(SourceChanged);
            IterationCountProperty.Changed.Subscribe(IterationCountChanged);
            AutoStartProperty.Changed.Subscribe(AutoStartChanged);
        }
        
        public static readonly AttachedProperty<string> SourceUriRawProperty = AvaloniaProperty
            .RegisterAttached<GifImage, Image, string>("SourceUriRaw");

        public static string GetSourceUriRaw(Image target)
        {
            return target.GetValue(SourceUriRawProperty);
        }

        public static void SetSourceUriRaw(Image target, string value)
        {
            target.SetValue(SourceUriRawProperty, value);
        }

        public static readonly AttachedProperty<Uri> SourceUriProperty = AvaloniaProperty
            .RegisterAttached<GifImage, Image, Uri>("SourceUri");

        public static Uri GetSourceUri(Image target)
        {
            return target.GetValue(SourceUriProperty);
        }

        public static void SetSourceUri(Image target, Uri value)
        {
            target.SetValue(SourceUriProperty, value);
        }

        public static readonly AttachedProperty<Stream> SourceStreamProperty = AvaloniaProperty
            .RegisterAttached<GifImage, Image, Stream>("SourceStream");

        public static Stream GetSourceStream(Image target)
        {
            return target.GetValue(SourceStreamProperty);
        }

        public static void SetSourceStream(Image target, Stream value)
        {
            target.SetValue(SourceStreamProperty, value);
        }

        public static readonly AttachedProperty<IterationCount> IterationCountProperty = AvaloniaProperty
            .RegisterAttached<GifImage, Image, IterationCount>("IterationCount", IterationCount.Infinite);

        public static IterationCount GetIterationCount(Image target)
        {
            return target.GetValue(IterationCountProperty);
        }

        public static void SetIterationCount(Image target, IterationCount value)
        {
            target.SetValue(IterationCountProperty, value);
        }

        public static readonly AttachedProperty<bool> AutoStartProperty = AvaloniaProperty
            .RegisterAttached<GifImage, Image, bool>("AutoStart", true);

        public static bool GetAutoStart(Image target)
        {
            return target.GetValue(AutoStartProperty);
        }

        public static void SetAutoStart(Image target, bool value)
        {
            target.SetValue(AutoStartProperty, value);
        }

        public static readonly AttachedProperty<GifInstance> InstanceProperty = AvaloniaProperty
            .RegisterAttached<GifImage, Image, GifInstance>("Instance");

        public static GifInstance GetInstance(Image target)
        {
            return target.GetValue(InstanceProperty);
        }

        public static void SetInstance(Image target, GifInstance value)
        {
            target.SetValue(InstanceProperty, value);
        }
        
        private static void AutoStartChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var image = e.Sender as Image;
            if (image == null)
                return;

            GetInstance(image)?.AutoStartChanged(e);
        }

        private static void IterationCountChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var image = e.Sender as Image;
            if (image == null)
                return;

            GetInstance(image)?.IterationCountChanged(e);
        }

        private static void SourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var image = e.Sender as Image;
            if (image == null)
                return;

            var instance = GetInstance(image);
            instance?.Dispose();

            var value = e.NewValue;
            if (value is string s)
                value = new Uri(s);

            instance = new GifInstance {TargetControl = image};
            instance.SetSource(value);
            SetInstance(image, instance);
        }
    }
}