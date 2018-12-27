using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Mandelbrot.Abstractions;
using Mandelbrot.Annotations;
using Mandelbrot.Models;
using Xamarin.Forms;

namespace Mandelbrot.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private List<MandelbrotPoint> _displayPoints;

        private ComplexPlaneArea _desiredComplexPlaneArea;
        private readonly IMandelbrotService _mandelbrotService;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainPageViewModel()
        {
            ComputeDisplayPointsCommand = new Command(RefreshDisplayPoints, CanRefreshDisplayPoints);
            _displayPoints = new List<MandelbrotPoint>();

            _mandelbrotService = DependencyService.Resolve<IMandelbrotService>();
        }

        private bool CanRefreshDisplayPoints()
        {
            return !DesiredComplexPlaneArea.Equals(ComplexPlaneArea.None);
        }

        private void RefreshDisplayPoints()
        {
            DisplayPoints.Clear();

            Task.Run(async () => { DisplayPoints = await _mandelbrotService.ProduceDisplayPoints(DesiredComplexPlaneArea, 0.001f); });
        }

        public ICommand ComputeDisplayPointsCommand { get; set; }

        public ComplexPlaneArea DesiredComplexPlaneArea
        {
            get => _desiredComplexPlaneArea;
            set
            {
                _desiredComplexPlaneArea = value;
                OnPropertyChanged();
                (ComputeDisplayPointsCommand as Command)?.ChangeCanExecute();
            }
        }

        public List<MandelbrotPoint> DisplayPoints
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