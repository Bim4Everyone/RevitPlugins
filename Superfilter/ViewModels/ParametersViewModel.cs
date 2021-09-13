using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using Superfilter.Models;

namespace Superfilter.ViewModels {
    internal class ParametersViewModel : BaseViewModel {
        private bool _selected;
        private readonly Definition _definition;

        public ParametersViewModel(Definition definition, IEnumerable<Parameter> parameters) {
            _definition = definition;
            Values = new ObservableCollection<ParameterViewModel>(GetParamViewModels(parameters));
        }

        public string Name {
            get => _definition?.Name ?? $"Без имени";
        }

        public int Count {
            get => Values.Count;
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

        private static IEnumerable<ParameterViewModel> GetParamViewModels(IEnumerable<Parameter> parameters) {
            return parameters
                .GroupBy(param => param, new ParameterValueComparer())
                .Select(item => new ParameterViewModel(item.Key, item))
                .OrderBy(item => item.Value);
        }
    }
}
