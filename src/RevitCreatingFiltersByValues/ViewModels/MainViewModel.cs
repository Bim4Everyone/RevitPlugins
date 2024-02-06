using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreatingFiltersByValues.Models;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;
using Color = Autodesk.Revit.DB.Color;
using System.Xml.Linq;
using System.Windows.Media;
using System.Windows.Documents;
using System.Security.Cryptography;
using System.Windows.Data;
using System.ComponentModel;

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _overrideByColor = true;
        private bool _overrideByPattern;
        private bool _overridingWithFilters = true;
        private bool _overridingWithRepaint = false;
        private ICollectionView _categoriesView;
        private string _categoriesFilter = string.Empty;
        private ICollectionView _paramsView;
        private string _paramsFilter = string.Empty;
        private ICollectionView _possibleValuesView;
        private string _possibleValuesFilter = string.Empty;
        private ParametersHelper _selectedFilterableParameter;
        private List<PossibleValue> _selectedPossibleValues = new List<PossibleValue>();

        private ColorHelper _selectedColor;
        private PatternsHelper _selectedPattern;
        private List<string> patternNames = new List<string>() {
            "ADSK_Линия_Диагональ_Вверх_2 мм",
            "ADSK_Линия_Диагональ_Вниз_2 мм",
            "ADSK_Линия_Накрест косая_2x2 мм",
            "04_Песок",
            "08.Грунт естественный",
            "ADSK_Грунт_Гравий",
            "ADSK_Древесина_01",
            "ADSK_Древесина_02",
            "ADSK_Формы_Зигзаг_01",
            "ADSK_Формы_Зигзаг_02",
            "ADSK_Формы_Соты",
            "ADSK_Формы_Треугольники",
        };



        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            
            LoadConfig();

            CategoryElements = _revitRepository.GetCategoriesInView(false);
            SolidFillPattern = _revitRepository.SolidFillPattern;
            PatternsInPj = _revitRepository.GetPatternsByNames(patternNames);
            SetCategoriesFilters();

            ClearCategoriesFilterInGUICommand = new RelayCommand(ClearCategoriesFilterInGUI);
            ClearParametersFilterInGUICommand = new RelayCommand(ClearParametersFilterInGUI);
            ClearPossibleValuesFilterInGUICommand = new RelayCommand(ClearPossibleValuesFilterInGUI);
            SelectAllCategoriesInGUICommand = new RelayCommand(SelectAllCategoriesInGUI);
            UnselectAllCategoriesInGUICommand = new RelayCommand(UnselectAllCategoriesInGUI);
            SelectAllValuesInGUICommand = new RelayCommand(SelectAllValuesInGUI);
            UnselectAllValuesInGUICommand = new RelayCommand(UnselectAllValuesInGUI);


            GetFilterableParametersCommand = new RelayCommand(GetFilterableParameters);
            GetPossibleValuesCommand = new RelayCommand(GetPossibleValues);
            CreateCommand = new RelayCommand(Create, CanCreate);

            ChangeColorCommand = new RelayCommand(ChangeColor, CanChangeColor);
            AddColorCommand = new RelayCommand(AddColor);
            DeleteColorCommand = new RelayCommand(DeleteColor, CanChangeColor);
            MoveColorUpCommand = new RelayCommand(MoveColorUp, CanChangeColor);
            MoveColorDownCommand = new RelayCommand(MoveColorDown, CanChangeColor);

            AddPatternCommand = new RelayCommand(AddPattern);
            DeletePatternCommand = new RelayCommand(DeletePattern, CanChangePattern);
            MovePatternUpCommand = new RelayCommand(MovePatternUp, CanChangePattern);
            MovePatternDownCommand = new RelayCommand(MovePatternDown, CanChangePattern);
        }



        public ICommand GetFilterableParametersCommand { get; }
        public ICommand GetPossibleValuesCommand { get; }
        public ICommand CreateCommand { get; }


        public ICommand ClearCategoriesFilterInGUICommand { get; }
        public ICommand ClearParametersFilterInGUICommand { get; }
        public ICommand ClearPossibleValuesFilterInGUICommand { get; }
        public ICommand SelectAllCategoriesInGUICommand { get; }
        public ICommand UnselectAllCategoriesInGUICommand { get; }
        public ICommand SelectAllValuesInGUICommand { get; }
        public ICommand UnselectAllValuesInGUICommand { get; }


        public ICommand ChangeColorCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand DeleteColorCommand { get; }
        public ICommand MoveColorUpCommand { get; }
        public ICommand MoveColorDownCommand { get; }

        public ICommand AddPatternCommand { get; }
        public ICommand DeletePatternCommand { get; }
        public ICommand MovePatternUpCommand { get; }
        public ICommand MovePatternDownCommand { get; }



        public FillPatternElement SolidFillPattern { get; set; }
        public ObservableCollection<PatternsHelper> PatternsInPj { get; set; } = new ObservableCollection<PatternsHelper>();
        public List<ParameterFilterElement> AllFiltersInPj { get; set; } = new List<ParameterFilterElement>();
        public List<string> AllFilterNamesInPj { get; set; } = new List<string>();
        public ObservableCollection<CategoryElements> CategoryElements { get; set; } = new ObservableCollection<CategoryElements>();
        public ObservableCollection<ParametersHelper> FilterableParameters { get; set; } = new ObservableCollection<ParametersHelper>();
        public ObservableCollection<PossibleValue> PossibleValues { get; set; } = new ObservableCollection<PossibleValue>();
        public List<ElementId> SelectedCatIds { get; set; } = new List<ElementId>();
        public List<Element> SelectedElements { get; set; } = new List<Element>();



        public ParametersHelper SelectedFilterableParameter {
            get => _selectedFilterableParameter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterableParameter, value);
        }

        public List<PossibleValue> SelectedPossibleValues {
            get => _selectedPossibleValues;
            set => this.RaiseAndSetIfChanged(ref _selectedPossibleValues, value);
        }
        public bool OverrideByColor {
            get => _overrideByColor;
            set => this.RaiseAndSetIfChanged(ref _overrideByColor, value);
        }
        public bool OverrideByPattern {
            get => _overrideByPattern;
            set => this.RaiseAndSetIfChanged(ref _overrideByPattern, value);
        }

        public bool OverridingWithFilters {
            get => _overridingWithFilters;
            set => this.RaiseAndSetIfChanged(ref _overridingWithFilters, value);
        }

        public bool OverridingWithRepaint {
            get => _overridingWithRepaint;
            set => this.RaiseAndSetIfChanged(ref _overridingWithRepaint, value);
        }

        public ObservableCollection<ColorHelper> Colors { get; set; } = new ObservableCollection<ColorHelper>() {
            new ColorHelper(255, 0, 0),
            new ColorHelper(0, 255, 0),
            new ColorHelper(0, 0, 255),
            new ColorHelper(255, 255, 0),
            new ColorHelper(0, 255, 255),
            new ColorHelper(255, 0, 255),
            new ColorHelper(255, 0, 128),
            new ColorHelper(128, 0, 255),
            new ColorHelper(255, 128, 0),
            new ColorHelper(64, 128, 128),
            new ColorHelper(192, 192, 192),
            new ColorHelper(250, 180, 135),
            new ColorHelper(240, 100, 10),
            new ColorHelper(240, 230, 115),
            new ColorHelper(0, 128, 0),
            new ColorHelper(255, 128, 255),
            new ColorHelper(128, 128, 128),
            new ColorHelper(255, 120, 150),
            new ColorHelper(110, 255, 180)
        };
        public ColorHelper SelectedColor {
            get => _selectedColor;
            set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
        }
        public PatternsHelper SelectedPattern {
            get => _selectedPattern;
            set => this.RaiseAndSetIfChanged(ref _selectedPattern, value);
        }

        /// <summary>
        /// Текстовое поле для привязки к TextBlock GUI фильтра списка категорий
        /// </summary>
        public string CategoriesFilter {
            get => _categoriesFilter;
            set {
                if(value != _categoriesFilter) {
                    _categoriesFilter = value;
                    _categoriesView.Refresh();
                    OnPropertyChanged(nameof(CategoriesFilter));
                }
            }
        }

        /// <summary>
        /// Текстовое поле для привязки к TextBlock GUI фильтра списка параметров
        /// </summary>
        public string ParamsFilter {
            get => _paramsFilter;
            set {
                if(value != _paramsFilter) {
                    _paramsFilter = value;
                    _paramsView.Refresh();
                    OnPropertyChanged(nameof(ParamsFilter));
                }
            }
        }

        /// <summary>
        /// Текстовое поле для привязки к TextBlock GUI фильтра списка возможных значений
        /// </summary>
        public string PossibleValuesFilter {
            get => _possibleValuesFilter;
            set {
                if(value != _possibleValuesFilter) {
                    _possibleValuesFilter = value;
                    _possibleValuesView.Refresh();
                    OnPropertyChanged(nameof(PossibleValuesFilter));
                }
            }
        }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

       

        /// <summary>
        /// Получает параметры по которым доступна фильтрация из выбранных категорий.
        /// Отрабатывает при выборе категорий в списке категорий GUI
        /// </summary>
        /// <param name="p"></param>
        private void GetFilterableParameters(object p) {
            FilterableParameters.Clear();
            PossibleValues.Clear();
            SelectedCatIds.Clear();

            List<CategoryElements> selectedCategories = new List<CategoryElements>();

            foreach(CategoryElements cat in CategoryElements) {
                if(cat.IsCheck) {
                    selectedCategories.Add(cat);
                }
            }


            // Получаем ID категорий для последующего получения параметров фильтрации
            foreach(CategoryElements cat in selectedCategories) {

                SelectedCatIds.Add(cat.CategoryIdInView);
            }


            // Получаем параметры для фильтров на основе ID категорий
            List<ElementId> elementIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, SelectedCatIds).ToList();

            foreach(ElementId id in elementIds) {
                
                ParametersHelper parametersHelper = new ParametersHelper();

                // Проверяем является ли параметр встроенным параметром
                if(id.IsSystemId()) {

                    BuiltInParameter parameterAsBuiltIn = (BuiltInParameter) id.GetIdValue();

                    parametersHelper.ParamName = LabelUtils.GetLabelFor(parameterAsBuiltIn);
                    parametersHelper.BInParameter = parameterAsBuiltIn;
                    parametersHelper.Id = id;
                    parametersHelper.IsBInParam = true;
                } else {
                    ParameterElement paramAsParameterElement = _revitRepository.Document.GetElement(id) as ParameterElement;

                    parametersHelper.ParamName = paramAsParameterElement.Name;
                    parametersHelper.ParamElement = paramAsParameterElement;
                    parametersHelper.Id = id;
                    parametersHelper.IsBInParam = false;
                }

                FilterableParameters.Add(parametersHelper);
            }

            FilterableParameters = new ObservableCollection<ParametersHelper>(FilterableParameters.OrderBy(i => i.ParamName));
            OnPropertyChanged(nameof(FilterableParameters));

            SetParamsFilters();
        }



        /// <summary>
        /// Получает варианты заполнения выбранного параметра у элементов выбранных категорий
        /// Отрабатывает при выборе параметра фильтрации в списке параметров GUI
        /// </summary>
        /// <param name="p"></param>
        public void GetPossibleValues(object p) {

            if(SelectedFilterableParameter is null) { return; }
            PossibleValues.Clear();
            SelectedElements.Clear();

            // Получаем элементы, которые выбрал пользователь через категории
            List<CategoryElements> selectedCategories = new List<CategoryElements>();

            foreach(CategoryElements cat in CategoryElements) {
                if(cat.IsCheck) {
                    SelectedElements.AddRange(cat.ElementsInView);
                }
            }


            // Перебираем выбранные через категории элементы и получаем их значения по выбранному параметру
            foreach(Element elem in SelectedElements) {

                PossibleValue possibleValue = new PossibleValue(elem, SelectedFilterableParameter);
                possibleValue.GetValue();

                if(possibleValue.ValueAsString is null) {
                    continue;
                }

                bool flag = false;
                foreach(PossibleValue pos in PossibleValues) {
                    if(pos.ValueAsString.Equals(possibleValue.ValueAsString)) {
                        pos.ElementsInPj.Add(elem);
                        flag = true;
                        break;
                    }
                }
                if(flag is false) {
                    PossibleValues.Add(possibleValue);
                }
            }

            PossibleValues = new ObservableCollection<PossibleValue>(PossibleValues.OrderBy(i => i.ValueAsString));
            OnPropertyChanged(nameof(PossibleValues));

            SetPossibleValuesFilters();
        }



        /// <summary>
        /// Создает фильтры на виде. Старые временные фильтры удаляет с вида. У существующих в проекте меняет категории
        /// Отрабатывает при нажатии кнопки "Ок" в GUI
        /// </summary>
        /// <param name="p"></param>
        private void Create(object p) {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление фильтров")) {

                int i = 0;
                int j = 0;

                AllFiltersInPj = _revitRepository.AllFilterElements;
                AllFilterNamesInPj = _revitRepository.AllFilterElementNames;
                string userName = _revitRepository.UserName;


                // Удаляем временные фильтры пользователя на виде
                _revitRepository.DeleteTempFiltersInView();

                foreach(PossibleValue pos in PossibleValues) {
                    if(pos.IsCheck is false) { continue; }

                    // Либо создаем фильтры и переопределяем видимость через них
                    if(OverridingWithFilters) {
                        string newFilterName = CorrectName(string.Format("${0}/{1}/{2}", userName, SelectedFilterableParameter.ParamName, pos.ValueAsString));

                        ParameterFilterElement parameterFilterElement = null;

                        if(AllFilterNamesInPj.Contains(newFilterName)) {

                            // Если создаваемый фильтр уже есть в проекте, то находим его
                            parameterFilterElement = (from filter in AllFiltersInPj where filter.Name.Equals(newFilterName) select filter).FirstOrDefault();

                            // Назначаем выбранные категории
                            parameterFilterElement.SetCategories(SelectedCatIds);
                        } else {
                            // Если его нет в проекте, то создаем
                            parameterFilterElement = ParameterFilterElement
                                .Create(_revitRepository.Document, newFilterName, SelectedCatIds);

                            // Создаем правила фильтрации в зависимости от типа данных параметра
                            FilterRule filterRule = null;
                            if(pos.StorageParamType == StorageType.String) {
#if REVIT_2022_OR_LESS
                                filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsString, true);
#else
                                filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsString);
#endif
                            } else if(pos.StorageParamType == StorageType.Double) {
                                filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsDouble, 0.0000001);
                            } else if(pos.StorageParamType == StorageType.ElementId) {
                                filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsElementId);
                            } else if(pos.StorageParamType == StorageType.Integer) {
                                filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsInteger);
                            }

                            if(filterRule is null) { continue; }

                            ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);

                            LogicalAndFilter elemFilter = new LogicalAndFilter(new List<ElementFilter>() { elemParamFilter });

                            // Задаем правила фильтрации объекту фильтра
                            parameterFilterElement.SetElementFilter(elemFilter);
                        }

                        // Применяем фильтр на вид
                        View view = _revitRepository.Document.ActiveView;
                        view.AddFilter(parameterFilterElement.Id);

                        OverrideGraphicSettings settings = GetOverrideGraphicSettings(i, j);

                        view.SetFilterOverrides(parameterFilterElement.Id, settings);
                    } else {
                        // Либо выполняем прямое переопределение графики элементов на виде
                        OverrideGraphicSettings settings = GetOverrideGraphicSettings(i, j);

                        foreach(Element elem in pos.ElementsInPj) {

                            _revitRepository.Document.ActiveView.SetElementOverrides(elem.Id, settings);
                        }
                    }


                    if(++i > Colors.Count - 1) {
                        i = 0;
                    }
                    if(++j > PatternsInPj.Count - 1) {
                        j = 0;
                    }
                }

                transaction.Commit();
            }

            SaveConfig();
        }

        /// <summary>
        /// Корректирует имя фильтра так, чтобы его принял конструктор фильтра.
        /// Заменяет запрещенные символы на нижнее подчеркивание
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string CorrectName(string name) {
            
            if(name.Contains('\\')) { name = name.Replace('\\', '-'); }
            if(name.Contains(':')) { name = name.Replace(':', '-'); }
            if(name.Contains('{')) { name = name.Replace('{', '-'); }
            if(name.Contains('}')) { name = name.Replace('}', '-'); }
            if(name.Contains('[')) { name = name.Replace('[', '-'); }
            if(name.Contains(']')) { name = name.Replace(']', '-'); }
            if(name.Contains('|')) { name = name.Replace('|', '-'); }
            if(name.Contains(';')) { name = name.Replace(';', '-'); }
            if(name.Contains('<')) { name = name.Replace('<', '-'); }
            if(name.Contains('>')) { name = name.Replace('>', '-'); }
            if(name.Contains('?')) { name = name.Replace('?', '-'); }
            if(name.Contains('`')) { name = name.Replace('`', '-'); }
            if(name.Contains('~')) { name = name.Replace('~', '-'); }

            return name;
        }


        private OverrideGraphicSettings GetOverrideGraphicSettings(int c, int p) {
            OverrideGraphicSettings settings = new OverrideGraphicSettings();
            // Если пользователь поставил галку перекрашивания
            if(OverrideByColor) {
                settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
                settings.SetSurfaceForegroundPatternColor(Colors[c].UserColor);

                settings.SetCutForegroundPatternId(SolidFillPattern.Id);
                settings.SetCutForegroundPatternColor(Colors[c].UserColor);
            }

            // Если пользователь поставил галку смены штриховки
            if(OverrideByPattern) {
                settings.SetSurfaceForegroundPatternId(PatternsInPj[p].Pattern.Id);

                settings.SetCutForegroundPatternId(PatternsInPj[p].Pattern.Id);
            }
            return settings;
        }

        private bool CanCreate(object p) {

            bool catsChecked = false;
            foreach(CategoryElements cat in CategoryElements) {
                if(cat.IsCheck) {
                    catsChecked = true;
                    break;
                }
            }
            if(!catsChecked) { ErrorText = "Не выбрана ни одна категория"; return false; }
           
            if(SelectedFilterableParameter is null) { ErrorText = "Не выбран параметр фильтрации"; return false; }

            bool valsChecked = false;
            foreach(PossibleValue pos in PossibleValues) {
                if(pos.IsCheck) {
                    valsChecked = true;
                    break;
                }
            }
            if(!valsChecked) { ErrorText = "Не выбрано ни одного значения для фильтрации"; return false; }

            ErrorText = string.Empty;
            return true;
        }



        /// <summary>
        /// Назначает фильтр привязанный к тексту, через который фильтруется список категорий в GUI
        /// </summary>
        /// <param name="p"></param>
        private void SetCategoriesFilters() {
            // Организуем фильтрацию списка категорий
            _categoriesView = CollectionViewSource.GetDefaultView(CategoryElements);
            _categoriesView.Filter = item => String.IsNullOrEmpty(CategoriesFilter) ? true :
                ((CategoryElements) item).CategoryName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Назначает фильтр привязанный к тексту, через который фильтруется список параметров в GUI
        /// </summary>
        /// <param name="p"></param>
        private void SetParamsFilters() {
            // Организуем фильтрацию списка параметров
            _paramsView = CollectionViewSource.GetDefaultView(FilterableParameters);
            _paramsView.Filter = item => String.IsNullOrEmpty(ParamsFilter) ? true :
                ((ParametersHelper) item).ParamName.IndexOf(ParamsFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Назначает фильтр привязанный к тексту, через который фильтруется список возможных значений в GUI
        /// </summary>
        /// <param name="p"></param>
        private void SetPossibleValuesFilters() {
            // Организуем фильтрацию списка возможных значений
            _possibleValuesView = CollectionViewSource.GetDefaultView(PossibleValues);
            _possibleValuesView.Filter = item => String.IsNullOrEmpty(PossibleValuesFilter) ? true :
                ((PossibleValue) item).ValueAsString.IndexOf(PossibleValuesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }



        /// <summary>
        /// Обнуление строки фильтра привязанного к тексту, через который фильтруется список категорий в GUI
        /// Работает при нажатии "x" в правой части области поиска
        /// </summary>
        /// <param name="p"></param>
        private void ClearCategoriesFilterInGUI(object p) {
            CategoriesFilter = string.Empty;
        }

        /// <summary>
        /// Ставит галку выбора всем категориям элементов, видимых на виде.
        /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка категорий в GUI
        /// </summary>
        /// <param name="p"></param>
        private void SelectAllCategoriesInGUI(object p) {
            foreach(CategoryElements cat in CategoryElements) {
                if(String.IsNullOrEmpty(CategoriesFilter) ? true :
                (cat.CategoryName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    cat.IsCheck = true;
                }
            }
            // Иначе не работало:
            _categoriesView.Refresh();

            GetFilterableParameters(null);
        }


        /// <summary>
        /// Снимает галку выбора у всех категорий элементов, видимых на виде.
        /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка категорий в GUI
        /// </summary>
        /// <param name="p"></param>
        private void UnselectAllCategoriesInGUI(object p) {
            foreach(CategoryElements cat in CategoryElements) {
                if(String.IsNullOrEmpty(CategoriesFilter) ? true :
                (cat.CategoryName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    cat.IsCheck = false;
                }
            }
            // Иначе не работало:
            _categoriesView.Refresh();

            GetFilterableParameters(null);
        }

        /// <summary>
        /// Ставит галку выбора всем видимым возможным значениям в списке возможных значений.
        /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка возможных значений в GUI
        /// </summary>
        /// <param name="p"></param>
        private void SelectAllValuesInGUI(object p) {
            foreach(PossibleValue pos in PossibleValues) {
                if(String.IsNullOrEmpty(PossibleValuesFilter) ? true :
                (pos.ValueAsString.IndexOf(PossibleValuesFilter, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    pos.IsCheck = true;
                }
            }
            // Иначе не работало:
            _possibleValuesView.Refresh();
        }


        /// <summary>
        /// Снимает галку выбора всем видимым возможным значениям в списке возможных значений.
        /// Отрабатывает при нажатии на кнопку "Отменить выбор" возле списка возможных значений в GUI
        /// </summary>
        /// <param name="p"></param>
        private void UnselectAllValuesInGUI(object p) {
            foreach(PossibleValue pos in PossibleValues) {
                if(String.IsNullOrEmpty(PossibleValuesFilter) ? true :
                (pos.ValueAsString.IndexOf(PossibleValuesFilter, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    pos.IsCheck = false;
                }
            }
            // Иначе не работало:
            _possibleValuesView.Refresh();
        }

        /// <summary>
        /// Обнуление строки фильтра привязанного к тексту, через который фильтруется список параметров в GUI
        /// Работает при нажатии "x" в правой части области поиска
        /// </summary>
        /// <param name="p"></param>
        private void ClearParametersFilterInGUI(object p) {
            ParamsFilter = string.Empty;
        }

        /// <summary>
        /// Обнуление строки фильтра привязанного к тексту, через который фильтруется список возможных значений в GUI
        /// Работает при нажатии "x" в правой части области поиска
        /// </summary>
        /// <param name="p"></param>
        private void ClearPossibleValuesFilterInGUI(object p) {
            PossibleValuesFilter = string.Empty;
        }

        /// <summary>
        /// Отработка изменений выбранного цвет в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void ChangeColor(object p) {
                       
            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {

                ColorHelper newColor = new ColorHelper(
                    colorSelectionDialog.SelectedColor.Red,
                    colorSelectionDialog.SelectedColor.Green,
                    colorSelectionDialog.SelectedColor.Blue);

                int index = Colors.IndexOf(SelectedColor);
                Colors[index] = newColor;
            }
        }
        /// <summary>
        /// Добавляет новый цвет в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void AddColor(object p) {
            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {
                Colors.Add(new ColorHelper(colorSelectionDialog.SelectedColor.Red, colorSelectionDialog.SelectedColor.Green, colorSelectionDialog.SelectedColor.Blue));
            }
        }
        /// <summary>
        /// Удаляет выбранный цвет в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void DeleteColor(object p) {

            Colors.Remove(SelectedColor);
        }
        /// <summary>
        /// Перемещает выбранный цвет вверх в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void MoveColorUp(object p) {

            int index = Colors.IndexOf(SelectedColor);
            if(index != 0) {
                Colors.Move(index, index - 1);
            }
        }
        /// <summary>
        /// Перемещает выбранный цвет вниз в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void MoveColorDown(object p) {

            int index = Colors.IndexOf(SelectedColor);
            if(Colors.Count - 1 != index) {
                Colors.Move(index, index + 1);
            }
        }
        /// <summary>
        /// Определяет можно ли изменить цвет
        /// True, если выбран цвет в списке цветов в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private bool CanChangeColor(object p) {
            if(SelectedColor is null) {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Добавляет новую штриховку в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void AddPattern(object p) {

            List<FillPatternElement> allDraftingPatterns = _revitRepository.AllDraftingPatterns;

            if(allDraftingPatterns.Count > 0) {
                PatternsInPj.Add(new PatternsHelper(allDraftingPatterns[0], allDraftingPatterns));
            }
        }
        /// <summary>
        /// Удаляет выбранный цвет в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void DeletePattern(object p) {

            PatternsInPj.Remove(SelectedPattern);
        }
        /// <summary>
        /// Перемещает выбранную штриховку вверх в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void MovePatternUp(object p) {

            int index = PatternsInPj.IndexOf(SelectedPattern);
            if(index != 0) {
                PatternsInPj.Move(index, index - 1);
            }
        }
        /// <summary>
        /// Перемещает выбранную штриховку вниз в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void MovePatternDown(object p) {

            int index = PatternsInPj.IndexOf(SelectedPattern);
            if(PatternsInPj.Count - 1 != index) {
                PatternsInPj.Move(index, index + 1);
            }
        }
        /// <summary>
        /// Определяет можно ли изменить штриховку
        /// True, если выбрана штриховка в списке штриховок в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private bool CanChangePattern(object p) {
            if(SelectedPattern is null) {
                return false;
            }
            return true;
        }


        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting is null) { return; }

            OverrideByPattern = setting.OverrideByPattern;
            OverrideByColor = setting.OverrideByColor;
            OverridingWithFilters = setting.OverridingWithFilters;
            OverridingWithRepaint = setting.OverridingWithRepaint;

            Colors = setting.Colors;
            patternNames = setting.PatternNames;
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting is null) {
                setting = _pluginConfig.AddSettings(_revitRepository.Document);
            }

            setting.OverrideByPattern = OverrideByPattern;
            setting.OverrideByColor = OverrideByColor;
            setting.OverridingWithFilters = OverridingWithFilters;
            setting.OverridingWithRepaint = OverridingWithRepaint;

            setting.Colors = Colors;
            setting.PatternNames = new List<string>(PatternsInPj.Select(item => item.PatternName));


            _pluginConfig.SaveProjectConfig();
        }
    }
}