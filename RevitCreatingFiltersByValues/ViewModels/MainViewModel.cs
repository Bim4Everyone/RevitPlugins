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

using Ninject.Activation;

using RevitCreatingFiltersByValues.Models;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;
using Color = Autodesk.Revit.DB.Color;

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _overrideByColor;
        private bool _overrideByPattern;
        private List<ElementId> _filterableCategories;
        private List<string> _selectedCategories = new List<string>();
        private ParametersHelper _selectedFilterableParameter;
        private List<string> _selectedPossibleValues = new List<string>();

        private List<Color> colors = new List<Color>() {
            new Color(255, 0, 0),
            new Color(0, 255, 0),
            new Color(0, 0, 255),
            new Color(255, 255, 0),
            new Color(0, 255, 255),
            new Color(255, 0, 255),
            new Color(255, 0, 128),
            new Color(128, 0, 255),
            new Color(255, 128, 0),
            new Color(64, 128, 128),
            new Color(192, 192, 192),
            new Color(250, 180, 135),
            new Color(250, 180, 135),
            new Color(240, 100, 10),
            new Color(240, 230, 115),
            new Color(0, 128, 0),
            new Color(255, 128, 255),
            new Color(128, 128, 128),
            new Color(255, 120, 150),
            new Color(110, 255, 180),
        };
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
            SolidFillPattern = FillPatternElement.GetFillPatternElementByName(_revitRepository.Document, FillPatternTarget.Drafting, "<Сплошная заливка>");

            foreach(FillPatternElement pattern in _revitRepository.AllPatterns) {
                if(patternNames.Contains(pattern.Name) && pattern.GetFillPattern().Target == FillPatternTarget.Drafting) {
                    Patterns.Add(pattern);
                }
            }

            TaskDialog.Show("Паттерном найдено", Patterns.Count.ToString());

            GetCategoriesInView();




            CreateCommand = new RelayCommand(Create, CanCreate);
            GetFilterableParametersCommand = new RelayCommand(GetFilterableParameters);
            GetPossibleValuesCommand = new RelayCommand(GetPossibleValues);
            SetPossibleValuesCommand = new RelayCommand(SetPossibleValues);
        }





        public ICommand CreateCommand { get; }
        public ICommand GetFilterableParametersCommand { get; }
        public ICommand GetPossibleValuesCommand { get; }
        public ICommand SetPossibleValuesCommand { get; }




        public FillPatternElement SolidFillPattern { get; set; }
        public List<FillPatternElement> Patterns { get; set; } = new List<FillPatternElement>();
        private List<Element> elementsInView { get; set; } = new List<Element>();
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
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
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


            // Получаем возможные значения через выбранный параметр и элементы
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
                        paramValue = GetParamValueFromPramsHelper(elem, SelectedFilterableParameter);
                    }
                } catch(Exception) {
                    continue;
                }

                if(!PossibleValues.Contains(paramValue)) {
                    PossibleValues.Add(paramValue);
                }
            }
        }


        public string GetParamValueFromPramsHelper(Element elem, ParametersHelper parametersHelper) {

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


        private void Create(object p) {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление фильтров")) {

                int i = 0;
                int j = 0;

                TaskDialog.Show("Цвета", colors.Count.ToString());
                TaskDialog.Show("Паттерны", Patterns.Count.ToString());

                foreach(string possibleValue in SelectedPossibleValues) {
                    
                    
                    
                    
                    
                    
                    // Создаем объект фильтра, связанный с определенными категориями
                    ParameterFilterElement parameterFilterElement = ParameterFilterElement
                        .Create(_revitRepository.Document, "$_" + SelectedFilterableParameter.ParamName + "_" + possibleValue, SelectedCatIds);

                    // Создаем правила фильтрации
                    FilterRule filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, possibleValue, true);

                    ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);

                    LogicalAndFilter elemFilter = new LogicalAndFilter(new List<ElementFilter>() { elemParamFilter });

                    // Задаем правила фильтрации объекту фильтра
                    parameterFilterElement.SetElementFilter(elemFilter);

                    // Применяем фильтр на вид
                    View view = _revitRepository.Document.ActiveView;
                    view.AddFilter(parameterFilterElement.Id);


                    OverrideGraphicSettings settings = new OverrideGraphicSettings();
                    if(OverrideByColor) {
                        settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
                        settings.SetSurfaceForegroundPatternColor(colors[i]);

                        settings.SetCutForegroundPatternId(SolidFillPattern.Id);
                        settings.SetCutForegroundPatternColor(colors[i]);
                    }

                    if(OverrideByPattern) {
                        settings.SetSurfaceForegroundPatternId(Patterns[j].Id);

                        settings.SetCutForegroundPatternId(Patterns[j].Id);
                    }

                    view.SetFilterOverrides(parameterFilterElement.Id, settings);

                    i++;
                    j++;
                    if(i > colors.Count) {
                        i = 0;
                    }
                    if(j > Patterns.Count) {
                        j = 0;
                    }
                }

                transaction.Commit();
            }
        }
        private bool CanCreate(object p) {
            return true;
        }

    }
}