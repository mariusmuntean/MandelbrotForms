using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FractalSharp.Extensions;
using FractalSharp.Models;
using FractalSharp.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace FractalSharp
{
    public partial class MainPage : ContentPage
    {
        private ConcurrentBag<SKBitmap> _mandelbrotBitmaps;
        private ConcurrentDictionary<int, SKColor> _colors;

        private readonly SKPaint _bitmapPaint = new SKPaint
        {
            Color = SKColors.Plum,
            IsAntialias = true,
            FilterQuality = SKFilterQuality.Medium
        };

        public MainPage()
        {
            InitializeComponent();

            this.SetBinding(MainPage.MandelbrotProperty, nameof(Mandelbrot), BindingMode.OneWay);
            this.SetBinding(MainPage.DesiredComplexPlaneAreaProperty, nameof(DesiredComplexPlaneArea), BindingMode.OneWayToSource);
        }

        public static readonly BindableProperty MandelbrotProperty = BindableProperty.Create(
            "Mandelbrot",
            typeof(Models.Mandelbrot),
            typeof(MainPage),
            null,
            BindingMode.OneWay,
            propertyChanged: OnDisplayPointsChanged
        );

        private static void OnDisplayPointsChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((MainPage) bindable).RefreshMandelbrotCanvas();
        }

        public Models.Mandelbrot Mandelbrot
        {
            get => (Models.Mandelbrot) GetValue(MandelbrotProperty);
            set => SetValue(MandelbrotProperty, value);
        }

        private void RefreshMandelbrotCanvas()
        {
            _mandelbrotBitmaps = new ConcurrentBag<SKBitmap>();
            _colors = new ConcurrentDictionary<int, SKColor>();

            if (Mandelbrot?.Points.Any() != true)
            {
                Device.BeginInvokeOnMainThread(() => { MandelCanvas.InvalidateSurface(); });
                return;
            }

            var schedulingBitmapsStopwatch = new Stopwatch();
            schedulingBitmapsStopwatch.Start();

            var chunksSize = Mandelbrot.Points.Count / Environment.ProcessorCount;
            Task.Run(() =>
            {
                var computingBitmapsStopwatch = new Stopwatch();
                computingBitmapsStopwatch.Start();
                Parallel.ForEach(Mandelbrot.Points.GetListChunks(chunksSize), pointsChunk =>
                {
                    var currentBitmap = new SKBitmap(Mandelbrot.Width, Mandelbrot.Height, false);
                    for (var i = 0; i < pointsChunk.Count; i++)
                    {
                        var displayPoint = pointsChunk[i];
                        var h = ((float) displayPoint.Iterations / PureMandelbrotService.MaxIterations);
                        var s = 1 / h;
                        var l = 1 / h;
                        var color = _colors.GetOrAdd(displayPoint.Iterations, SKColor.FromHsl(h, s, l));

                        // Map from complex numbers to device pixels
                        var pixelPoint = GetPixelPointFromComplexNumber(displayPoint.Complex, Mandelbrot);
                        currentBitmap.SetPixel((int) pixelPoint.X, (int) pixelPoint.Y, color);
                    }

                    _mandelbrotBitmaps.Add(currentBitmap);

                    Device.BeginInvokeOnMainThread(() => { MandelCanvas.InvalidateSurface(); });
                });

                Device.BeginInvokeOnMainThread(() =>
                {
                    TotalBitmapComputationLbl.Text = $"Total bitmap computation took: {computingBitmapsStopwatch.ElapsedMilliseconds} ms";
                });
            });


            SchedulingBitmapsDurationLbl.Text = $"Scheduling bitmaps took {schedulingBitmapsStopwatch.ElapsedMilliseconds} ms";
        }

        private static SKPoint GetPixelPointFromComplexNumber(Complex complex, Models.Mandelbrot mandelbrot)
        {
            var realRange = mandelbrot.BottomRight.Re - mandelbrot.TopLeft.Re;
            var realFactor = Math.Abs(complex.Re - mandelbrot.TopLeft.Re) / realRange;
            var px = realFactor * mandelbrot.Width;

            var imaginaryRange = mandelbrot.TopLeft.Im - mandelbrot.BottomRight.Im;
            var imaginaryFactor = Math.Abs(complex.Im - mandelbrot.BottomRight.Im) / imaginaryRange;
            var py = imaginaryFactor * mandelbrot.Height;

            return new SKPoint(px, py);
        }

        public static readonly BindableProperty DesiredComplexPlaneAreaProperty = BindableProperty.Create(
            "DesiredComplexPlaneArea",
            typeof(ComplexPlaneArea),
            typeof(MainPage),
            ComplexPlaneArea.None,
            BindingMode.TwoWay,
            propertyChanged: OnCanvasInfoChanged
        );

        private static void OnCanvasInfoChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            Console.WriteLine($"New ComplexPlaneArea: {newvalue}");
        }

        public ComplexPlaneArea DesiredComplexPlaneArea
        {
            get => (ComplexPlaneArea) GetValue(DesiredComplexPlaneAreaProperty);
            set => SetValue(DesiredComplexPlaneAreaProperty, value);
        }


        private void SKCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var surface = e.Surface;
            var canvas = surface.Canvas;
            var info = e.Info;

            DesiredComplexPlaneArea = new ComplexPlaneArea
            {
                MinRe = -2f,
                MaxRe = 1f,
                MinIm = -1f,
                MaxIm = 1f,
                StepSize = 0.001f
            };

            canvas.Clear();

            if (_mandelbrotBitmaps?.Any() != true)
            {
                return;
            }

            // Draw each bitmap on top of the next one. Also scaled.
            var scalingFactor = Math.Min(((float) info.Width) / _mandelbrotBitmaps.First().Width, ((float) info.Height) / _mandelbrotBitmaps.First().Height);

            var scaledBitmapWidth = _mandelbrotBitmaps.First().Width * scalingFactor;
            var scaledBitmapHeight = _mandelbrotBitmaps.First().Height * scalingFactor;

            var imgTop = (info.Height - scaledBitmapHeight) / 2.0f;
            var imgLeft = (info.Width - scaledBitmapWidth) / 2.0f;

            foreach (var skBitmap in _mandelbrotBitmaps)
            {
                var dest = new SKRect(imgLeft,
                    imgTop,
                    imgLeft + scaledBitmapWidth,
                    imgTop + scaledBitmapHeight);
                canvas.DrawImage(SKImage.FromBitmap(skBitmap), dest, _bitmapPaint);
            }

            LastDrawDurationLbl.Text = $"last draw took {stopwatch.ElapsedMilliseconds} ms";
        }
    }
}