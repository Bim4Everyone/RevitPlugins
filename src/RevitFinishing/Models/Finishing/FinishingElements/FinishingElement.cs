using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitFinishing.Models.Finishing
{
    /// <summary>
    /// Абстрактный класс для экземпляра отделки.
    /// Каждый элемент отделки хранит список всех помещений, к которым он относится.
    /// </summary>
    internal abstract class FinishingElement {
        private protected readonly Element _revitElement;
        private protected readonly FinishingCalculator _calculator;
        private protected readonly ParamCalculationService _paramService;
        private protected readonly SharedParamsConfig _paramConfig = SharedParamsConfig.Instance;

        public FinishingElement(Element element, 
                                FinishingCalculator calculator, 
                                ParamCalculationService paramService) {
            _revitElement = element;
            _calculator = calculator;
            _paramService = paramService;
        }

        public Element RevitElement => _revitElement;
        public List<RoomElement> Rooms { get; set; }

        private protected void UpdateFromSharedParam(SharedParam param) {
            _revitElement.SetParamValue(param, _paramService.GetRoomsParameters(Rooms, param));
        }

        private protected void UpdateFromBltnParam(IEnumerable<RoomElement> rooms, SharedParam param, BuiltInParameter bltnParam) {
            _revitElement.SetParamValue(param, _paramService.GetRoomsParameters(rooms, bltnParam));
        }

        private protected void UpdateFromKeyParam(IEnumerable<RoomElement> rooms, SharedParam param, ProjectParam keyParam) {
            _revitElement.SetParamValue(param, _paramService.GetRoomsKeyParameters(rooms, keyParam));
        }

        private protected void UpdateFromInstParam(SharedParam param, BuiltInParameter bltnParam) {
            _revitElement.SetParamValue(param, _revitElement.GetParamValue<double>(bltnParam));
        }

        private protected void UpdateOrderParam(SharedParam param, int value) {
            _revitElement.SetParamValue(param, value);
        }

        /// <summary>
        /// Проверка типов отделки помещений.
        /// Все помещения, к которым относятся экземпляр отделки должны иметь одинаковый тип отделки.
        /// </summary>
        public bool CheckFinishingTypes() {
            List<string> finishingTypes = Rooms
                .Select(x => x.RoomFinishingType)
                .Distinct()
                .ToList();

            if(finishingTypes.Count == 1)
                return true;
            return false;
        }

        public void UpdateFinishingParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            UpdateFromSharedParam(_paramConfig.FloorFinishingType1);
            UpdateFromSharedParam(_paramConfig.FloorFinishingType2);
            UpdateFromSharedParam(_paramConfig.FloorFinishingType3);
            UpdateFromSharedParam(_paramConfig.FloorFinishingType4);
            UpdateFromSharedParam(_paramConfig.FloorFinishingType5);

            UpdateFromSharedParam(_paramConfig.CeilingFinishingType1);
            UpdateFromSharedParam(_paramConfig.CeilingFinishingType2);
            UpdateFromSharedParam(_paramConfig.CeilingFinishingType3);
            UpdateFromSharedParam(_paramConfig.CeilingFinishingType4);
            UpdateFromSharedParam(_paramConfig.CeilingFinishingType5);

            UpdateFromSharedParam(_paramConfig.WallFinishingType1);
            UpdateFromSharedParam(_paramConfig.WallFinishingType2);
            UpdateFromSharedParam(_paramConfig.WallFinishingType3);
            UpdateFromSharedParam(_paramConfig.WallFinishingType4);
            UpdateFromSharedParam(_paramConfig.WallFinishingType5);
            UpdateFromSharedParam(_paramConfig.WallFinishingType6);
            UpdateFromSharedParam(_paramConfig.WallFinishingType7);
            UpdateFromSharedParam(_paramConfig.WallFinishingType8);
            UpdateFromSharedParam(_paramConfig.WallFinishingType9);
            UpdateFromSharedParam(_paramConfig.WallFinishingType10);

            UpdateFromSharedParam(_paramConfig.BaseboardFinishingType1);
            UpdateFromSharedParam(_paramConfig.BaseboardFinishingType2);
            UpdateFromSharedParam(_paramConfig.BaseboardFinishingType3);
            UpdateFromSharedParam(_paramConfig.BaseboardFinishingType4);
            UpdateFromSharedParam(_paramConfig.BaseboardFinishingType5);

            UpdateFromBltnParam(Rooms, _paramConfig.FinishingRoomName, BuiltInParameter.ROOM_NAME);
            UpdateFromBltnParam(Rooms, _paramConfig.FinishingRoomNumber, BuiltInParameter.ROOM_NUMBER);
            UpdateFromBltnParam(finishingType.Rooms, _paramConfig.FinishingRoomNames, BuiltInParameter.ROOM_NAME);
            UpdateFromBltnParam(finishingType.Rooms, _paramConfig.FinishingRoomNumbers, BuiltInParameter.ROOM_NUMBER);

            UpdateFromKeyParam(Rooms, _paramConfig.FinishingType, ProjectParamsConfig.Instance.RoomFinishingType);

            UpdateFromInstParam(_paramConfig.SizeArea, BuiltInParameter.HOST_AREA_COMPUTED);
            UpdateFromInstParam(_paramConfig.SizeVolume, BuiltInParameter.HOST_VOLUME_COMPUTED);
        }

        public abstract void UpdateCategoryParameters();
    }
}
