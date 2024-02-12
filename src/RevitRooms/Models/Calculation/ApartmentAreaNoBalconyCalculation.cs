using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal class ApartmentAreaNoBalconyCalculation : DoubleParamCalculation {
        public ApartmentAreaNoBalconyCalculation(int percent, int accuracy)
            : base(percent, accuracy) {
            RevitParam = SharedParamsConfig.Instance.ApartmentAreaNoBalcony;
        }

        public override void CalculateParam(SpatialElementViewModel spatialElement) {
            if(spatialElement.IsRoomBalcony != true && spatialElement.Phase.ElementId == Phase.Id) {
                CalculationValue += ConvertValueToSquareMeters(spatialElement.RoomArea, _accuracy);
            }
        }
    }
}
