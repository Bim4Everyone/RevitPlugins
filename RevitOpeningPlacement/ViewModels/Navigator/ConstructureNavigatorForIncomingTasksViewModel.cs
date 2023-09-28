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
    /// Модель представления окна для просмотра входящих заданий на отверстия от архитектора в файле конструктора
    /// </summary>
    internal class ConstructureNavigatorForIncomingTasksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;


        public ConstructureNavigatorForIncomingTasksViewModel(
            RevitRepository revitRepository,
            ICollection<OpeningArTaskIncomingViewModel> openingsArTasksIncomingViewModels,
            ICollection<OpeningRealKrViewModel> openingsRealViewModels) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(openingsArTasksIncomingViewModels is null) { throw new ArgumentNullException(nameof(openingsArTasksIncomingViewModels)); }
            if(openingsRealViewModels is null) { throw new ArgumentNullException(nameof(openingsRealViewModels)); }

            OpeningsArTasksIncoming = new ObservableCollection<OpeningArTaskIncomingViewModel>(openingsArTasksIncomingViewModels);
            OpeningsArTasksIncomingViewSource = new CollectionViewSource() { Source = OpeningsArTasksIncoming };

            OpeningsReal = new ObservableCollection<OpeningRealKrViewModel>(openingsRealViewModels);
            OpeningsRealViewSource = new CollectionViewSource() { Source = OpeningsReal };

            SelectCommand = new RelayCommand(SelectElement);
            IncomingTaskSelectionChangedCommand = new RelayCommand(IncomingTaskSelectionChanged, CanSelect);
            OpeningRealSelectionChangedCommand = new RelayCommand(OpeningRealSelectionChanged, CanSelect);
            RenewCommand = new RelayCommand(Renew);
        }


        // Входящие задания на отверстия из АР
        public ObservableCollection<OpeningArTaskIncomingViewModel> OpeningsArTasksIncoming { get; }

        public CollectionViewSource OpeningsArTasksIncomingViewSource { get; private set; }

        private OpeningArTaskIncomingViewModel _selectedOpeningArTaskIncoming;

        public OpeningArTaskIncomingViewModel SelectedOpeningArTaskIncoming {
            get => _selectedOpeningArTaskIncoming;
            set => RaiseAndSetIfChanged(ref _selectedOpeningArTaskIncoming, value);
        }


        // Чистовые отверстия из активного документа КР
        public ObservableCollection<OpeningRealKrViewModel> OpeningsReal { get; }

        public bool ShowOpeningsReal => OpeningsReal.Count > 0;

        public CollectionViewSource OpeningsRealViewSource { get; private set; }

        private OpeningRealKrViewModel _selectedOpeningReal;

        public OpeningRealKrViewModel SelectedOpeningReal {
            get => _selectedOpeningReal;
            set => RaiseAndSetIfChanged(ref _selectedOpeningReal, value);
        }


        public ICommand SelectCommand { get; }

        public ICommand RenewCommand { get; }

        public ICommand IncomingTaskSelectionChangedCommand { get; }

        public ICommand OpeningRealSelectionChangedCommand { get; }


        private void SelectElement(object p) {
            if(p is ISelectorAndHighlighter selectorAndHighlighter) {
                _revitRepository.SelectAndShowElement(selectorAndHighlighter);
            }
        }

        private void IncomingTaskSelectionChanged(object p) {
            if(OpeningsArTasksIncomingViewSource.View.CurrentPosition > -1
                && OpeningsArTasksIncomingViewSource.View.CurrentPosition < OpeningsArTasksIncoming.Count) {
                SelectElement((OpeningArTaskIncomingViewModel) p);
            }
        }

        private void OpeningRealSelectionChanged(object p) {
            if(OpeningsRealViewSource.View.CurrentPosition > -1
                && OpeningsRealViewSource.View.CurrentPosition < OpeningsReal.Count) {
                SelectElement((OpeningRealKrViewModel) p);
            }
        }

        private bool CanSelect(object p) {
            return p is ISelectorAndHighlighter;
        }

        private void Renew(object p) {
            Action action = () => {
                var cmd = new GetOpeningTasksCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}
