using System;
using System.Globalization;
using System.Windows.Data;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace dosymep.WPF.Converters {
    internal sealed class ElementIdConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            var elementId = (value as ElementId
                             ?? ElementId.InvalidElementId).GetIdValue();
            
            if(targetType == typeof(int)) {
                return System.Convert.ToInt32(elementId);
            } else if(targetType == typeof(long)) {
                return System.Convert.ToInt64(elementId);
            } else if(targetType == typeof(string)) {
                return System.Convert.ToString(elementId);
            }

            return elementId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            switch(value) {
                case int intValue:
                    return ConvertToElementId(intValue);
                case long longValue:
                    return ConvertToElementId(longValue);
                case string stringValue:
                    return ConvertToElementId(stringValue);
                default:
                    return ElementId.InvalidElementId;
            }
        }

        private ElementId ConvertToElementId(int value) {
#if REVIT_2023_OR_LESS
            return new ElementId(value);
#else
            return new ElementId((long) value);
#endif
        }

        private ElementId ConvertToElementId(long value) {
#if REVIT_2023_OR_LESS
            return new ElementId((int) value);
#else
            return new ElementId(value);
#endif
        }

        private ElementId ConvertToElementId(string value) {
            if(string.IsNullOrEmpty(value)) {
                return ElementId.InvalidElementId;
            }

#if REVIT_2023_OR_LESS
            return int.TryParse(value, out int result)
                ? new ElementId(result)
                : ElementId.InvalidElementId;
#else
            return long.TryParse(value, out long result)
                ? new ElementId(result)
                : ElementId.InvalidElementId;
#endif
        }
    }
}