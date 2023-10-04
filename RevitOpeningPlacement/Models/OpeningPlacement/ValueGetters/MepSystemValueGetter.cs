using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class MepSystemValueGetter : IValueGetter<StringParamValue> {
        private readonly Element _mepElement;
        private readonly OpeningsGroup _openingsGroup;
        private readonly bool _createdByOpeningGroup = false;

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
            _mepElement = mepElement ?? throw new ArgumentNullException(nameof(mepElement));
            _createdByOpeningGroup = false;
        }

        public MepSystemValueGetter(OpeningsGroup openingsGroup) {
            _openingsGroup = openingsGroup ?? throw new ArgumentNullException(nameof(openingsGroup));
            _createdByOpeningGroup = true;
        }

        public StringParamValue GetValue() {
            if(_createdByOpeningGroup) {
                return new StringParamValue(_openingsGroup.GetMepSystems());
            } else {
                if(_mepElement.IsExistsParam(_mepSystemParameter)) {
                    return new StringParamValue(_mepElement.GetParamValue<string>(_mepSystemParameter));
                } else {
                    // у коробов и кабельных лотков нет имени системы
                    return new StringParamValue(GetElectricMepSystem(_mepElement));
                }
            }
        }

        private string GetElectricMepSystem(Element element) {
            var doc = element?.Document.Title ?? string.Empty;
            if(IsEomTitle(doc)) {
                return "ЭОМ";
            } else if(IsSsTitle(doc)) {
                return "СС";
            } else {
                return string.Empty;
            }
        }

        private bool IsEomTitle(string title) {
            if(string.IsNullOrWhiteSpace(title)) {
                return false;
            }
            var service = RevitRepository.GetBimModelPartsService();
            return service.InAnyBimModelParts(
                title,
                new BimModelPart[]{
                    BimModelPart.EOPart,
                    BimModelPart.EMPart,
                    BimModelPart.EOMPart,
            });
        }

        private bool IsSsTitle(string title) {
            if(string.IsNullOrWhiteSpace(title)) {
                return false;
            }
            var service = RevitRepository.GetBimModelPartsService();
            return service.GetBimModelPart(title) == BimModelPart.SSPart;
        }
    }
}
