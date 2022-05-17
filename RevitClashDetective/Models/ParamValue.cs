using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal class ParamValue {
        private string _stringValue;

        public object Value { get; }
        public string StringValue { get; }

        public ParamValue(object value, string stringValue) {
            Value = value;
            if(string.IsNullOrEmpty(stringValue)) {
                StringValue = Value?.ToString();
            } else {
                StringValue = stringValue;
            }
        }

        public override bool Equals(object obj) {
            return obj is ParamValue value &&
                   EqualityComparer<object>.Default.Equals(Value, value.Value) &&
                   StringValue == value.StringValue;
        }

        public override int GetHashCode() {
            int hashCode = 931601283;
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StringValue);
            return hashCode;
        }
    }

    internal class ParamValueComparer : IComparer<ParamValue> {
        public int Compare(ParamValue x, ParamValue y) {
            if(x.Value is IComparable compX && y.Value is IComparable compY) {
                return compX.CompareTo(compY);
            }
            return x.StringValue.CompareTo(y.StringValue);
        }
    }
}
