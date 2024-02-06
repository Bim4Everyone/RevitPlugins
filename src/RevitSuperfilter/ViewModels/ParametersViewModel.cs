using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSuperfilter.Models;

namespace RevitSuperfilter.ViewModels {
    internal class ParametersViewModel : SelectableObjectViewModel<Definition>, IParametersViewModel {
        private readonly Definition _definition;

        public ParametersViewModel(Definition definition, IEnumerable<Parameter> parameters)
            : base(definition) {
            _definition = definition;

            Values = new ObservableCollection<IParameterViewModel>(GetParamViewModels(parameters));
            foreach(IParameterViewModel item in Values) {
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
                    foreach(IParameterViewModel item in Values) {
                        item.PropertyChanged -= ParameterViewModelPropertyChanged;
                        item.IsSelected = base.IsSelected;
                        item.PropertyChanged += ParameterViewModelPropertyChanged;
                    }
                }
            }
        }

        public ObservableCollection<IParameterViewModel> Values { get; }

        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true || IsSelected == null) {
                return Values.SelectMany(item => item.GetSelectedElements());
            }

            return Enumerable.Empty<Element>();
        }

        private static IEnumerable<IParameterViewModel> GetParamViewModels(IEnumerable<Parameter> parameters) {
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
