using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.Views;

namespace RevitAxonometryViews.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly AxonometryConfig _axonometryConfig;

        private readonly List<HvacSystemViewModel> _hvacSystems = new List<HvacSystemViewModel>();
        private List<HvacSystemViewModel> _filteredView = new List<HvacSystemViewModel>();
        private List<string> _filterCriterion = new List<string>();

        private string _filterValue = string.Empty;
        private string _selectedCriteria = string.Empty;
        private string _errorText;
        private bool _useOneView;

        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _axonometryConfig = _revitRepository.AxonometryConfig;
            _hvacSystems = GetHvacSystems();
            _selectedCriteria = _axonometryConfig.SystemName;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(CreateViews, CanCreateViews);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        /// <summary>
        /// Отфильтрованный список систем, выводящийся на ГУИ
        /// </summary>
        public List<HvacSystemViewModel> FilteredView {
            get => _filteredView;
            set => this.RaiseAndSetIfChanged(ref _filteredView, value);
        }

        /// <summary>
        /// С этим свойством связана галочка "Все выделенные на один вид"
        /// </summary>
        public bool UseOneView {
            get => _useOneView;
            set => this.RaiseAndSetIfChanged(ref _useOneView, value);
        }

        /// <summary>
        /// Список критериев для фильтрации
        /// </summary>
        public List<string> FilterCriterion {
            get => _filterCriterion;
            set {
                this.RaiseAndSetIfChanged(ref _filterCriterion, value);
                SelectedCriteria = _filterCriterion[0];
            }
        }

        /// <summary>
        /// Текст, который подаестся в свойство фильтра для вида.
        /// Каждый раз при редактировании обновляет FilteredView
        /// </summary>
        public string FilterValue {
            get => _filterValue;
            set {
                RaiseAndSetIfChanged(ref _filterValue, value);
                UpdateFilteredView();
            }
        }

        /// <summary>
        /// Критерий по которому идет фильтрация/сортировка вида.
        /// Каждый раз при редактировании обновляет FilteredView
        /// </summary>
        public string SelectedCriteria {
            get => _selectedCriteria;
            set {
                RaiseAndSetIfChanged(ref _selectedCriteria, value);
                UpdateFilteredView();
            }
        }

        /// <summary>
        /// Текст ошибки выводимый внизу окна
        /// </summary>
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        /// <summary>
        /// Обновление отсортированного списка для вывода на экран
        /// </summary>
        private void UpdateFilteredView() {
            FilteredView = _hvacSystems.Where(x =>
                (x.SystemName.Contains(FilterValue) || x.SharedName.Contains(FilterValue)))
                    .OrderBy(x => x.SystemName).ToList();
            OnPropertyChanged(nameof(FilteredView));
        }

        /// <summary>
        /// Загрузка данных для вывода в окно через LoadViewCommand
        /// </summary>
        private void LoadView() {
            FilterCriterion = new List<string>() {
                _axonometryConfig.SystemName,
                _axonometryConfig.SharedVisSystemName
            };

            UpdateFilteredView();
        }

        /// <summary>
        /// Создаем коллекцию объектов систем с именами для создания по ним фильтров
        /// </summary>
        public List<HvacSystemViewModel> GetHvacSystems() {
            List<Element> allSystems = _revitRepository.Document.GetElementsByMultiCategory(new List<BuiltInCategory>() {
                BuiltInCategory.OST_DuctSystem,
                BuiltInCategory.OST_PipingSystem });

            List<HvacSystemViewModel> newSystems = new List<HvacSystemViewModel>();

            return new List<HvacSystemViewModel>(
                allSystems.Select(
                    system => new HvacSystemViewModel(system.Name, _revitRepository.GetSharedSystemName(system))));
        }

        /// <summary>
        /// Получаем выбранные объекты из листа и отправляем на создание в репозиторий.
        /// Реализуется через CreateViewsCommand
        /// </summary>
        public void CreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();

            _revitRepository.ExecuteViewCreation(selectedItems,
                new CreationViewRules(
                SelectedCriteria,
                UseOneView,
                _revitRepository));
        }

        /// <summary>
        /// Проверка, выбраны ли системы. Если не выбраны - пишем предупреждение
        /// </summary>
        private bool CanCreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();
            if(selectedItems.Count == 0) {
                ErrorText = "Не выделены системы";
                return false;
            }

            if(!(_revitRepository.ActiveUIDocument.ActiveView is View3D ||
                _revitRepository.ActiveUIDocument.ActiveView is ViewPlan)) {
                ErrorText = "Должен быть активным 2D/3D вид";
                return false;
            }



            ErrorText = string.Empty;
            return true;
        }
    }
}
