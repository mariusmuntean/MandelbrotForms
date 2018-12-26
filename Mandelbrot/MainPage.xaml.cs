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
            canvas.Translate(info.Width / 2.0f, info.Height / 2.0f);

            var bitmap = new SKBitmap(info.Width, info.Height);

            var maxIterations = 100;

            using (var bitmapCanvas = new SKCanvas(bitmap))
            {
                for (var pixX = 0; pixX < info.Width; pixX++)
                {
                    for (var pixY = 0; pixY < info.Height; pixY++)
                    {
                        // Map pixel coordinates to Mandelbrot scale, i.e. -2.5 < x < 1 and -1 < y 1 
                        var mandelX = MapToHorizontalMandelbrotScale(pixX,
                            0.0f,
                            info.Width);
                        var mandelY = MapToVerticalMandelbrotScale(pixY,
                            0.0f,
                            info.Height);

                        var mandelOrbitX = 0.0f;
                        var mandelOrbitY = 0.0f;
                        var currentIteration = 0;

                        while (mandelOrbitX * mandelOrbitX + mandelOrbitY * mandelOrbitY <= 2 * 2
                               && currentIteration < maxIterations)
                        {
                            var mandelOrbitTempX = mandelOrbitX * mandelOrbitX - mandelOrbitY * mandelOrbitY + mandelX;
                            mandelOrbitY = 2 * mandelOrbitX * mandelOrbitY + mandelY;
                            mandelOrbitX = mandelOrbitTempX;
                            currentIteration++;
                        }

                        var pixelColor = currentIteration < maxIterations ? SKColors.Black : SKColors.Red;
                        bitmapCanvas.DrawPoint(pixX, pixY, pixelColor);
                    }
                }
            }

            canvas.DrawBitmap(bitmap, -info.Width / 2.0f, -info.Height / 2.0f);
        }

        private float MapToVerticalMandelbrotScale(int x, float rangeStart, float rangeEnd)
        {
            return MapValueFromRangeToRange(x,
                rangeStart,
                rangeEnd,
                -1,
                1);
        }

        private float MapToHorizontalMandelbrotScale(int y, float rangeStart, float rangeEnd)
        {
            return MapValueFromRangeToRange(y,
                rangeStart,
                rangeEnd,
                -2.5f,
                1.0f);
        }

        private static float MapValueFromRangeToRange(int val,
            float originalRangeMin,
            float originalRangeMax,
            float newRangeMin,
            float newRangeMax)
        {
            if (val < originalRangeMin)
            {
                return newRangeMin;
            }

            if (val > originalRangeMax)
            {
                return newRangeMax;
            }

            var originalRangeMagnitude = Math.Abs(originalRangeMax - originalRangeMin);
            var valueMagnitude = Math.Abs(val - originalRangeMin);
            var rangePercentage = valueMagnitude / originalRangeMagnitude;

            var newRangeValue = newRangeMin + Math.Abs(newRangeMax - newRangeMin) * rangePercentage;

            return newRangeValue;
        }
    }
}