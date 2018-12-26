using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Mandelbrot.Annotations;
using Mandelbrot.Models;
using Mandelbrot.Services;
using Xamarin.Forms;

namespace Mandelbrot.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private List<DisplayPoint> _displayPoints;
        private ICommand _computeDisplayPointsCommand;
        private MandelbrotService _mandelbrotService;
        private CanvasInfo _canvasInfo;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainPageViewModel()
        {
            _computeDisplayPointsCommand = new Command(RefreshDisplayPoints, CanRefreshDisplayPoints);
            _displayPoints = new List<DisplayPoint>();

            _mandelbrotService = new MandelbrotService();
        }

        private bool CanRefreshDisplayPoints()
        {
            return true;
//            return CanvasInfo != null;
        }

        private void RefreshDisplayPoints()
        {
            DisplayPoints.Clear();
            _mandelbrotService.ProduceDisplayPoints(
//                new Range(0, 1200, 0, 800),
//                new Range(0, 1200, 0, 800),
                CanvasInfo.CanvasDimensions,
                CanvasInfo.CanvasPartitionDimentions,
                displayPointsBatch =>
                {
                    DisplayPoints = DisplayPoints.Concat(displayPointsBatch).ToList();
//                    DisplayPoints.AddRange(displayPointsBatch);
//                    OnPropertyChanged(nameof(DisplayPoints));
                });
        }

        public ICommand ComputeDisplayPointsCommand
        {
            get => _computeDisplayPointsCommand;
            set => _computeDisplayPointsCommand = value;
        }

        public CanvasInfo CanvasInfo
        {
            get => _canvasInfo;
            set
            {
                _canvasInfo = value;
                OnPropertyChanged();
            }
        }

        public List<DisplayPoint> DisplayPoints
        {
            get => _displayPoints;
            set
            {
                _displayPoints = value;
                OnPropertyChanged();
            }
        }
    }
}