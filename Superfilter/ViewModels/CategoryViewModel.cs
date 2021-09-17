using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;
using dosymep.WPF.ViewModels;

using Superfilter.Models;

namespace Superfilter.ViewModels {
    internal class CategoryViewModel : SelectableObjectViewModel<Category> {
        private ObservableCollection<ParametersViewModel> parameters;

        public CategoryViewModel(Category category, IEnumerable<Element> elements)
            : base(category) {
            Category = category;
            Elements = new ObservableCollection<Element>(elements);
        }

        public Category Category { get; }
        public ObservableCollection<Element> Elements { get; }

        public ObservableCollection<ParametersViewModel> Parameters {
            get {
                if(parameters == null) {
                    parameters = new ObservableCollection<ParametersViewModel>(GetParamsViewModel());
                }

                return parameters;
            }
        }

        public override string DisplayData {
            get => Category?.Name ?? "Без категории";
        }

        public int Count {
            get => Elements.Count;
        }

        private IEnumerable<ParametersViewModel> GetParamsViewModel() {
            return Elements
                .SelectMany(element => element.GetOrderedParameters().Where(param => param.HasValue))
                .GroupBy(param => param, new ParamComparer())
                .Select(param => new ParametersViewModel(param.Key.Definition, param))
                .OrderBy(param => param.DisplayData);
        }
    }
}
