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

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _overrideByColor = true;
        private bool _overrideByPattern;
        private List<ElementId> _filterableCategories;
        private List<string> _selectedCategories = new List<string>();
        private ParametersHelper _selectedFilterableParameter;
        private List<string> _selectedPossibleValues = new List<string>();

        private ColorHelper _selectedColor;
        private PatternsHelper _selectedPattern;
        private List<string> patternNames = new List<string>() {
            "Алюминий",
            "Вертикальный",
            "Горизонтальный",
            "Диагональ вверх",
            "Диагональ вниз",
            "Диагональ крест-накрест",
            "Древесина - Окончательная отделка",
        };


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            GetUserPatterns();
            GetCategoriesInView();


            CreateCommand = new RelayCommand(Create, CanCreate);
            GetFilterableParametersCommand = new RelayCommand(GetFilterableParameters);
            GetPossibleValuesCommand = new RelayCommand(GetPossibleValues);
            SetPossibleValuesCommand = new RelayCommand(SetPossibleValues);

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



        public ICommand CreateCommand { get; }
        public ICommand GetFilterableParametersCommand { get; }
        public ICommand GetPossibleValuesCommand { get; }
        public ICommand SetPossibleValuesCommand { get; }

        public ICommand ChangeColorCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand DeleteColorCommand { get; }
        public ICommand MoveColorUpCommand { get; }
        public ICommand MoveColorDownCommand { get; }

        public ICommand AddPatternCommand { get; }
        public ICommand DeletePatternCommand { get; }
        public ICommand MovePatternUpCommand { get; }
        public ICommand MovePatternDownCommand { get; }


        private List<Element> elementsInView { get; set; } = new List<Element>();
        public FillPatternElement SolidFillPattern { get; set; }
        public ObservableCollection<PatternsHelper> PatternsInPj { get; set; } = new ObservableCollection<PatternsHelper>();
        public List<ParameterFilterElement> AllFiltersInPj { get; set; } = new List<ParameterFilterElement>();
        public List<ParameterFilterElement> AllFiltersInView { get; set; } = new List<ParameterFilterElement>();
        public List<string> AllFilterNamesInPj { get; set; } = new List<string>();
        public List<string> AllFilterNamesInView { get; set; } = new List<string>();
        public Dictionary<string, CategoryElements> DictCategoryElements { get; set; } = new Dictionary<string, CategoryElements>();
        public ObservableCollection<ParametersHelper> FilterableParameters { get; set; } = new ObservableCollection<ParametersHelper>();
        public ObservableCollection<string> PossibleValues { get; set; } = new ObservableCollection<string>();
        public List<ElementId> SelectedCatIds { get; set; } = new List<ElementId>();


        public List<string> SelectedCategories {
            get => _selectedCategories;
            set => this.RaiseAndSetIfChanged(ref _selectedCategories, value);
        }

        public ParametersHelper SelectedFilterableParameter {
            get => _selectedFilterableParameter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterableParameter, value);
        }

        public List<string> SelectedPossibleValues {
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

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }



        /// <summary>
        /// Находим штриховки по именам, указаным польователем
        /// </summary>
        public void GetUserPatterns() {
            SolidFillPattern = FillPatternElement.GetFillPatternElementByName(_revitRepository.Document, FillPatternTarget.Drafting, "<Сплошная заливка>");
            List<FillPatternElement> allDraftingPatterns = _revitRepository.AllDraftingPatterns;

            foreach(FillPatternElement pattern in allDraftingPatterns) {
                if(patternNames.Contains(pattern.Name)) {
                    PatternsInPj.Add(new PatternsHelper(pattern, allDraftingPatterns));
                }
            }
        }


        /// <summary>
        /// Получает категории, представленные на виде + элементы в словаре по ним
        /// </summary>
        public void GetCategoriesInView() {
            elementsInView = _revitRepository.ElementsInView;
            _filterableCategories = _revitRepository.FilterableCategories;


            foreach(Element item in elementsInView) {
                if(item.Category is null) { continue; }

                Category catOfElem = item.Category;
                string elemCategoryName = catOfElem.Name;
                ElementId elemCategoryId = catOfElem.Id;


                // Отсеиваем категории, которые не имеют параметров фильтрации
                if(!_filterableCategories.Contains(elemCategoryId)) { continue; }


                if(DictCategoryElements.ContainsKey(elemCategoryName)) {
                    DictCategoryElements[elemCategoryName].ElementsInView.Add(item);

                } else {
                    DictCategoryElements.Add(elemCategoryName, new CategoryElements(catOfElem, elemCategoryId, new List<Element>() { item }));
                }
            }
        }



        /// <summary>
        /// Получает параметры по которым доступна фильтрация из выбранных категорий
        /// </summary>
        /// <param name="p"></param>
        private void GetFilterableParameters(object p) {
            FilterableParameters.Clear();
            PossibleValues.Clear();
            SelectedCategories.Clear();
            SelectedCatIds.Clear();

            // Забираем список выбранных элементов через CommandParameter
            System.Collections.IList selectedCategories = p as System.Collections.IList;

            // Получаем ID категорий для последующего получения параметров фильтрации
            foreach(var item in selectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }

                SelectedCategories.Add(categoryName);
                SelectedCatIds.Add(DictCategoryElements[categoryName].CategoryIdInView);
            }


            // Получаем параметры для фильтров на основе ID категорий
            List<ElementId> elementIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, SelectedCatIds).ToList();

            foreach(ElementId id in elementIds) {
                // Переводим сначала в ParameterElement, если null, значит это BuiltInParameter
                ParameterElement paramAsParameterElement = _revitRepository.Document.GetElement(id) as ParameterElement;
                
                ParametersHelper parametersHelper = new ParametersHelper();

                // Если он null, значит это встроенный параметр
                if(paramAsParameterElement is null) {
                    BuiltInParameter parameterAsBuiltIn = (BuiltInParameter) Enum.ToObject(typeof(BuiltInParameter), id.IntegerValue);

                    parametersHelper.ParamName = LabelUtils.GetLabelFor(parameterAsBuiltIn);
                    parametersHelper.BInParameter = parameterAsBuiltIn;
                    parametersHelper.Id = id;
                    parametersHelper.IsBInParam = true;
                } else {
                    parametersHelper.ParamName = paramAsParameterElement.Name;
                    parametersHelper.ParamElement = paramAsParameterElement;
                    parametersHelper.Id = id;
                    parametersHelper.IsBInParam = false;
                }

                FilterableParameters.Add(parametersHelper);
            }
        }



        /// <summary>
        /// Получает варианты заполнения выбранного параметра у элементов выбранных категорий
        /// </summary>
        /// <param name="p"></param>
        public void GetPossibleValues(object p) {

            if(SelectedFilterableParameter is null) { return; }
            PossibleValues.Clear();

            List<Element> elementsForWork = new List<Element>();

            // Получаем элементы, которые выбрал пользователь через категории
            foreach(var item in SelectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }

                elementsForWork.AddRange(DictCategoryElements[categoryName].ElementsInView);
            }


            // Получаем возможные значения через выбранные параметр и элементы
            foreach(Element elem in elementsForWork) {

                string paramValue = string.Empty;

                try {
                    // Сначала работаем по исключениям - атрибутам, которые нельзя запросить через параметры
                    if(SelectedFilterableParameter.BInParameter == BuiltInParameter.ALL_MODEL_TYPE_NAME) {
                        paramValue = elem.Name;
                    } else if(SelectedFilterableParameter.BInParameter == BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM) {
                        ElementType type = _revitRepository.Document.GetElement(elem.GetTypeId()) as ElementType;
                        paramValue = type.FamilyName;
                    } else {
                        // Теперь получаем значения через параметры (сначала пробуем на экземпляре, потом на типе)
                        paramValue = GetParamValueFromParamsHelper(elem, SelectedFilterableParameter);
                    }
                } catch(Exception) {
                    continue;
                }

                if(!PossibleValues.Contains(paramValue)) {
                    PossibleValues.Add(paramValue);
                }
            }
        }


        public string GetParamValueFromParamsHelper(Element elem, ParametersHelper parametersHelper) {

            string paramValue = string.Empty;

            if(SelectedFilterableParameter.IsBInParam) {
                Parameter param = elem.get_Parameter(SelectedFilterableParameter.BInParameter);
                if(param != null) {
                    // Значит параметр на экземпляре
                    paramValue = param.AsValueString();
                } else {
                    // Значит на типе
                    ElementType elementType = _revitRepository.Document.GetElement(elem.GetTypeId()) as ElementType;
                    if(elementType is null) { return string.Empty; }

                    paramValue = elementType.get_Parameter(SelectedFilterableParameter.BInParameter).AsValueString();
                }

            } else {
                Parameter param = elem.LookupParameter(SelectedFilterableParameter.ParamName);
                if(param != null) {
                    // Значит параметр на экземпляре
                    paramValue = param.AsValueString();
                } else {
                    // Значит на типе
                    ElementType elementType = _revitRepository.Document.GetElement(elem.GetTypeId()) as ElementType;
                    if(elementType is null) { return string.Empty; }

                    paramValue = elementType.LookupParameter(SelectedFilterableParameter.ParamName).AsValueString();
                }
            }

            return paramValue;
        }



        /// <summary>
        /// Переводит выбранные значений для фильтрации в нормальный формат
        /// </summary>
        /// <param name="p"></param>
        private void SetPossibleValues(object p) {
            SelectedPossibleValues.Clear();

            // Забираем список выбранных значений через CommandParameter
            System.Collections.IList selectedPossibleValues = p as System.Collections.IList;

            // Получаем элементы, которые выбрал пользователь
            foreach(var item in selectedPossibleValues) {
                string possibleValue = item as string;
                if(possibleValue == null) { continue; }

                SelectedPossibleValues.Add(possibleValue);
            }
        }


        /// <summary>
        /// Создает фильтры на виде. Старые временные фильтры удаляет с вида. У существующих в проекте меняет категории
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

                foreach(string possibleValue in SelectedPossibleValues) {

                    string newFilterName = string.Format("${0}_{1}_{2}", userName, SelectedFilterableParameter.ParamName, possibleValue);

                    ParameterFilterElement parameterFilterElement = null;

                    if(AllFilterNamesInPj.Contains(newFilterName)) {

                        // Если создаваемый фильтр уже есть в проекте, то находим его
                        parameterFilterElement =  (from filter in AllFiltersInPj where filter.Name.Equals(newFilterName) select filter).FirstOrDefault();

                        // Назначаем выбранные категории
                        parameterFilterElement.SetCategories(SelectedCatIds);
                    } else {
                        // Если его нет в проекте, то создаем
                        parameterFilterElement = ParameterFilterElement
                            .Create(_revitRepository.Document, newFilterName, SelectedCatIds);

                        // Создаем правила фильтрации
                        FilterRule filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, possibleValue, true);

                        ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);

                        LogicalAndFilter elemFilter = new LogicalAndFilter(new List<ElementFilter>() { elemParamFilter });

                        // Задаем правила фильтрации объекту фильтра
                        parameterFilterElement.SetElementFilter(elemFilter);
                    }

                    // Применяем фильтр на вид
                    View view = _revitRepository.Document.ActiveView;
                    view.AddFilter(parameterFilterElement.Id);

                    OverrideGraphicSettings settings = new OverrideGraphicSettings();
                    // Если пользователь поставил галку перекрашивания
                    if(OverrideByColor) {
                        settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
                        settings.SetSurfaceForegroundPatternColor(Colors[i].UserColor);

                        settings.SetCutForegroundPatternId(SolidFillPattern.Id);
                        settings.SetCutForegroundPatternColor(Colors[i].UserColor);
                    }
                    
                    // Если пользователь поставил галку смены штриховки
                    if(OverrideByPattern) {
                        settings.SetSurfaceForegroundPatternId(PatternsInPj[j].Pattern.Id);

                        settings.SetCutForegroundPatternId(PatternsInPj[j].Pattern.Id);
                    }

                    view.SetFilterOverrides(parameterFilterElement.Id, settings);

                    i++;
                    j++;
                    if(i > Colors.Count - 1) {
                        i = 0;
                    }
                    if(j > PatternsInPj.Count - 1) {
                        j = 0;
                    }
                }

                transaction.Commit();
            }
        }
        private bool CanCreate(object p) {
            return true;
        }


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
        private void AddColor(object p) {
            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {
                Colors.Add(new ColorHelper(colorSelectionDialog.SelectedColor.Red, colorSelectionDialog.SelectedColor.Green, colorSelectionDialog.SelectedColor.Blue));
            }
        }
        private void DeleteColor(object p) {

            Colors.Remove(SelectedColor);
        }

        private void MoveColorUp(object p) {

            int index = Colors.IndexOf(SelectedColor);
            if(index != 0) {
                Colors.Move(index, index - 1);
            }
        }

        private void MoveColorDown(object p) {

            int index = Colors.IndexOf(SelectedColor);
            if(Colors.Count - 1 != index) {
                Colors.Move(index, index + 1);
            }
        }

        private bool CanChangeColor(object p) {
            if(SelectedColor is null) {
                return false;
            }
            return true;
        }

        private void AddPattern(object p) {

            List<FillPatternElement> allDraftingPatterns = _revitRepository.AllDraftingPatterns;

            if(allDraftingPatterns.Count > 0) {
                PatternsInPj.Add(new PatternsHelper(allDraftingPatterns[0], allDraftingPatterns));
            }
        }

        private void DeletePattern(object p) {

            PatternsInPj.Remove(SelectedPattern);
        }
        private void MovePatternUp(object p) {

            int index = PatternsInPj.IndexOf(SelectedPattern);
            if(index != 0) {
                PatternsInPj.Move(index, index - 1);
            }
        }

        private void MovePatternDown(object p) {

            int index = PatternsInPj.IndexOf(SelectedPattern);
            if(PatternsInPj.Count - 1 != index) {
                PatternsInPj.Move(index, index + 1);
            }
        }
        private bool CanChangePattern(object p) {
            if(SelectedPattern is null) {
                return false;
            }
            return true;
        }
    }
}