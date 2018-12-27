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
        Random rand = new Random();

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

            if (DisplayPoints?.Any() != true)
            {
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var chunksSize = DisplayPoints.Count / Environment.ProcessorCount;
            Task.Run(() =>
            {
                Parallel.ForEach(DisplayPoints.GetChunks(chunksSize), pointsChunk =>
                {
                    var currentBitmap = new SKBitmap(CanvasInfo.CanvasDimensions.Width, CanvasInfo.CanvasDimensions.Height, false);

                    using (var bitmapCanvas = new SKCanvas(currentBitmap))
                    {
                        foreach (var displayPoint in pointsChunk)
                        {
                            var color = SKColors.Black;
                            if (displayPoint.DistanceToCenter >= 2 * 2)
                            {
                                var r = displayPoint.Iterations % 256;
                                var g = (displayPoint.Iterations * 2) % 256;
                                var b = (displayPoint.Iterations * 3) % 256;
                                color = Color.FromRgb(r, g, b).ToSKColor();
                            }

                            bitmapCanvas.DrawPoint(displayPoint.Coordinates.X, displayPoint.Coordinates.Y, color);
                        }
                    }

                    _mandelbrotBitmaps.Add(currentBitmap);

                    Device.BeginInvokeOnMainThread(() => { MandelCanvas.InvalidateSurface(); });
                });
            });


            SchedulingBitmapsDurationLbl.Text = $"Scheduling bitmaps took {stopwatch.ElapsedMilliseconds} ms";
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