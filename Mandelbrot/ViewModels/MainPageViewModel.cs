using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            ComputeDisplayPointsCommand = new Command(RefreshDisplayPoints, CanRefreshDisplayPoints);
            _displayPoints = new List<DisplayPoint>();

            _mandelbrotService = new MandelbrotService();
        }

        private bool CanRefreshDisplayPoints()
        {
            return CanvasInfo != null;
        }

        private void RefreshDisplayPoints()
        {
            DisplayPoints.Clear();

            Task.Run(async () =>
            {
                await _mandelbrotService.ProduceDisplayPoints(
                    CanvasInfo.CanvasDimensions,
                    CanvasInfo.CanvasPartitionDimentions,
                    displayPointsBatch => { DisplayPoints = DisplayPoints.Concat(displayPointsBatch).ToList(); });
            });
        }

        public ICommand ComputeDisplayPointsCommand { get; set; }

        public CanvasInfo CanvasInfo
        {
            get => _canvasInfo;
            set
            {
                _canvasInfo = value;
                OnPropertyChanged();
                (ComputeDisplayPointsCommand as Command)?.ChangeCanExecute();
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