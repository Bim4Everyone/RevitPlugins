using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private ParametersHelper _selectedFilterableParameter;
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


        public Dictionary<string, CategoryElements> DictCategoryElements { get; set; } = new Dictionary<string, CategoryElements>();
        public System.Collections.IList SelectedCategories { get; set; }             // Список категорий, которые выбрал пользователь
        public System.Collections.IList SelectedPossibleValues { get; set; }             // Список категорий, которые выбрал пользователь
        //public List<Category> SelectedCategories { get; set; }             // Список категорий, которые выбрал пользователь




        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public ParametersHelper SelectedFilterableParameter {
            get => _selectedFilterableParameter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterableParameter, value);
        }





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



        // Получение списка возможных параметров фильтрации
        private void GetFilterableParameters(object p) {
            FilterableParameters.Clear();
            PossibleValues.Clear();

            // Забираем список выбранных элементов через CommandParameter
            SelectedCategories = p as System.Collections.IList;

            // Получаем ID категорий для последующего получения параметров фильтрации
            List<ElementId> selectedCatsId = new List<ElementId>();
            foreach(var item in SelectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }

                selectedCatsId.Add(DictCategoryElements[categoryName].CategoryIdInView);
            }


            // Получаем параметры для фильтров на основе ID категорий
            List<ElementId> elementIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, selectedCatsId).ToList();

            foreach(ElementId id in elementIds) {

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



        // Получение списка возможных параметров фильтрации
        private void SetPossibleValues(object p) {
            // Забираем список выбранных значений через CommandParameter
            SelectedPossibleValues = p as System.Collections.IList;

            TaskDialog.Show("ss", SelectedPossibleValues.Count.ToString());
        }





        private void Create(object p) {
            // Забираем список выбранных элементов через CommandParameter
            SelectedCategories = p as System.Collections.IList;

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<ElementId> selectedElements = new List<ElementId>();
            List<ElementId> selectedCatsId = new List<ElementId>();
            foreach(var item in SelectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }


                foreach(Element elem in DictCategoryElements[categoryName].ElementsInView) {
                    selectedElements.Add(elem.Id);
                }



                selectedCatsId.Add(DictCategoryElements[categoryName].CategoryIdInView);
            }



            _revitRepository.ActiveUIDocument.Selection.SetElementIds(selectedElements);


            // Закинуть списки в свойтсва проекат, дописать логику по выведению пользователю возможных значений выбранного параметра

        }
        private bool CanCreate(object p) {
            return true;
        }
    }
}