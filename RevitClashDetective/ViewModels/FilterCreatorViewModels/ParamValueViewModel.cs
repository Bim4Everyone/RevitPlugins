using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using DevExpress.Dialogs.Core.ViewModel;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models {
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
