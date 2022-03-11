using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels {
    internal class ParameterViewModel : SelectableObjectViewModel<Parameter>, IComparable<ParameterViewModel>,
        IParameterViewModel {
        private const string _emptyValue = "Пустое значение";
        private const string _defaultValue = "Без значения";
        
        private readonly Parameter _parameter;

        public ParameterViewModel(Parameter parameter, IEnumerable<Parameter> parameters)
            : base(parameter) {
            _parameter = parameter;
            Elements = new ObservableCollection<Element>(parameters.Select(item => item.Element));
        }

        public object Value {
            get {
                if(_parameter.Definition == null) {
                    return null;
                }

                if(_parameter.StorageType == StorageType.ElementId) {
                    return _parameter.AsValueString();
                }

                return _parameter.AsObject();
            }
        }

        public override string DisplayData {
            get {
                if(!_parameter.HasValue) {
                    return _defaultValue;
                }
                
                if(_parameter.Definition == null) {
                    return _defaultValue;
                }

                string value = _parameter.AsValueString();
                if(!string.IsNullOrEmpty(value)) {
                    return value;
                }

                value = _parameter.AsObject()?.ToString();
                if(!string.IsNullOrEmpty(value)) {
                    return value;
                }

                return _emptyValue;
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
            if(other == null || other._parameter.Definition == null) {
                return 1;
            }

            if(_parameter.Definition == null) {
                return -1;
            }

            if(!_parameter.HasValue && !other._parameter.HasValue) {
                return 0;
            }

            if(_parameter.StorageType == other._parameter.StorageType) {
                return Comparer<object>.Default.Compare(Value, other.Value);
            }

            return _parameter.StorageType.CompareTo(other._parameter.StorageType);
        }
    }
}