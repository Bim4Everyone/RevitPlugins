using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models {
    internal class LintelElementCorrelator {
        private RevitRepository _revitRepository;
        private Dictionary<XYZ, FamilyInstance> _elementLocationDict;
        public List<ICorrelator> Correlators { get; set; }
        
        public LintelElementCorrelator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _elementLocationDict = _revitRepository
                  .GetAllElementsInWall()
                  .Where(e => e.Location != null)
                  .ToDictionary(e => _revitRepository.GetLocationPoint(e));
            Correlators = new List<ICorrelator>();
            Correlators.Add(new GroupCorrelator());
            Correlators.Add(new DimensionCorrelator());
            Correlators.Add(new GeometricalCorrelator());
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            foreach(var correlator in Correlators) {
                var element = correlator.Correlate(_revitRepository, lintel, _elementLocationDict);
                if(element != null)
                    return element;
            }
            return null;
        }
    }

    internal class GroupCorrelator : ICorrelator {
        public FamilyInstance Correlate(RevitRepository revitRepository, FamilyInstance lintel, Dictionary<XYZ, FamilyInstance> elementLocationDict = null) {
            if(lintel.SuperComponent != null) {
                return (FamilyInstance) revitRepository.GetElementById(lintel.SuperComponent.Id);
            }
            return null;
        }
    }

    internal class DimensionCorrelator : ICorrelator {
        public FamilyInstance Correlate(RevitRepository revitRepository, FamilyInstance lintel, Dictionary<XYZ, FamilyInstance> elementLocationDict = null) {
            var allignElement = revitRepository.GetDimensionFamilyInstance(lintel);
            if(allignElement != null) {
                if(allignElement.Category.Id == revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id ||
                    allignElement.Category.Id == revitRepository.GetCategory(BuiltInCategory.OST_Windows).Id) {
                    return (FamilyInstance) revitRepository.GetElementById(allignElement.Id);
                }

            }
            return null;
        }
    }

    internal class GeometricalCorrelator : ICorrelator {
        public FamilyInstance Correlate(RevitRepository revitRepository, FamilyInstance lintel, Dictionary<XYZ, FamilyInstance> elementLocationDict = null) {
            var lintelLocation = ((LocationPoint) lintel.Location).Point;
            XYZ nearestXYZ = elementLocationDict.First().Key;
            var minDist = lintelLocation.DistanceTo(nearestXYZ);
            foreach(var elementLocation in elementLocationDict.Keys) {
                var dist = lintelLocation.DistanceTo(elementLocation);
                if(dist < minDist) {
                    minDist = dist;
                    nearestXYZ = elementLocation;
                }
            }
            if(minDist < 1.6) { //TODO: другое число (может, половина ширины проема)
                return elementLocationDict[nearestXYZ];
            }
            return null;
        }
    }
}
