using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitClashDetective.Models.Value {
    internal class ParamValue : IComparable<ParamValue>, IEquatable<ParamValue> {
        public virtual object Value { get; }
        public string DisplayValue { get; set; }

        public virtual int CompareTo(ParamValue other) {
            return Comparer<object>.Default.Compare(Value, other.Value);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParamValue);
        }

        public virtual bool Equals(ParamValue other) {
            return other != null
                && EqualityComparer<string>.Default.Equals(DisplayValue, other.DisplayValue);
        }

        public override int GetHashCode() {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(DisplayValue);
        }
    }

    internal class ParamValue<T> : ParamValue, IComparable<ParamValue<T>> where T : IComparable {
        
        public ParamValue(T value, string stringValue) {
            TValue = value;
            DisplayValue = stringValue ?? TValue?.ToString();
        }

        public T TValue { get; set; }
        public override object Value => TValue;

        public override int CompareTo(ParamValue other) {
            return CompareTo(other as ParamValue<T>);
        }

        public int CompareTo(ParamValue<T> other) {
            return other == null
                ? base.CompareTo(other)
                : TValue.CompareTo(other.TValue);
        }
    }
}
