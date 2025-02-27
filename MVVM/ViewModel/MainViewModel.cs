using System;
using LamerHelper.Core;

namespace LamerHelper.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand OptimizationViewCommand { get; set; }
        public RelayCommand TweaksViewCommand { get; set; }


        public HomeViewModel HomeVM { get; set; }
        public OptimizationViewModel OptVM { get; set; }
        public TweaksViewModel TweaksVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            OptVM = new OptimizationViewModel();
            TweaksVM = new TweaksViewModel();

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            OptimizationViewCommand = new RelayCommand(o =>
            {
                CurrentView = OptVM;
            });

            TweaksViewCommand = new RelayCommand(o =>
            {
                CurrentView = TweaksVM;
            });
        }
    }
}
