using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mandelbrot.Annotations;

namespace Mandelbrot.ViewModels
{
    public class MainPageViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        
        
    }
}