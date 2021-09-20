﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace Superfilter.ViewModels {
    internal class ParameterViewModel : SelectableObjectViewModel<Parameter>, IComparable<ParameterViewModel> {
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
                    return _defaultValue;
                }

                if(_parameter.StorageType == StorageType.ElementId) {
                    return _parameter.Element.Document.GetElement(_parameter.AsElementId())?.Name ?? _defaultValue;
                }

                return _parameter.AsObject() ?? _defaultValue;
            }
        }

        public override string DisplayData {
            get {
                if(_parameter.Definition == null) {
                    return _defaultValue;
                }

                return _parameter.AsValueString() ?? _defaultValue;
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

            if(_parameter.StorageType == other._parameter.StorageType) {
                return Comparer<object>.Default.Compare(Value, other.Value);
            }

            return _parameter.StorageType.CompareTo(other._parameter.StorageType);
        }
    }
}