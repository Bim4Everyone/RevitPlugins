using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления окна для просмотра исходящих заданий на отверстия в файле инженера
    /// </summary>
    internal class MepNavigatorForOutcomingTasksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private OpeningMepTaskOutcomingViewModel _selectedOpeningMepTaskOutcoming;


        public MepNavigatorForOutcomingTasksViewModel(RevitRepository revitRepository, ICollection<OpeningMepTaskOutcomingViewModel> openingsMepTasksOutcoming) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }
            if(openingsMepTasksOutcoming is null) {
                throw new ArgumentNullException(nameof(openingsMepTasksOutcoming));
            }

            _revitRepository = revitRepository;

            OpeningsMepTaskOutcoming = new ObservableCollection<OpeningMepTaskOutcomingViewModel>(openingsMepTasksOutcoming);
            OpeningsMepTasksOutcomingViewSource = new CollectionViewSource() { Source = OpeningsMepTaskOutcoming };

            SelectCommand = RelayCommand.Create<ISelectorAndHighlighter>(SelectElement, CanSelect);
            RenewCommand = RelayCommand.Create(Renew);
        }


        public ObservableCollection<OpeningMepTaskOutcomingViewModel> OpeningsMepTaskOutcoming { get; }

        public CollectionViewSource OpeningsMepTasksOutcomingViewSource { get; private set; }

        public OpeningMepTaskOutcomingViewModel SelectedOpeningMepTaskOutcoming {
            get => _selectedOpeningMepTaskOutcoming;
            set => RaiseAndSetIfChanged(ref _selectedOpeningMepTaskOutcoming, value);
        }


        public ICommand SelectCommand { get; }

        public ICommand RenewCommand { get; }


        private void SelectElement(ISelectorAndHighlighter p) {
            _revitRepository.SelectAndShowElement(p);
        }

        private bool CanSelect(ISelectorAndHighlighter p) {
            return p != null;
        }

        private void Renew() {
            Action action = () => {
                var command = new GetOpeningTasksCmd();
                command.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}
