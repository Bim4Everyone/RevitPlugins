using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class SlopeParams {
        private readonly RevitRepository _revitRepository;

        public SlopeParams(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public void SetSlopeParams(FamilyInstance slope, SlopeCreationData slopeCreationData) {
            slope.SetParamValue("Высота", slopeCreationData.Height);
            slope.SetParamValue("Ширина", slopeCreationData.Width);
            slope.SetParamValue("Глубина", slopeCreationData.Depth);

            //Поворот элемента
            double rotationAngle = slopeCreationData.RotationRadiansAngle;
            Transform openingTransform = slope.GetTotalTransform();
            Line middleLine = Line.CreateUnbound(slopeCreationData.Center, openingTransform.BasisZ);
            ElementTransformUtils.RotateElement(_revitRepository.Document, slope.Id, middleLine, rotationAngle);
        }
    }
}
