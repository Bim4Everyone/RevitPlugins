using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreatingFiltersByValues.Models;

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }



        public ObservableCollection<string> FilterableParameters { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> CategoriesInView { get; set; } = new ObservableCollection<string>();

        public Dictionary<string, CategoryElements> DictCategoryElements { get; set; } = new Dictionary<string, CategoryElements>();
        public System.Collections.IList SelectedCategoriesTemp { get; set; }             // Список категорий, которые выбрал пользователь
        public List<Category> SelectedCategories { get; set; }             // Список категорий, которые выбрал пользователь


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            GetCategoriesInView();

            CreateCommand = new RelayCommand(Create, CanCreate);
            GetFilterableParametersCommand = new RelayCommand(GetFilterableParameters);
        }









        public ICommand CreateCommand { get; }
        private void Create(object p) {
            // Забираем список выбранных элементов через CommandParameter
            SelectedCategoriesTemp = p as System.Collections.IList;

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<ElementId> selectedElements = new List<ElementId>();
            List<ElementId> selectedCatsId = new List<ElementId>();
            foreach(var item in SelectedCategoriesTemp) {
                string categoryName = item as string;
                if(categoryName == null) { continue;}


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










        public void GetCategoriesInView() {
            IList<Element> collector = new FilteredElementCollector(_revitRepository.Document, _revitRepository.Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach(var item in collector) {
                Element elem = item as Element;
                if(elem is null || elem.Category is null) { continue; }

                Category catOfElem = elem.Category;
                string elemCategoryName = catOfElem.Name;
                ElementId elemCategoryId = catOfElem.Id;


                // Отсеиваем категории, которые не имеют параметров фильтрации
                if(!_revitRepository.FilterableCategories.Contains(elemCategoryId)) { continue; }



                if(DictCategoryElements.ContainsKey(elemCategoryName)) {
                    DictCategoryElements[elemCategoryName].ElementsInView.Add(elem);

                } else {
                    DictCategoryElements.Add(elemCategoryName, new CategoryElements(catOfElem, elemCategoryId));
                    CategoriesInView.Add(elemCategoryName);
                }
            }
        }



        // Получение списка возможных параметров фильтрации
        public ICommand GetFilterableParametersCommand { get; }
        private void GetFilterableParameters(object p) {
            FilterableParameters.Clear();

            // Забираем список выбранных элементов через CommandParameter
            SelectedCategoriesTemp = p as System.Collections.IList;

            // Получаем ID категорий для последующего получения параметров фильтрации
            List<ElementId> selectedCatsId = new List<ElementId>();
            foreach(var item in SelectedCategoriesTemp) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }


                selectedCatsId.Add(DictCategoryElements[categoryName].CategoryIdInView);
            }


            // Получаем параметры для фильтров на основе ID категорий
            List<ElementId> elementIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, selectedCatsId).ToList();

            foreach(ElementId id in elementIds) {

                ParameterElement paramAsParameterElement = _revitRepository.Document.GetElement(id) as ParameterElement;
                string paramName = string.Empty;
                // Значит он встроенный
                if(paramAsParameterElement is null) {
                    BuiltInParameter parameterAsBuiltIn = (BuiltInParameter) Enum.ToObject(typeof(BuiltInParameter), id.IntegerValue);
                    paramName = LabelUtils.GetLabelFor(parameterAsBuiltIn);
                } else {
                    paramName = paramAsParameterElement.Name;
                }

                FilterableParameters.Add(paramName);
            }
        }






        public void GetPossibleValues() {
            
        }
    }
}