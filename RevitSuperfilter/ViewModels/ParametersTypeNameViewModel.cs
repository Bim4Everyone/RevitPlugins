using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels {
    internal class ParametersTypeNameViewModel : SelectableObjectViewModel<Category>, IParametersViewModel {
        public ParametersTypeNameViewModel(Category category, IEnumerable<ElementType> elementTypes)
            : base(category) {
            ElementTypes = new ObservableCollection<ElementType>(elementTypes);
            Values = new ObservableCollection<IParameterViewModel>(GetParamsViewModel());
            foreach(IParameterViewModel item in Values) {
                item.PropertyChanged += ParametersViewModelPropertyChanged;
            }
        }

        public override string DisplayData => LabelUtils.GetLabelFor(BuiltInParameter.SYMBOL_NAME_PARAM);
        public int Count => Values.Count;
        public ObservableCollection<ElementType> ElementTypes { get; }
        public ObservableCollection<IParameterViewModel> Values { get; }

        public override bool? IsSelected {
            get => base.IsSelected;
            set {
                base.IsSelected = value;
                if(base.IsSelected.HasValue) {
                    foreach(IParameterViewModel item in Values) {
                        item.PropertyChanged -= ParametersViewModelPropertyChanged;
                        item.IsSelected = base.IsSelected;
                        item.PropertyChanged += ParametersViewModelPropertyChanged;
                    }
                }
            }
        }

        private void ParametersViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName.Equals(nameof(IParameterViewModel.IsSelected))) {
                UpdateSelection(Values);
            }
        }

        private IEnumerable<IParameterViewModel> GetParamsViewModel() {
            return ElementTypes
                .GroupBy(item => item, new Models.ElementTypeNameComparer())
                .Select(item => new ParameterTypeNameViewModel(item.Key, item))
                .OrderBy(item => item.DisplayData);
        }

        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true || IsSelected == null) {
                return Values.SelectMany(item => item.GetSelectedElements());
            }

            return Enumerable.Empty<Element>();
        }
    }
}
