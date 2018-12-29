using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FractalSharp.Abstractions;
using FractalSharp.Models;
using Xamarin.Forms;

namespace FractalSharp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Models.Mandelbrot _mandelbrot;

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

            _mandelbrotService = DependencyService.Resolve<IMandelbrotService>();
        }

        private bool CanRefreshDisplayPoints()
        {
            return !DesiredComplexPlaneArea.Equals(ComplexPlaneArea.None);
        }

        private void RefreshDisplayPoints()
        {
            Mandelbrot = null;

            Task.Run(async () => { Mandelbrot = await _mandelbrotService.ProduceDisplayPoints(DesiredComplexPlaneArea); });
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

        public Models.Mandelbrot Mandelbrot
        {
            get => _mandelbrot;
            set
            {
                _mandelbrot = value;
                OnPropertyChanged();
            }
        }
    }
}