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
        private List<HvacSystemViewModel> _hvacSystems = new List<HvacSystemViewModel>();
        private List<string> _filterCriterion = new List<string>();
        private string _filterValue = string.Empty;
        private string _selectedCriteria = AxonometryConfig.SystemName;
        private string _errorText;
        private bool _useOneView;
        private readonly ProjectParameters _projectParameters;


        public MainViewModel(RevitRepository revitRepository) {
            

            _revitRepository = revitRepository;
            _hvacSystems = GetHvacSystems();

            _projectParameters = ProjectParameters.Create(_revitRepository.Application);
            //_projectParameters.SetupRevitParam(_revitRepository.Document, SharedParamsConfig.Instance.FinishingRoomName);

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(CreateViews, CanCreateViews);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand CreateViewsCommand { get; }

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
        /// Загрузка данных для вывода в окно через LoadViewCommand
        /// </summary>
        private void LoadView() {
            FilterCriterion = new List<string>() {
                AxonometryConfig.SystemName,
                AxonometryConfig.SharedVisSystemName
            };

            FilteredView = _hvacSystems;
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
        /// <summary>
        /// Текст ошибки выводимый внизу окна
        /// </summary>
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        /// <summary>
        /// Отфильтрованный список систем, выводящийся на ГУИ
        /// </summary>
        public List<HvacSystemViewModel> FilteredView {
            get {
                return _hvacSystems.Where(x =>
                (x.SystemName.Contains(FilterValue) || x.SharedName.Contains(FilterValue)))
                    .OrderBy(x => LogicalOrderByName(x)).ToList();
            }
            set {
                this.RaiseAndSetIfChanged(ref _hvacSystems, value);
            }
        }

        /// <summary>
        /// Логический фильтр для сортировки
        /// </summary>
        private string LogicalOrderByName(HvacSystemViewModel system) {
            if(SelectedCriteria == AxonometryConfig.SharedVisSystemName) {
                return system.SharedName;
            }
            return system.SystemName;
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
                _revitRepository.Document));
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
