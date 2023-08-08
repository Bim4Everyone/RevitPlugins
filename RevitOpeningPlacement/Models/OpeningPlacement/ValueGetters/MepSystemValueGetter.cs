using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class MepSystemValueGetter : IValueGetter<StringParamValue> {
        private readonly Element _mepElement;

        /// <summary>
        /// Параметр "Имя системы"
        /// </summary>
        private const BuiltInParameter _mepSystemParameter = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;

        /// <summary>
        /// Конструктор объекта, предоставляющего название системы инженерного элемента
        /// </summary>
        /// <param name="mepElement">Элемент инженерной системы</param>
        /// <exception cref="ArgumentException">Исключение, если элемент не содержит параметр "Имя системы"</exception>
        /// <exception cref="ArgumentNullException">Исключение, если ссылка на элемент null</exception>
        public MepSystemValueGetter(Element mepElement) {
            if(mepElement is null) {
                throw new ArgumentNullException(nameof(mepElement));
            }
            _mepElement = mepElement;
        }

        public StringParamValue GetValue() {
            if(_mepElement.IsExistsParam(_mepSystemParameter)) {
                return new StringParamValue(_mepElement.GetParamValue<string>(_mepSystemParameter));
            } else {
                // у коробов и кабельных лотков нет имени системы
                return new StringParamValue(string.Empty);
            }
        }
    }
}
