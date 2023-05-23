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

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private List<ElementId> _filterableCategories;



        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

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




        private List<Element> elementsInView= new List<Element>();
        public ObservableCollection<ParametersHelper> FilterableParameters { get; set; } = new ObservableCollection<ParametersHelper>();
        public ObservableCollection<string> PossibleValues { get; set; } = new ObservableCollection<string>();
        public List<string> SelectedCategories { get; set; } = new List<string>();
        public List<ElementId> SelectedCatIds { get; set; } = new List<ElementId>();
        public List<string> SelectedPossibleValues { get; set; } = new List<string>();
        public ParametersHelper SelectedFilterableParameter { get; set; }

        public Dictionary<string, CategoryElements> DictCategoryElements { get; set; } = new Dictionary<string, CategoryElements>();



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
                    DictCategoryElements.Add(elemCategoryName, new CategoryElements(catOfElem, elemCategoryId));
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
                    parametersHelper.IsBInParam = true;
                } else {
                    parametersHelper.ParamName = paramAsParameterElement.Name;
                    parametersHelper.ParamElement = paramAsParameterElement;
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
                    if(SelectedFilterableParameter.BInParameter == BuiltInParameter.ALL_MODEL_TYPE_NAME) {
                        paramValue = elem.Name;
                    } else if(SelectedFilterableParameter.BInParameter == BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM) {
                        ElementType type = _revitRepository.Document.GetElement(elem.GetTypeId()) as ElementType;
                        paramValue = type.FamilyName;
                    } else {
                        if(SelectedFilterableParameter.IsBInParam) {
                            Parameter param = elem.get_Parameter(SelectedFilterableParameter.BInParameter);
                            if(param != null) {
                                // Значит параметр на экземпляре
                                paramValue = param.AsValueString();
                            } else {
                                // Значит на типе
                                ElementType elementType = _revitRepository.Document.GetElement(elem.GetTypeId()) as ElementType;
                                if(elementType is null) { continue; }

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
                                if(elementType is null) { continue; }

                                paramValue = elementType.LookupParameter(SelectedFilterableParameter.ParamName).AsValueString();
                            }
                        }
                    }
                } catch(Exception) {
                    continue;
                }

                if(!PossibleValues.Contains(paramValue)) {
                    PossibleValues.Add(paramValue);
                }
            }
        }



        /// <summary>
        /// Переводит выбранные значений для фильтрации в нормальный формат
        /// </summary>
        /// <param name="p"></param>
        private void SetPossibleValues(object p) {
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
            //List<ElementId> categoryIds = new List<ElementId>();
            //categories.Add(new ElementId(BuiltInCategory.OST_Walls));
            List<FilterRule> filterRules = new List<FilterRule>();

            using(Transaction transaction = _revitRepository.StartTransaction("Добавление фильтров")) {

                // Create filter element assocated to the input categories
                ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(_revitRepository.Document, "Example view filter", SelectedCatIds);

                // Criterion 1 - wall type Function is "Exterior"
                ElementId exteriorParamId = new ElementId(BuiltInParameter.FUNCTION_PARAM);
                filterRules.Add(ParameterFilterRuleFactory.CreateEqualsRule(exteriorParamId, (int) WallFunction.Exterior));

                // Criterion 2 - wall height > some number
                ElementId lengthId = new ElementId(BuiltInParameter.CURVE_ELEM_LENGTH);
                filterRules.Add(ParameterFilterRuleFactory.CreateGreaterOrEqualRule(lengthId, 28.0, 0.0001));

            //    // Criterion 3 - custom shared parameter value matches string pattern
            //    // Get the id for the shared parameter - the ElementId is not hardcoded, so we need to get an instance of this type to find it
            //    Guid spGuid = new Guid("96b00b61-7f5a-4f36-a828-5cd07890a02a");
            //    FilteredElementCollector collector = new FilteredElementCollector(doc);
            //    collector.OfClass(typeof(Wall));
            //    Wall wall = collector.FirstElement() as Wall;

            //    if(wall != null) {
            //        Parameter sharedParam = wall.get_Parameter(spGuid);
            //        ElementId sharedParamId = sharedParam.Id;

            //        filterRules.Add(ParameterFilterRuleFactory.CreateBeginsWithRule(sharedParamId, "15."));
            //    }

                ElementFilter elemFilter = CreateElementFilterFromFilterRules(filterRules);
                parameterFilterElement.SetElementFilter(elemFilter);

                // Apply filter to view
                View view = _revitRepository.Document.ActiveView;
                view.AddFilter(parameterFilterElement.Id);
                view.SetFilterVisibility(parameterFilterElement.Id, false);

                transaction.Commit();
            }
        }
        private bool CanCreate(object p) {
            return true;
        }



        private static ElementFilter CreateElementFilterFromFilterRules(IList<FilterRule> filterRules) {
            // We use a LogicalAndFilter containing one ElementParameterFilter
            // for each FilterRule. We could alternatively create a single
            // ElementParameterFilter containing the entire list of FilterRules.
            IList<ElementFilter> elemFilters = new List<ElementFilter>();
            foreach(FilterRule filterRule in filterRules) {
                ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);
                elemFilters.Add(elemParamFilter);
            }
            LogicalAndFilter elemFilter = new LogicalAndFilter(elemFilters);

            return elemFilter;
        }
    }
}