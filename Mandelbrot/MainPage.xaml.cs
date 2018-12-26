using System;
using System.Collections.Generic;
using System.Linq;
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
            MandelCanvas.InvalidateSurface();
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

            var bitmap = new SKBitmap(info.Width, info.Height);

            var maxIterations = 100;

            if (DisplayPoints?.Any() != true)
            {
                return;
            }

            using (var bitmapCanvas = new SKCanvas(bitmap))
            {
                foreach (var displayPoint in DisplayPoints)
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