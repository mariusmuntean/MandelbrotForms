using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        public MainPage()
        {
            InitializeComponent();

            this.SetBinding(MainPage.DisplayPointsProperty, nameof(DisplayPoints), BindingMode.OneWay);
            this.SetBinding(MainPage.CanvasInfoProperty, nameof(CanvasInfo), BindingMode.OneWayToSource);
        }

        public static readonly BindableProperty DisplayPointsProperty = BindableProperty.Create(
            "DisplayPoints",
            typeof(List<DisplayPoint>),
            typeof(MainPage),
            null,
            BindingMode.OneWay,
            propertyChanged: OnDisplayPointsChanged
        );

        private static void OnDisplayPointsChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((MainPage) bindable).RefreshMandelbrotCanvas();
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
                Parallel.ForEach(DisplayPoints.GetChunks(chunksSize), pointsChunk =>
                {
                    var currentBitmap = new SKBitmap(CanvasInfo.CanvasDimensions.Width, CanvasInfo.CanvasDimensions.Height, false);

                    using (var bitmapCanvas = new SKCanvas(currentBitmap))
                    {
                        foreach (var displayPoint in pointsChunk)
                        {
                            var h = ((float) displayPoint.Iterations / MandelbrotService.MaxIterations);
                            var s = 1 / h;
                            var l = 1 / h;
                            var color = _colors.GetOrAdd(displayPoint.Iterations, SKColor.FromHsl(h, s, l));
                            bitmapCanvas.DrawPoint(displayPoint.Coordinates.X, displayPoint.Coordinates.Y, color);
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


        public List<DisplayPoint> DisplayPoints
        {
            get => (List<DisplayPoint>) GetValue(DisplayPointsProperty);

            set => SetValue(DisplayPointsProperty, value);
        }

        public static readonly BindableProperty CanvasInfoProperty = BindableProperty.Create(
            "CanvasInfo",
            typeof(CanvasInfo),
            typeof(MainPage),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnCanvasInfoChanged
        );

        private ConcurrentBag<SKBitmap> _mandelbrotBitmaps;
        private ConcurrentDictionary<int, SKColor> _colors;

        private static void OnCanvasInfoChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            Console.WriteLine($"New CanvasInfo: {newvalue}");
        }

        public CanvasInfo CanvasInfo
        {
            get => (CanvasInfo) GetValue(CanvasInfoProperty);
            set => SetValue(CanvasInfoProperty, value);
        }


        private void SKCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var surface = e.Surface;
            var canvas = surface.Canvas;
            var info = e.Info;

            CanvasInfo = new CanvasInfo
            {
                CanvasDimensions = new Range(0, info.Width, 0, info.Height),
                CanvasPartitionDimentions = new Range(0, info.Width, 0, info.Height)
            };

            canvas.Clear();
            canvas.Translate(info.Width / 2.0f, info.Height / 2.0f);

            if (_mandelbrotBitmaps?.Any() != true)
            {
                return;
            }

            foreach (var skBitmap in _mandelbrotBitmaps)
            {
                canvas.DrawBitmap(skBitmap, -info.Width / 2.0f, -info.Height / 2.0f);
            }

            LastDrawDurationLbl.Text = $"last draw took {stopwatch.ElapsedMilliseconds} ms";
        }
    }
}