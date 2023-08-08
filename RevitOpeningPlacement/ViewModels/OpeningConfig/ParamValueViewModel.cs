using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Value;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class ParamValueViewModel : BaseViewModel, IEquatable<ParamValueViewModel> {
        public ParamValueViewModel(ParamValue paramValue) {
            ParamValue = paramValue;
        }

        public object Value => ParamValue.Value;
        public string DisplayValue => ParamValue.DisplayValue;
        public ParamValue ParamValue { get; set; }



        public override bool Equals(object obj) {
            return Equals(obj as ParamValueViewModel);
        }

        public bool Equals(ParamValueViewModel other) {
            return other != null && ParamValue.Equals(other.ParamValue);
        }

        public override int GetHashCode() {
            int hashCode = 931601283;
            hashCode = hashCode * -1521134295 + EqualityComparer<ParamValue>.Default.GetHashCode(ParamValue);
            return hashCode;
        }
    }
}
