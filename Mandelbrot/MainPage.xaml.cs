using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Mandelbrot
{
    public partial class MainPage : ContentPage
    {
        Random rand = new Random();

        public MainPage()
        {
            InitializeComponent();
        }

        private void SKCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;
            var info = e.Info;

            canvas.Clear();

            var bitmap = new SKBitmap(info.Width - 10, info.Height - 10);
            using (var bitmapCanvas = new SKCanvas(bitmap))
            {
                for (var i = 5; i < info.Width - 5; i++)
                {
                    for (var j = 5; j < info.Height - 5; j++)
                    {
                        var color = rand.NextDouble() > 0.5f ? SKColors.Red : SKColors.Black;
                        bitmapCanvas.DrawPoint(new SKPoint(i, j), color);
                    }
                }
            }

            canvas.DrawBitmap(bitmap, 5, 5);
        }
    }
}