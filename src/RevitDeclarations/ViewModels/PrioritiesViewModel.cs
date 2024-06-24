using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class PrioritiesViewModel : BaseViewModel {
        private PrioritiesConfig _prioritiesConfig;
        private List<PriorityViewModel> _priorities;

        public PrioritiesViewModel() {
            _prioritiesConfig = new DefaultPrioritiesConfig();

            _priorities = _prioritiesConfig
                .Priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x))
                .ToList();

            SetDefaultConfigCommand = new RelayCommand(SetDefaultConfig);
            ImportConfigCommand = new RelayCommand(ImportConfig);
            ExportConfigCommand = new RelayCommand(ExportConfig);
        }

        public ICommand SetDefaultConfigCommand { get; }
        public ICommand ImportConfigCommand { get; }
        public ICommand ExportConfigCommand { get; }

        public List<PriorityViewModel> PrioritiesVM {
            get => _priorities;
            set => RaiseAndSetIfChanged(ref _priorities, value);
        }

        public PrioritiesConfig PrioritiesConfig => _prioritiesConfig;

        public void SetDefaultConfig(object obj) {
            _prioritiesConfig = new DefaultPrioritiesConfig();
        }

        public void ImportConfig(object obj) {
            _prioritiesConfig = new PrioritiesConfig(PrioritiesVM
                                        .Select(x => x.Priority)
                                        .ToList());
        }

        public void ExportConfig(object obj) {

        }
    }
}
