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
            ICollection<IOpeningTaskIncomingForKrViewModel> openingsTasksIncomingViewModels,
            ICollection<OpeningRealKrViewModel> openingsRealViewModels) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(openingsTasksIncomingViewModels is null) { throw new ArgumentNullException(nameof(openingsTasksIncomingViewModels)); }
            if(openingsRealViewModels is null) { throw new ArgumentNullException(nameof(openingsRealViewModels)); }

            OpeningsTasksIncoming = new ObservableCollection<IOpeningTaskIncomingForKrViewModel>(openingsTasksIncomingViewModels);
            OpeningsTasksIncomingViewSource = new CollectionViewSource() { Source = OpeningsTasksIncoming };

            OpeningsReal = new ObservableCollection<OpeningRealKrViewModel>(openingsRealViewModels);
            OpeningsRealViewSource = new CollectionViewSource() { Source = OpeningsReal };

            SelectCommand = new RelayCommand(SelectElement);
            IncomingTaskSelectionChangedCommand = new RelayCommand(IncomingTaskSelectionChanged, CanSelect);
            OpeningRealSelectionChangedCommand = new RelayCommand(OpeningRealSelectionChanged, CanSelect);
            RenewCommand = new RelayCommand(Renew);
            PlaceRealOpeningBySingleTaskCommand = new RelayCommand(PlaceRealOpeningBySingleTask);
            PlaceOneRealOpeningByManyTasksCommand = new RelayCommand(PlaceOneRealOpeningByManyTasks);
            PlaceManyRealOpeningsByManyTasksCommand = new RelayCommand(PlaceManyRealOpeningsByManyTasks);
            PlaceManyRealOpeningsByManyTasksInManyHostsCommand = new RelayCommand(PlaceManyRealOpeningsByManyTasksInManyHosts);
        }


        // Входящие задания на отверстия из АР
        public ObservableCollection<IOpeningTaskIncomingForKrViewModel> OpeningsTasksIncoming { get; }

        public CollectionViewSource OpeningsTasksIncomingViewSource { get; private set; }

        private IOpeningTaskIncomingForKrViewModel _selectedOpeningTaskIncoming;

        public IOpeningTaskIncomingForKrViewModel SelectedOpeningTaskIncoming {
            get => _selectedOpeningTaskIncoming;
            set => RaiseAndSetIfChanged(ref _selectedOpeningTaskIncoming, value);
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

        public ICommand PlaceRealOpeningBySingleTaskCommand { get; }

        public ICommand PlaceOneRealOpeningByManyTasksCommand { get; }

        public ICommand PlaceManyRealOpeningsByManyTasksCommand { get; }

        public ICommand PlaceManyRealOpeningsByManyTasksInManyHostsCommand { get; }


        private void SelectElement(object p) {
            if(p is ISelectorAndHighlighter selectorAndHighlighter) {
                _revitRepository.SelectAndShowElement(selectorAndHighlighter);
            }
        }

        private void IncomingTaskSelectionChanged(object p) {
            if(OpeningsTasksIncomingViewSource.View.CurrentPosition > -1
                && OpeningsTasksIncomingViewSource.View.CurrentPosition < OpeningsTasksIncoming.Count) {
                SelectElement((IOpeningTaskIncomingForKrViewModel) p);
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

        private void PlaceRealOpeningBySingleTask(object p) {
            Action action = () => {
                var cmd = new PlaceOneOpeningRealByOneTaskCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }

        private void PlaceOneRealOpeningByManyTasks(object p) {
            Action action = () => {
                var cmd = new PlaceOneOpeningRealByManyTasksCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }

        private void PlaceManyRealOpeningsByManyTasks(object p) {
            Action action = () => {
                var cmd = new PlaceManyOpeningRealsByManyTasksInOneHostCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }

        private void PlaceManyRealOpeningsByManyTasksInManyHosts(object p) {
            Action action = () => {
                var cmd = new PlaceManyOpeningRealsByManyTasksInManyHostsCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}
