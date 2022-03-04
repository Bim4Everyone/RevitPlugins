using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models {
    internal abstract class AnnotationManager {
        protected readonly RevitRepository _revitRepository;
        private protected FamilySymbol _type;

        public AnnotationManager(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }
        public void UpdateAndPlaceAnnotation(SpotDimension spot, AnnotationSymbol annotation, int floorCount, double floorHeight) {
            if(annotation != null) {
                UpdatePlacement(spot, annotation);
                UpdateValues(spot, annotation, floorCount, floorHeight);
            } else {
                Place(spot, floorCount, floorHeight);
            }
        }

        private protected void SetParameters(FamilyInstance annotation, double level, int count, double typicalFloorHeight) {
            annotation.SetParamValue("Количество типовых этажей", count);
            annotation.SetParamValue("Вкл_Уровень_1", 0);
            annotation.SetParamValue("Высота типового этажа", typicalFloorHeight/1000);
#if D2020 || R2020
            annotation.SetParamValue("Уровень_1", UnitUtils.ConvertFromInternalUnits(level, DisplayUnitType.DUT_METERS));
#else
            annotation.SetParamValue("Уровень_1", UnitUtils.ConvertFromInternalUnits(level, UnitTypeId.Meters));
#endif
        }

        protected virtual void UpdatePlacement(SpotDimension spot, AnnotationSymbol annotation) { }
        protected virtual void UpdateValues(SpotDimension spot, AnnotationSymbol annotation, int floorCount, double floorHeight) { }
        protected virtual void Place(SpotDimension spot, int floorCount, double floorHeight) {
            var placePoint = GetPlacePoint(spot);
            var annotation = _revitRepository.CreateAnnotation(_type, placePoint, spot.View);
            double elevation = 0;
            var references = spot.References;
            foreach(Reference r in references) {
                var element = _revitRepository.GetElement(r.ElementId);
                if(element is Level level) {
                    elevation = level.Elevation;
                    break;
                }
                if(element is FamilyInstance familyInstance) {
                    if(familyInstance.Location is LocationCurve locationCurve && locationCurve.Curve is Line line) {
                        elevation = line.Origin.Z;
                        break;
                    }
                }
            }
            SetParameters(annotation, elevation, floorCount, floorHeight);
        }

        private protected abstract XYZ GetPlacePoint(SpotDimension spot);
    }
}
