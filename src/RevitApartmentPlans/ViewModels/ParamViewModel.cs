using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitApartmentPlans.ViewModels {
    internal class ParamViewModel : BaseViewModel, IEquatable<ParamViewModel> {
        private readonly ParameterElement _parameterElement;

        public ParamViewModel(ParameterElement parameterElement) {
            _parameterElement = parameterElement ?? throw new ArgumentNullException(nameof(parameterElement));
        }


        public string Name => _parameterElement.Name;


        public bool Equals(ParamViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return _parameterElement.Name == other._parameterElement.Name;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParamViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
