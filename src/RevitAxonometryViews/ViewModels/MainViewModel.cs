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
        private readonly ViewFactory _viewFactory;
        private readonly CollectorOperator _collectorOperator;
        private readonly AxonometryConfig _axonometryConfig;

        private readonly IReadOnlyCollection<HvacSystemViewModel> _hvacSystems;
        private readonly IReadOnlyCollection<string> _filterCriterion;

        private string _filterValue;
        private string _selectedCriteria;
        private string _errorText;
        private bool _useOneView;

        public MainViewModel(
            RevitRepository revitRepository, 
            ViewFactory viewFactory, 
            CollectorOperator collectorOperator) {
            _revitRepository = revitRepository;
            _viewFactory = viewFactory;
            _collectorOperator = collectorOperator;
            _axonometryConfig = _revitRepository.AxonometryConfig;
            _hvacSystems = GetHvacSystems();
            _selectedCriteria = _axonometryConfig.SystemName;

            _filterValue = string.Empty;
            _selectedCriteria = _axonometryConfig.SharedVisSystemName;
            _filterCriterion = new List<string>() {
                _axonometryConfig.SystemName,
                _axonometryConfig.SharedVisSystemName
            };

            FilteredView = new ObservableCollection<HvacSystemViewModel>(_hvacSystems);
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(CreateViews, CanCreateViews);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        /// <summary>
        /// Отфильтрованный список систем, выводящийся на ГУИ
        /// </summary>
        public ObservableCollection<HvacSystemViewModel> FilteredView {
            get;
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
        public IReadOnlyCollection<string> FilterCriterion => _filterCriterion;

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
        /// Получаем выбранные объекты из листа и отправляем на создание в репозиторий.
        /// Реализуется через CreateViewsCommand
        /// </summary>
        public void CreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();

            _viewFactory.ExecuteViewCreation(selectedItems,
                new CreationViewRules(
                SelectedCriteria,
                UseOneView,
                _revitRepository));
        }

        /// <summary>
        /// Обновление отсортированного списка для вывода на экран
        /// </summary>
        private void UpdateFilteredView() {
            FilteredView.Clear();

            IEnumerable<IGrouping<string, HvacSystemViewModel>> grouped;

            // группировка по выбранному критерию
            if(SelectedCriteria == _axonometryConfig.SystemName) {
                grouped = _hvacSystems
                    .Where(x => x.SystemName?.IndexOf(FilterValue, StringComparison.Ordinal) >= 0)
                    .GroupBy(x => x.SystemName);
            } else {
                grouped = _hvacSystems
                    .Where(x => x.SharedName?.IndexOf(FilterValue, StringComparison.Ordinal) >= 0)
                    .GroupBy(x => x.SharedName);
            }

            foreach(var group in grouped.OrderBy(g => g.Key)) {
                if(SelectedCriteria == _axonometryConfig.SystemName) {
                    string systemName = group.Key;
                    // Убираем из списка имен дубликаты, если больше одного имени - заменяем на <Варианты>
                    string sharedName = group.Select(x => x.SharedName).Distinct().Count() == 1
                        ? group.First().SharedName
                        : "<Варианты>";

                    foreach(var item in group) {
                        item.DisplaySystemName = systemName;
                        item.DisplaySharedName = sharedName;
                    }
                } else {
                    string sharedName = group.Key;
                    string systemName = group.Select(x => x.SystemName).Distinct().Count() == 1
                        ? group.First().SystemName
                        : "<Варианты>";

                    foreach(var item in group) {
                        item.DisplaySharedName = sharedName;
                        item.DisplaySystemName = systemName;
                    }
                }

                // Добавляем только один элемент в итоговый список. Для сгруппированных элементов 
                // это поможет избежать ситуаций, где создается 50 видов и 50 фильтров
                FilteredView.Add(group.First());
            }
        }

        /// <summary>
        /// Загрузка данных для вывода в окно через LoadViewCommand
        /// </summary>
        private void LoadView() {

            UpdateFilteredView();
        }

        /// <summary>
        /// Создаем коллекцию объектов систем с именами для создания по ним фильтров
        /// </summary>
        private IReadOnlyCollection<HvacSystemViewModel> GetHvacSystems() {
            IList<Element> allSystems = _collectorOperator.GetElementsByMultiCategory(_revitRepository.Document, new List<BuiltInCategory>() {
                BuiltInCategory.OST_DuctSystem,
                BuiltInCategory.OST_PipingSystem });

            List<HvacSystemViewModel> newSystems = new List<HvacSystemViewModel>();

            return new List<HvacSystemViewModel>(
                allSystems.Select(
                    system => new HvacSystemViewModel(system.Name, _revitRepository.GetSharedSystemName((MEPSystem)system))));
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

            ErrorText = string.Empty;
            return true;
        }
    }
}
