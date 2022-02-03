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
        public List<ICorrelator> Correlators { get; set; }
        
        public LintelElementCorrelator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            
            Correlators = new List<ICorrelator>();
            Correlators.Add(new GroupCorrelator(_revitRepository));
            Correlators.Add(new DimensionCorrelator(_revitRepository));
            Correlators.Add(new GeometricalCorrelator(_revitRepository));
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            foreach(var correlator in Correlators) {
                var element = correlator.Correlate(lintel);
                if(element != null)
                    return element;
            }
            return null;
        }
    }

    internal class GroupCorrelator : ICorrelator {
        private readonly RevitRepository _revitRepository;
        public GroupCorrelator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public FamilyInstance Correlate(FamilyInstance lintel) {
            if(lintel.SuperComponent != null) {
                return (FamilyInstance) _revitRepository.GetElementById(lintel.SuperComponent.Id);
            }
            return null;
        }
    }

    internal class DimensionCorrelator : ICorrelator {
        private readonly RevitRepository _revitRepository;

        public DimensionCorrelator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            var allignElement = _revitRepository.GetDimensionFamilyInstance(lintel);
            if(allignElement != null) {
                if(allignElement.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id ||
                    allignElement.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Windows).Id) {
                    return (FamilyInstance) _revitRepository.GetElementById(allignElement.Id);
                }

            }
            return null;
        }
    }

    internal class GeometricalCorrelator : ICorrelator {
        private Dictionary<XYZ, FamilyInstance> _elementLocationDict;
        public GeometricalCorrelator(RevitRepository revitRepository) {
            _elementLocationDict = revitRepository
                  .GetAllElementsInWall()
                  .Where(e => e.Location != null)
                  .ToDictionary(e => revitRepository.GetLocationPoint(e));
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            var lintelLocation = ((LocationPoint) lintel.Location).Point;
            //XYZ nearestXYZ = _elementLocationDict.First().Key;
            //var minDist = lintelLocation.DistanceTo(nearestXYZ);
            //foreach(var elementLocation in _elementLocationDict.Keys) {
            //    var dist = lintelLocation.DistanceTo(elementLocation);
            //    if(dist < minDist) {
            //        minDist = dist;
            //        nearestXYZ = elementLocation;
            //    }
            //}

            var nearestXYZ = _elementLocationDict.Keys.OrderBy(item => lintelLocation.DistanceTo(item)).First();
            if(lintelLocation.DistanceTo(nearestXYZ) < 1.6) { //TODO: другое число (может, половина ширины проема)
                return _elementLocationDict[nearestXYZ];
            }
            return null;
        }
    }
}
