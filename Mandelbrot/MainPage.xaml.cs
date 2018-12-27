using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Mandelbrot.Extensions;
using Mandelbrot.Models;
using Mandelbrot.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Mandelbrot
{
    public partial class MainPage : ContentPage
    {
        private ConcurrentBag<SKBitmap> _mandelbrotBitmaps;
        private ConcurrentDictionary<int, SKColor> _colors;

        public MainPage()
        {
            InitializeComponent();

            this.SetBinding(MainPage.DisplayPointsProperty, nameof(DisplayPoints), BindingMode.OneWay);
            this.SetBinding(MainPage.DesiredComplexPlaneAreaProperty, nameof(DesiredComplexPlaneArea), BindingMode.OneWayToSource);
        }

        public static readonly BindableProperty DisplayPointsProperty = BindableProperty.Create(
            "DisplayPoints",
            typeof(List<MandelbrotPoint>),
            typeof(MainPage),
            null,
            BindingMode.OneWay,
            propertyChanged: OnDisplayPointsChanged
        );

        private static void OnDisplayPointsChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((MainPage) bindable).RefreshMandelbrotCanvas();
        }

        public List<MandelbrotPoint> DisplayPoints
        {
            get => (List<MandelbrotPoint>) GetValue(DisplayPointsProperty);

            set => SetValue(DisplayPointsProperty, value);
        }

        private void RefreshMandelbrotCanvas()
        {
            _mandelbrotBitmaps = new ConcurrentBag<SKBitmap>();
            _colors = new ConcurrentDictionary<int, SKColor>();

            if (DisplayPoints?.Any() != true)
            {
                Device.BeginInvokeOnMainThread(() => { MandelCanvas.InvalidateSurface(); });
                return;
            }

            var schedulingBitmapsStopwatch = new Stopwatch();
            schedulingBitmapsStopwatch.Start();

            var chunksSize = DisplayPoints.Count / Environment.ProcessorCount;
            Task.Run(() =>
            {
                var computingBitmapsStopwatch = new Stopwatch();
                computingBitmapsStopwatch.Start();
                Parallel.ForEach(DisplayPoints.GetListChunks(chunksSize), pointsChunk =>
                {
                    var currentBitmap = new SKBitmap((int) MandelCanvas.CanvasSize.Width, (int) MandelCanvas.CanvasSize.Height, false);

                    using (var bitmapCanvas = new SKCanvas(currentBitmap))
                    {
                        for (var i = 0; i < pointsChunk.Count; i++)
                        {
                            var displayPoint = pointsChunk[i];
                            var h = ((float) displayPoint.Iterations / PureMandelbrotService.MaxIterations);
                            var s = 1 / h;
                            var l = 1 / h;
                            var color = _colors.GetOrAdd(displayPoint.Iterations, SKColor.FromHsl(h, s, l));

                            // Map from complex numbers to device pixels
                            var pixelPoint = GetPixelPointFromComplexNumber(displayPoint.Complex, MandelCanvas.CanvasSize);
                            bitmapCanvas.DrawPoint(pixelPoint, color);
                        }
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

        private static SKPoint GetPixelPointFromComplexNumber(Complex complex, SKSize canvasSize)
        {
            var realRange = 1.0f - (-2.5f);
            var realFactor = Math.Abs(complex.Re - (-2.5f)) / realRange;
            var px = realFactor * canvasSize.Width;

            var imaginaryRange = 1.0f - (-1.0f);
            var imaginaryFactor = Math.Abs(complex.Im - (-1.0f)) / imaginaryRange;
            var py = imaginaryFactor * canvasSize.Height;

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
                MinRe = -2.5f,
                MaxRe = 1.0f,
                MinIm = -1.0f,
                MaxIm = 1.0f
            };

            canvas.Clear();
            canvas.Translate(info.Width / 2.0f, info.Height / 2.0f);

            if (_mandelbrotBitmaps?.Any() != true)
            {
                return;
            }

            // Draw each bitmap on top of the next one. Also scaled.
            var scalingFactor = Math.Min(_mandelbrotBitmaps.First().Width / info.Width, _mandelbrotBitmaps.First().Height / info.Height);


            foreach (var skBitmap in _mandelbrotBitmaps)
            {
                var dest = new SKRect(-info.Width * scalingFactor / 2.0f,
                    -info.Height * scalingFactor / 2.0f,
                    info.Width * scalingFactor / 2.0f,
                    info.Height * scalingFactor / 2.0f);
                canvas.DrawBitmap(skBitmap, dest);
            }

            LastDrawDurationLbl.Text = $"last draw took {stopwatch.ElapsedMilliseconds} ms";
        }
    }
}