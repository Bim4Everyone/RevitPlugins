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
        protected FamilySymbol _type;

        public AnnotationManager(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public void CreateAnnotation(SpotDimension spot, int floorCount, double floorHeight) {
            var annotation = PlaceAnnotation(spot, floorCount, floorHeight);
            SetParameters(annotation, spot, floorCount, floorHeight);
            MirrorAnnotation(annotation);
        }

        protected FamilyInstance PlaceAnnotation(SpotDimension spot, int floorCount, double floorHeight) {
            FamilyInstance annotation = null;
            using(Transaction t = _revitRepository.StartTransaction("Создание аннотации")) {
                var placePoint = GetPlacePoint(spot);
                annotation = _revitRepository.CreateAnnotation(_type, placePoint, spot.View);
                t.Commit();
            }
            return annotation;
        }

        protected double GetSpotDimensionLevel(SpotDimension spot) {
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
            return elevation;
        }

        private void SetParameters(FamilyInstance annotation, SpotDimension spot, int count, double typicalFloorHeight) {
            using(Transaction t = _revitRepository.StartTransaction("Установка параметров")) {
                var level = GetSpotDimensionLevel(spot);
                annotation.SetParamValue("Количество типовых этажей", count);
                annotation.SetParamValue("Вкл_Уровень_1", 0);
                annotation.SetParamValue("Id высотной отметки", spot.Id.IntegerValue);
                annotation.SetParamValue("Высота типового этажа", typicalFloorHeight / 1000);
#if D2020 || R2020
                annotation.SetParamValue("Уровень_1", UnitUtils.ConvertFromInternalUnits(level, DisplayUnitType.DUT_METERS));
#else
                annotation.SetParamValue("Уровень_1", UnitUtils.ConvertFromInternalUnits(level, UnitTypeId.Meters));
#endif
                t.Commit();
            }
        }

        protected abstract XYZ GetPlacePoint(SpotDimension spot);

        public void UpdateAnnotation(SpotDimension spot, AnnotationSymbol annotation, int floorCount, double floorHeight) {
            using(Transaction t = _revitRepository.StartTransaction("Создание аннотации")) {
                var placePoint = GetPlacePoint(spot);
                ((LocationPoint) annotation.Location).Point = placePoint;
                t.Commit();
            }
            SetParameters(annotation, spot, floorCount, floorHeight);
        }
        protected virtual void MirrorAnnotation(FamilyInstance annotation) {}
        protected virtual void UpdatePlacement(SpotDimension spot, AnnotationSymbol annotation) { }
        protected virtual void UpdateValues(SpotDimension spot, AnnotationSymbol annotation, int floorCount, double floorHeight) { }
    }
}
