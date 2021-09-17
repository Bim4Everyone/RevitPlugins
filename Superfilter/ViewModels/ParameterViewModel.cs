using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace Superfilter.ViewModels {
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
                        return _parameter.Element.Document.GetElement(_parameter.AsElementId())?.Name ?? $"Без значения";
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
                    return _parameter.AsValueString() ?? $"Без значения";
                } catch {
                    return $"Без значения";
                }
            }
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

        public ObservableCollection<Element> Elements { get; }
    }
}
