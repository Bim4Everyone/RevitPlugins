using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления окна для просмотра входящих заданий на отверстия от инженера в файле архитектора или конструктора
    /// </summary>
    internal class OpeningsMepTaskIncomingViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private OpeningMepTaskIncomingViewModel _selectedOpeningMepTaskIncoming;


        public OpeningsMepTaskIncomingViewModel(RevitRepository revitRepository, ICollection<OpeningMepTaskIncomingViewModel> openingsMepTasksIncoming) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }
            if(openingsMepTasksIncoming is null) {
                throw new ArgumentNullException(nameof(openingsMepTasksIncoming));
            }

            _revitRepository = revitRepository;

            OpeningsMepTaskIncoming = new ObservableCollection<OpeningMepTaskIncomingViewModel>(openingsMepTasksIncoming);
            OpeningsMepTasksIncomingViewSource = new CollectionViewSource() { Source = OpeningsMepTaskIncoming };

            SelectCommand = new RelayCommand(SelectElement);
            SelectionChangedCommand = new RelayCommand(SelectionChanged, CanSelect);
            RenewCommand = new RelayCommand(Renew);
        }


        public ObservableCollection<OpeningMepTaskIncomingViewModel> OpeningsMepTaskIncoming { get; }

        public CollectionViewSource OpeningsMepTasksIncomingViewSource { get; private set; }

        public OpeningMepTaskIncomingViewModel SelectedOpeningMepTaskIncoming {
            get => _selectedOpeningMepTaskIncoming;
            set => RaiseAndSetIfChanged(ref _selectedOpeningMepTaskIncoming, value);
        }


        public ICommand SelectCommand { get; }

        public ICommand SelectionChangedCommand { get; }

        public ICommand RenewCommand { get; }


        private void SelectElement(object p) {
            if(!(p is OpeningMepTaskIncomingViewModel opening)) { return; }
            var elements = new Element[] { opening.GetFamilyInstance() };
            _revitRepository.SelectAndShowElement(elements);
        }

        private void SelectionChanged(object p) {
            if(OpeningsMepTasksIncomingViewSource.View.CurrentPosition > -1
                && OpeningsMepTasksIncomingViewSource.View.CurrentPosition < OpeningsMepTaskIncoming.Count) {
                SelectElement((OpeningMepTaskIncomingViewModel) p);
            }
        }

        private bool CanSelect(object p) {
            return p is OpeningMepTaskIncomingViewModel;
        }

        private void Renew(object p) {
            Action action = () => {
                var command = new GetOpeningTaskCommand();
                command.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}
