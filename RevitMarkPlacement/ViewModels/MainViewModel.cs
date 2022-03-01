using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private SampleMode _selectedSampleMode;
        private ObservableCollection<SpotDimentionViewModel> _spotDimetions;
        private int _floorCount;
        private ParameterMode _selectedParameterMode;
        private ObservableCollection<string> _floorParameterNames;
        private string _selectedParameterName;


        public MainViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;

        }

        public int FloorCount {
            get => _floorCount;
            set => this.RaiseAndSetIfChanged(ref _floorCount, value);
        }

        public SampleMode SelectedSampleMode {
            get => _selectedSampleMode;
            set => this.RaiseAndSetIfChanged(ref _selectedSampleMode, value);
        }

        public ParameterMode SelectedParameterMode {
            get => _selectedParameterMode;
            set => this.RaiseAndSetIfChanged(ref _selectedParameterMode, value);
        }

        public string SelectedParameterName { 
            get => _selectedParameterName; 
            set => this.RaiseAndSetIfChanged(ref _selectedParameterName, value); 
        }

        public ObservableCollection<SpotDimentionViewModel> SpotDimetions {
            get => _spotDimetions;
            set => this.RaiseAndSetIfChanged(ref _spotDimetions, value);
        }

        public ObservableCollection<string> FloorParameterNames {
            get => _floorParameterNames;
            set => this.RaiseAndSetIfChanged(ref _floorParameterNames, value);
        }

    }

    internal enum SampleMode {
        [Description("Создать по всему проекту")]
        AllElements,
        [Description("Создать по выбранным элементам")]
        SelectedElements
    }

    internal enum ParameterMode {
        [Description("Индивидульная настройка")]
        Individual,
        [Description("По глобальному параметру")]
        GlobalParameter
    }

    internal class SpotDimentionViewModel : BaseViewModel {
        private bool _isChecked;
        private string _name;

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public string Name { 
            get => _name; 
            set => _name = value; 
        }
    }
}
