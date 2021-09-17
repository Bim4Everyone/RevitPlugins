using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                foreach(ParameterViewModel parameter in Values) {
                    parameter.IsSelected = value;
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
