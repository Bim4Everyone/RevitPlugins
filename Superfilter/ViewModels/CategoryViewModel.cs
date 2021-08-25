using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace Superfilter.ViewModels {
    internal class CategoryViewModel : BaseViewModel {
        private bool _selected;

        public CategoryViewModel(Category category, IEnumerable<Element> elements) {
            Category = category;
            Elements = new ObservableCollection<Element>(elements.OrderBy(item => item.Name));
            Parameters = new ObservableCollection<ParametersViewModel>(Elements.SelectMany(item => item.GetOrderedParameters().Where(item1 => item1.HasValue)).GroupBy(item => item.Definition, new ParameterDefinitionComparer()).Select(item => new ParametersViewModel(item.Key, item)).OrderBy(item => item.Name));
        }

        public Category Category { get; }
        public ObservableCollection<Element> Elements { get; }
        public ObservableCollection<ParametersViewModel> Parameters { get; }

        public string Name {
            get => Category?.Name ?? "Без категории";
        }

        public int Count {
            get => Elements.Count;
        }

        public bool Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }
    }

    internal class ParametersViewModel : BaseViewModel {
        private bool _selected;
        private readonly Definition _definition;

        public ParametersViewModel(Definition definition, IEnumerable<Parameter> parameters) {
            _definition = definition;
            Values = new ObservableCollection<ParameterViewModel>(parameters.GroupBy(item => item, new ParameterValueComparer()).Select(item => new ParameterViewModel(item.Key, item)).OrderBy(item => item.Value));
        }

        public string Name {
            get => $"{_definition?.Name} [{Values.Count}]" ?? $"Без имени [{Values.Count}]";
        }

        public bool Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged(nameof(Selected));

                foreach(ParameterViewModel parameter in Values) {
                    parameter.Selected = value;
                }
            }
        }

        public ObservableCollection<ParameterViewModel> Values { get; }
    }

    internal class ParameterViewModel : BaseViewModel {
        private bool _selected;
        private readonly Parameter _parameter;

        public ParameterViewModel(Parameter parameter, IEnumerable<Parameter> parameters) {
            _parameter = parameter;
            Elements = new ObservableCollection<Element>(parameters.Select(item => item.Element));
        }

        public object Value {
            get {
                try {
                    if(_parameter.StorageType == StorageType.ElementId) {
                        return "Element: " + _parameter.Element.Document.GetElement(_parameter.AsElementId()).Name;
                    }

                    return _parameter.AsObject() ?? "Без значения";
                } catch {
                    return "Без значения";
                }
            }
        }

        public string DisplayValue {
            get {
                try {
                    if(_parameter.StorageType == StorageType.ElementId) {
                        return _parameter.Element.Document.GetElement(_parameter.AsElementId()).Name + $" [{Elements.Count}] Element";
                    }

                    return $"{_parameter.AsValueString()} [{Elements.Count}]" ?? $"Без значения [{Elements.Count}]";
                } catch {
                    return $"Без значения [{Elements.Count}]";
                }
            }
        }

        public bool Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public ObservableCollection<Element> Elements { get; }
    }

    internal class ParameterDefinitionComparer : IEqualityComparer<Definition> {
        public bool Equals(Definition x, Definition y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Definition obj) {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }

    internal class ParameterValueComparer : IEqualityComparer<Parameter> {
        public bool Equals(Parameter x, Parameter y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            if(x.HasValue == y.HasValue) {
                return true;
            }

            if(x.HasValue || y.HasValue) {
                return false;
            }

            if(x.DisplayUnitType != y.DisplayUnitType) {
                return false;
            }

            if(x.StorageType != y.StorageType) {
                return false;
            }

            return x.AsValueString()?.Equals(y.AsValueString(), System.StringComparison.CurrentCultureIgnoreCase) == true;
        }

        public int GetHashCode(Parameter obj) {
            if(obj.HasValue) {
                try {
                    return obj?.AsValueString()?.GetHashCode() ?? 0;
                } catch {
                }
            }

            return "Без значения".GetHashCode();
        }
    }
}
