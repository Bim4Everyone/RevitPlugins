using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Value {
    internal abstract class ParamValue : IComparable<ParamValue>, IEquatable<ParamValue> {
        public virtual object Value { get; }
        public string DisplayValue { get; set; }

        public static ParamValue GetParamValue(RevitParam revitParam, Element element) {
            switch(revitParam.StorageType) {
                case StorageType.Integer: {
                    return new IntParamValue(element.GetParamValueOrDefault<int>(revitParam), element.GetParamValueStringOrDefault(revitParam));
                }
                case StorageType.Double: {
                    return new DoubleParamValue(element.GetParamValueOrDefault<double>(revitParam), element.GetParamValueStringOrDefault(revitParam));
                }
                case StorageType.String: {
                    return new StringParamValue(element.GetParamValueOrDefault<string>(revitParam), element.GetParamValueStringOrDefault(revitParam));
                }
                case StorageType.ElementId: {
                    return new ElementIdParamValue(element.GetParamValueStringOrDefault(revitParam), element.GetParamValueStringOrDefault(revitParam));
                }
                default: {
                    throw new ArgumentOutOfRangeException(nameof(revitParam.StorageType), $"У параметра {revitParam.Name} не определен тип данных.");
                }
            }
        }

        public static ParamValue GetParamValue(RevitParam revitParam, string value, string displayValue) {
            switch(revitParam.StorageType) {
                case StorageType.Integer: {
                    return new IntParamValue(value != null ? int.Parse(value) : 0, displayValue);
                }
                case StorageType.Double: {
                    return new DoubleParamValue(value != null ? double.Parse(value) : 0, displayValue);
                }
                case StorageType.String: {
                    return new StringParamValue(value, displayValue);
                }
                case StorageType.ElementId: {
                    return new ElementIdParamValue(value, displayValue);
                }
                default: {
                    throw new ArgumentOutOfRangeException(nameof(revitParam.StorageType), $"У параметра {revitParam.Name} не определен тип данных.");
                }
            }
        }

        public abstract FilterRule GetFilterRule(IVisiter visiter, Document doc, RevitParam param);

        public abstract void SetParamValue(Element element, string paramName);

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

        private protected ElementId GetParamId(Document doc, RevitParam revitParam) {
            ElementId paramId = ElementId.InvalidElementId;
            if(revitParam is SystemParam systemParam) {
                paramId = new ElementId(systemParam.SystemParamId);
            } else {
                paramId = revitParam.GetRevitParamElement(doc)?.Id ?? ElementId.InvalidElementId;
            }
            return paramId;
        }
    }

    internal abstract class ParamValue<T> : ParamValue, IComparable<ParamValue<T>> where T : IComparable {
        public ParamValue() {

        }

        public ParamValue(T value, string stringValue) {
            TValue = value;
            DisplayValue = stringValue ?? TValue?.ToString();
        }

        public ParamValue(T value) {
            TValue = value;
            DisplayValue = TValue?.ToString();
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