using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.Views;

namespace RevitAxonometryViews.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private List<HvacSystemViewModel> _hvacSystems;
        private string _filterValue = string.Empty;
        private string _selectedCriteria = string.Empty;
        private string _errorText;

        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _hvacSystems = _revitRepository.GetHvacSystems();

            FilterCriterion = new ObservableCollection<string>() {
                AxonometryConfig.SystemName,
                AxonometryConfig.FopVisSystemName
            };
            SelectedCriteria = FilterCriterion[0];

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(CreateViews, CanCreateViews);
        }

        public ObservableCollection<string> FilterCriterion { get; set; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand CreateViewsCommand { get; }
        public bool UseFopVisName { get; set; }
        public bool UseOneView { get; set; }

        /// <summary>
        /// Загрузка данных для вывода в окно через LoadViewCommand
        /// </summary>
        private void LoadView() {
            //_hvacSystems = _revitRepository.GetHvacSystems();
            //FilterCriterion = new ObservableCollection<string>() {
            //    AxonometryConfig.SystemName,
            //    AxonometryConfig.FopVisSystemName
            //};
            //SelectedCriteria = FilterCriterion[0];
        }

        /// <summary>
        /// Текст, который подаестся в свойство фильтра для вида.
        /// Каждый раз при редактировании обновляет FilteredView
        /// </summary>
        public string FilterValue {
            get => _filterValue;
            set {
                if(value != _filterValue) {
                    _filterValue = value;
                    RaiseAndSetIfChanged(ref _filterValue, value);
                    OnPropertyChanged(nameof(FilteredView));
                }
            }
        }

        /// <summary>
        /// Критерий по которому идет фильтрация/сортировка вида.
        /// Каждый раз при редактировании обновляет FilteredView
        /// </summary>
        public string SelectedCriteria {
            get => _selectedCriteria;
            set {
                if(value != _selectedCriteria) {
                    _selectedCriteria = value;
                    RaiseAndSetIfChanged(ref _selectedCriteria, value);
                    OnPropertyChanged(nameof(FilteredView));
                }
            }
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public List<HvacSystemViewModel> FilteredView =>
            _hvacSystems.Where(x => LogicalFilterByName(x)).OrderBy(x => LogicalOrderByName(x)).ToList();


        /// <summary>
        /// Логический фильтр для сортировки
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        private string LogicalOrderByName(HvacSystemViewModel system) {
            if(SelectedCriteria == AxonometryConfig.FopVisSystemName) {
                return system.FopName;
            }
            return system.SystemName;
        }

        /// <summary>
        /// Логический фильтр по которому осуществляется фильтрация списка систем, в зависимости от выбранного критерия фильтрации
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        private bool LogicalFilterByName(HvacSystemViewModel system) {
            if(SelectedCriteria == AxonometryConfig.FopVisSystemName) {
                return system.FopName.Contains(FilterValue);
            }
            return system.SystemName.Contains(FilterValue);
        }

        /// <summary>
        /// Получаем выбранные объекты из листа и отправляем на создание в репозиторий. 
        /// Реализуется через CreateViewsCommand
        /// </summary>
        public void CreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();
            _revitRepository.ExecuteViewCreation(selectedItems, UseFopVisName, UseOneView);
        }


        /// <summary>
        /// Проверка, выбраны ли системы. Если не выбраны - пишем предупреждение
        /// </summary>
        /// <returns></returns>
        private bool CanCreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();
            if(selectedItems.Count == 0) {
                ErrorText = "Не выделены системы";
                return false;
            }
            ErrorText = string.Empty;
            return true;
        }
    }
}
