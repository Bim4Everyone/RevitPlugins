using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DateValueGetter : IValueGetter<StringParamValue> {

        public DateValueGetter() { }

        public StringParamValue GetValue() {
            return new StringParamValue(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }
    }
}
