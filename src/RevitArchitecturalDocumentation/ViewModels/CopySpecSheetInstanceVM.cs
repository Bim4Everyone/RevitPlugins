using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;
using RevitArchitecturalDocumentation.Views;

using Reference = Autodesk.Revit.DB.Reference;
using Transaction = Autodesk.Revit.DB.Transaction;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class CopySpecSheetInstanceVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ObservableCollection<SheetHelper> _selectedSheets = new ObservableCollection<SheetHelper>();
        private ObservableCollection<SpecHelper> _scheduleSheetInstances = new ObservableCollection<SpecHelper>();
        private List<string> _filterNamesFromSpecs = new List<string>();
        private string _selectedFilterNameForSpecs = string.Empty;

        private string _errorText;

        public CopySpecSheetInstanceVM(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;


            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            SelectSpecsCommand = RelayCommand.Create(SelectSpecs);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand SelectSpecsCommand { get; }


        /// <summary>
        /// Список оболочек над элементами листов, выбранных пользователем до запуска плагина. 
        /// Оболочка имеют функционал по нахождению номера этажа по имени листа
        /// </summary>
        public ObservableCollection<SheetHelper> SelectedSheets {
            get => _selectedSheets;
            set => this.RaiseAndSetIfChanged(ref _selectedSheets, value);
        }

        /// <summary>
        /// Список оболочек над элементами спецификаций, выбранных пользователем.
        /// Оболочка имеют функционал по работе с именем спецификации и ее фильтрами
        /// </summary]
        public ObservableCollection<SpecHelper> ScheduleSheetInstances {
            get => _scheduleSheetInstances;
            set => this.RaiseAndSetIfChanged(ref _scheduleSheetInstances, value);
        }

        /// <summary>
        /// Список имен полей фильтров, которые есть одновременно во всех выбранных спеках
        /// </summary>
        public List<string> FilterNamesFromSpecs {
            get => _filterNamesFromSpecs;
            set => this.RaiseAndSetIfChanged(ref _filterNamesFromSpecs, value);
        }

        /// <summary>
        /// Имя поля фильтра, которое уазал пользователь как то, где прописан этаж
        /// </summary>
        public string SelectedFilterNameForSpecs {
            get => _selectedFilterNameForSpecs;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterNameForSpecs, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }


        /// <summary>
        /// Метод, отрабатывающий при загрузке окна
        /// </summary>
        private void LoadView() {

            LoadConfig();
            GetSelectedSheets();
        }

        /// <summary>
        /// Метод, отрабатывающий при нажатии кнопки "Ок"
        /// </summary>
        private void AcceptView() {

            SaveConfig();
            CopySpecs();
        }

        /// <summary>
        /// Определяет можно ли запустить работу плагина
        /// </summary>
        private bool CanAcceptView() {

            if(SelectedSheets.Count == 0) {
                ErrorText = "Не выбрано ни одного листа";
                return false;
            }


            foreach(SheetHelper sheetHelper in SelectedSheets) {

                // LevelNumberFormat заполняется после последней проверки при получении имени, поэтому, если он не заполнен, значит все есть ошибки, обновляем их
                if(sheetHelper.NameHelper.LevelNumberFormat.Length == 0) {
                    try {
                        // Анализируем и получаем номер одновременно, т.к. чтобы проанализировать номер уровня
                        // нужно получить другую информацию, что по факту равно загрузке при получении уровня
                        sheetHelper.NameHelper.AnalyzeNGetLevelNumber();

                    } catch(ViewNameException ex) {
                        ErrorText = ex.Message;
                        return false;
                    }
                }
            }


            if(ScheduleSheetInstances.Count == 0) {
                ErrorText = "Не выбрано ни одной спецификации на листе";
                return false;
            }


            foreach(SpecHelper specHelper in ScheduleSheetInstances) {

                // LevelNumberFormat заполняется после последней проверки при получении имени, поэтому, если он заполнен, значит все ок
                if(specHelper.NameHelper.LevelNumberFormat.Length == 0) {
                    try {
                        // Анализируем и получаем номер одновременно, т.к. чтобы проанализировать номер уровня
                        // нужно получить другую информацию, что по факту равно загрузке при получении уровня
                        specHelper.NameHelper.AnalyzeNGetLevelNumber();

                    } catch(ViewNameException ex) {
                        ErrorText = ex.Message;
                        return false;
                    }
                }
            }

            if(SelectedFilterNameForSpecs == string.Empty) {
                ErrorText = "Не выбрано поле фильтрации этажа";
                return false;
            }

            ErrorText = string.Empty;
            return true;
        }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        private void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            SelectedFilterNameForSpecs = settings.SelectedFilterNameForSpecs;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.SelectedFilterNameForSpecs = SelectedFilterNameForSpecs;

            _pluginConfig.SaveProjectConfig();
        }


        /// <summary>
        /// Получает список листов, выбранные пользователем до начала работы
        /// </summary>
        private void GetSelectedSheets() {

            foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                ViewSheet sheet = _revitRepository.Document.GetElement(id) as ViewSheet;
                if(sheet != null) {

                    SheetHelper sheetHelper = new SheetHelper(_revitRepository, sheet);
                    try {
                        sheetHelper.NameHelper.AnalyzeNGetLevelNumber();

                    } catch(ViewNameException ex) {
                        ErrorText = ex.Message;
                    }
                    SelectedSheets.Add(sheetHelper);
                }
            }
        }

        /// <summary>
        /// Метод команды по выбору видовых окон спецификаций в пространстве Revit после закрытия окна плагина
        /// </summary>
        private void SelectSpecs() {

            ErrorText = string.Empty;
            ScheduleSheetInstances.Clear();
            ISelectionFilter selectFilter = new ScheduleSelectionFilter();
            IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                            .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");

            foreach(Reference reference in references) {

                ScheduleSheetInstance elem = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                if(elem is null) {
                    continue;
                }

                SpecHelper specHelper = new SpecHelper(_revitRepository, elem);
                ScheduleSheetInstances.Add(specHelper);
                try {
                    specHelper.NameHelper.AnalyzeNGetNameInfo();

                } catch(ViewNameException ex) {
                    ErrorText = ex.Message;
                }
            }
            GetFilterNames();


            CopySpecSheetInstanceV window = new CopySpecSheetInstanceV {
                DataContext = this
            };
            window.ShowDialog();
        }


        /// <summary>
        /// Метод перебирает все выбранные спеки во всех заданиях и собирает список параметров фильтрации, принадлежащий всем одновременно
        /// </summary>
        private void GetFilterNames() {

            FilterNamesFromSpecs.Clear();

            foreach(SpecHelper spec in ScheduleSheetInstances) {
                if(FilterNamesFromSpecs.Count == 0) {
                    FilterNamesFromSpecs.AddRange(spec.GetFilterNames());
                } else {
                    FilterNamesFromSpecs = FilterNamesFromSpecs.Intersect(spec.GetFilterNames()).ToList();
                }
            }
        }



        /// <summary>
        /// Метод, отрабатывающий при запуске плагина в работу. Выполняет копирование спецификаций
        /// </summary>
        private void CopySpecs() {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Копирование спецификаций")) {

                foreach(SheetHelper sheetHelper in SelectedSheets) {

                    foreach(SpecHelper specHelper in ScheduleSheetInstances) {

                        SpecHelper newSpecHelper = specHelper.GetOrDublicateNSetSpec(SelectedFilterNameForSpecs, sheetHelper.NameHelper.LevelNumber);

                        // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование, 
                        newSpecHelper.PlaceSpec(sheetHelper);
                    }
                }

                transaction.Commit();
            }
        }
    }
}
