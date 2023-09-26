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

            SelectCommand = new RelayCommand(SelectElement);
            IncomingTaskSelectionChangedCommand = new RelayCommand(IncomingTaskSelectionChanged, CanSelect);
            OpeningRealSelectionChangedCommand = new RelayCommand(OpeningRealSelectionChanged, CanSelect);
            RenewCommand = new RelayCommand(Renew);
            PlaceRealOpeningBySingleTaskCommand = new RelayCommand(PlaceRealOpeningBySingleTask);
            PlaceOneRealOpeningByManyTasksCommand = new RelayCommand(PlaceOneRealOpeningByManyTasks);
            PlaceManyRealOpeningsByManyTasksCommand = new RelayCommand(PlaceManyRealOpeningsByManyTasks);
            PlaceManyRealOpeningsByManyTasksInManyHostsCommand = new RelayCommand(PlaceManyRealOpeningsByManyTasksInManyHosts);
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

        public ICommand IncomingTaskSelectionChangedCommand { get; }

        public ICommand OpeningRealSelectionChangedCommand { get; }

        public ICommand RenewCommand { get; }

        public ICommand PlaceRealOpeningBySingleTaskCommand { get; }

        public ICommand PlaceOneRealOpeningByManyTasksCommand { get; }

        public ICommand PlaceManyRealOpeningsByManyTasksCommand { get; }

        public ICommand PlaceManyRealOpeningsByManyTasksInManyHostsCommand { get; }


        private void SelectElement(object p) {
            if(!(p is ISelectorAndHighlighter famInstanceProvider)) { return; }
            _revitRepository.SelectAndShowElement(famInstanceProvider);
        }

        private void IncomingTaskSelectionChanged(object p) {
            if(OpeningsMepTasksIncomingViewSource.View.CurrentPosition > -1
                && OpeningsMepTasksIncomingViewSource.View.CurrentPosition < OpeningsMepTaskIncoming.Count) {
                SelectElement((OpeningMepTaskIncomingViewModel) p);
            }
        }

        private void OpeningRealSelectionChanged(object p) {
            if(OpeningsRealViewSource.View.CurrentPosition > -1
                && OpeningsRealViewSource.View.CurrentPosition < OpeningsReal.Count) {
                SelectElement((OpeningRealArViewModel) p);
            }
        }

        private bool CanSelect(object p) {
            return p is ISelectorAndHighlighter;
        }

        private void Renew(object p) {
            Action action = () => {
                var command = new GetOpeningTasksCmd();
                command.ExecuteCommand(_revitRepository.UIApplication);
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
