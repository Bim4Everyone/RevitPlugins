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
    /// Модель представления окна для просмотра входящих заданий на отверстия от инженера в файле архитектора или конструктора
    /// </summary>
    internal class ArchitectureNavigatorForIncomingTasksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;


        public ArchitectureNavigatorForIncomingTasksViewModel(
            RevitRepository revitRepository,
            ICollection<OpeningMepTaskIncomingViewModel> openingsMepTasksIncomingViewModels,
            ICollection<OpeningRealArViewModel> openingsRealViewModels) {

            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }
            if(openingsMepTasksIncomingViewModels is null) {
                throw new ArgumentNullException(nameof(openingsMepTasksIncomingViewModels));
            }

            _revitRepository = revitRepository;

            OpeningsMepTaskIncoming = new ObservableCollection<OpeningMepTaskIncomingViewModel>(openingsMepTasksIncomingViewModels);
            OpeningsMepTasksIncomingViewSource = new CollectionViewSource() { Source = OpeningsMepTaskIncoming };

            OpeningsReal = new ObservableCollection<OpeningRealArViewModel>(openingsRealViewModels);
            OpeningsRealViewSource = new CollectionViewSource() { Source = OpeningsReal };

            SelectCommand = RelayCommand.Create<ISelectorAndHighlighter>(SelectElement, CanSelect);
            RenewCommand = RelayCommand.Create(Renew);
            PlaceRealOpeningBySingleTaskCommand = RelayCommand.Create(PlaceRealOpeningBySingleTask);
            PlaceOneRealOpeningByManyTasksCommand = RelayCommand.Create(PlaceOneRealOpeningByManyTasks);
            PlaceManyRealOpeningsByManyTasksCommand = RelayCommand.Create(PlaceManyRealOpeningsByManyTasks);
            PlaceManyRealOpeningsByManyTasksInManyHostsCommand = RelayCommand.Create(PlaceManyRealOpeningsByManyTasksInManyHosts);
        }


        // Входящие задания на отверстия
        public ObservableCollection<OpeningMepTaskIncomingViewModel> OpeningsMepTaskIncoming { get; }

        public CollectionViewSource OpeningsMepTasksIncomingViewSource { get; private set; }

        private OpeningMepTaskIncomingViewModel _selectedOpeningMepTaskIncoming;
        public OpeningMepTaskIncomingViewModel SelectedOpeningMepTaskIncoming {
            get => _selectedOpeningMepTaskIncoming;
            set => RaiseAndSetIfChanged(ref _selectedOpeningMepTaskIncoming, value);
        }


        // Чистовые отверстия из активного документа
        public bool ShowOpeningsReal => OpeningsReal.Count > 0;
        public ObservableCollection<OpeningRealArViewModel> OpeningsReal { get; }

        public CollectionViewSource OpeningsRealViewSource { get; private set; }

        private OpeningRealArViewModel _selectedOpeningReal;
        public OpeningRealArViewModel SelectedOpeningReal {
            get => _selectedOpeningReal;
            set => RaiseAndSetIfChanged(ref _selectedOpeningReal, value);
        }


        public ICommand SelectCommand { get; }

        public ICommand RenewCommand { get; }

        public ICommand PlaceRealOpeningBySingleTaskCommand { get; }

        public ICommand PlaceOneRealOpeningByManyTasksCommand { get; }

        public ICommand PlaceManyRealOpeningsByManyTasksCommand { get; }

        public ICommand PlaceManyRealOpeningsByManyTasksInManyHostsCommand { get; }


        private void SelectElement(ISelectorAndHighlighter famInstanceProvider) {
            _revitRepository.SelectAndShowElement(famInstanceProvider);
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

        private void PlaceRealOpeningBySingleTask() {
            Action action = () => {
                var cmd = new PlaceOneOpeningRealByOneTaskCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }

        private void PlaceOneRealOpeningByManyTasks() {
            Action action = () => {
                var cmd = new PlaceOneOpeningRealByManyTasksCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }

        private void PlaceManyRealOpeningsByManyTasks() {
            Action action = () => {
                var cmd = new PlaceManyOpeningRealsByManyTasksInOneHostCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }

        private void PlaceManyRealOpeningsByManyTasksInManyHosts() {
            Action action = () => {
                var cmd = new PlaceManyOpeningRealsByManyTasksInManyHostsCmd();
                cmd.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}
