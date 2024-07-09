using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Converters {
    [ValueConversion(typeof(ViewType), typeof(IEnumerable<string>))]
    internal class ViewTypeConverter : MarkupExtension, IValueConverter {
        private const string _floorPlan = "План этажа";
        private const string _ceilingPlan = "План потолка";


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var viewType = (ViewType?) value;
            switch(viewType) {
                case ViewType.FloorPlan:
                    return _floorPlan;
                case ViewType.CeilingPlan:
                    return _ceilingPlan;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var viewType = (string) value;
            switch(viewType) {
                case _floorPlan:
                    return ViewType.FloorPlan;
                case _ceilingPlan:
                    return ViewType.CeilingPlan;
                default:
                    return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
