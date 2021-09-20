using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace Superfilter.ViewModels {
    internal class ParameterViewModel : SelectableObjectViewModel<Parameter>, IComparable<ParameterViewModel> {
        private readonly Parameter _parameter;

        public ParameterViewModel(Parameter parameter, IEnumerable<Parameter> parameters)
            : base(parameter) {
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

        public override string DisplayData {
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

        public ObservableCollection<Element> Elements { get; }

        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true) {
                return Elements;
            }

            return Enumerable.Empty<Element>();
        }

        public int CompareTo(ParameterViewModel other) {
            if(_parameter.StorageType == other._parameter.StorageType) {
                return Comparer<object>.Default.Compare(Value, other.Value);
            }

            return _parameter.StorageType.CompareTo(other._parameter.StorageType);
        }
    }
}
