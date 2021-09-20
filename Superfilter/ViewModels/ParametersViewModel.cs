﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using Superfilter.Models;

namespace Superfilter.ViewModels {
    internal class ParametersViewModel : SelectableObjectViewModel<Definition> {
        private readonly Definition _definition;

        public ParametersViewModel(Definition definition, IEnumerable<Parameter> parameters)
            : base(definition) {
            _definition = definition;
            
            Values = new ObservableCollection<ParameterViewModel>(GetParamViewModels(parameters));
            foreach(ParameterViewModel item in Values) {
                item.PropertyChanged += ParameterViewModelPropertyChanged;
            }
        }

        public override string DisplayData {
            get => _definition?.Name ?? $"Без имени";
        }

        public int Count {
            get => Values.Count;
        }

        public override bool? IsSelected {
            get => base.IsSelected;
            set {
                base.IsSelected = value;
                if(base.IsSelected.HasValue) {
                    foreach(ParameterViewModel item in Values) {
                        item.PropertyChanged -= ParameterViewModelPropertyChanged;
                        item.IsSelected = base.IsSelected;
                        item.PropertyChanged += ParameterViewModelPropertyChanged;
                    }
                }
            }
        }

        public ObservableCollection<ParameterViewModel> Values { get; }

        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true || IsSelected == null) {
                return Values.SelectMany(item => item.GetSelectedElements());
            }

            return Enumerable.Empty<Element>();
        }

        private static IEnumerable<ParameterViewModel> GetParamViewModels(IEnumerable<Parameter> parameters) {
            return parameters
                .GroupBy(param => param, new ParameterValueComparer())
                .Select(item => new ParameterViewModel(item.Key, item))
                .OrderBy(item => item);
        }

        private void ParameterViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName.Equals(nameof(ParameterViewModel.IsSelected))) {
                UpdateSelection(Values);
            }
        }
    }
}
