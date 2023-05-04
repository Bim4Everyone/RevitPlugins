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


        public ObservableCollection<string> CategoriesInView { get; set; } = new ObservableCollection<string>();
        public Dictionary<string, CategoryElements> DictCategoryElements { get; set; } = new Dictionary<string, CategoryElements>();
        public System.Collections.IList SelectedCategories { get; set; }             // Список меток основ, которые выбрал пользователь


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            GetCategoriesInView();

            CreateCommand = new RelayCommand(Create, CanCreate);
        }




        public ICommand CreateCommand { get; }
        private void Create(object p) {
            // Забираем список выбранных элементов через CommandParameter
            SelectedCategories = p as System.Collections.IList;

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<ElementId> selectedElements = new List<ElementId>();
            foreach(var item in SelectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue;}


                foreach(Element elem in DictCategoryElements[categoryName].ElementsInView) {
                    selectedElements.Add(elem.Id);
                }
            }




            _revitRepository.ActiveUIDocument.Selection.SetElementIds(selectedElements);

        }

        public void GetCategoriesInView() {
            IList<Element> collector = new FilteredElementCollector(_revitRepository.Document, _revitRepository.Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach(var item in collector) {
                Element elem = item as Element;
                if(elem is null || elem.Category is null) { continue; }

                string elemCategoryName = elem.Category.Name;


                if(DictCategoryElements.ContainsKey(elemCategoryName)) {
                    DictCategoryElements[elemCategoryName].ElementsInView.Add(elem);

                } else {
                    DictCategoryElements.Add(elemCategoryName, new CategoryElements(elem.Category));
                    CategoriesInView.Add(elemCategoryName);
                }
            }
        }



        private bool CanCreate(object p) {
            return true;
        }
    }
}