using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.Forms.Platform.MacOS;
using Xamarin.Forms;

namespace Mandelbrot.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow _window;

        public AppDelegate()
        {
            var windowStyle = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
            var rect = new CGRect(400, 400, 1000, 1000);

            _window = new NSWindow(rect, windowStyle, NSBackingStore.Buffered, false)
            {
                Title = "Mandelbrot",
                TitleVisibility = NSWindowTitleVisibility.Hidden
            };
        }

        public override NSWindow MainWindow => _window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }
    }
}
