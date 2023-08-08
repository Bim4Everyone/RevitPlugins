using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

using Element = DevExpress.Map.Kml.Model.Element;

namespace RevitRooms.ViewModels {
    internal class LevelViewModel : ElementViewModel<Level> {
        private readonly string _name;
        public List<Level> Levels { get; }

        public LevelViewModel(string name, List<Level> levels, RevitRepository revitRepository,
            IEnumerable<SpatialElement> spatialElements)
            : base(levels.FirstOrDefault(), revitRepository) {
            _name = name;
            Levels = levels;
            SpatialElements =
                new ObservableCollection<SpatialElementViewModel>(GetSpatialElements(revitRepository, spatialElements));
        }

        public override string Name => _name;

        public string LevelNames => string.Join(Environment.NewLine,
            Levels.OrderBy(item => item.Elevation).Select(item => item.Name).Distinct());

#if REVIT_2020_OR_LESS
        public string Elevation {
            get {
                return UnitUtils.ConvertFromInternalUnits(Element.Elevation, DisplayUnitType.DUT_METERS)
                    .ToString("0.000", CultureInfo.InvariantCulture);
            }
        }
#else
        public string Elevation {
            get {
                return UnitUtils.ConvertFromInternalUnits(Element.Elevation, UnitTypeId.Meters)
                    .ToString("0.000", CultureInfo.InvariantCulture);
            }
        }
#endif

        public int RoomsCount {
            get { return SpatialElements.Count; }
        }

        public override string LevelName {
            get { return Name; }
        }

        public ObservableCollection<SpatialElementViewModel> SpatialElements { get; }

        private HashSet<ElementId> GetSpatialElementsHashSet() {
            return new HashSet<ElementId>(SpatialElements.Select(item => item.ElementId));
        }

        public IEnumerable<RoomSeparatorViewModel> GetRoomSeparators(IEnumerable<PhaseViewModel> phases) {
            HashSet<ElementId> phaseElements = new HashSet<ElementId>(phases
                .Select(item => item.ElementId));

            return RevitRepository.GetRoomSeparators()
                .Where(item => item.LevelId == Element.Id)
                .Where(item => phaseElements.Contains(item.CreatedPhaseId))
                .Select(item => new RoomSeparatorViewModel(item, 
                    new PhaseViewModel((Phase) RevitRepository.GetElement(item.CreatedPhaseId), RevitRepository), RevitRepository));
        }

        public IEnumerable<FamilyInstanceViewModel> GetDoors(IEnumerable<PhaseViewModel> phases) {
            HashSet<ElementId> spatialElements = GetSpatialElementsHashSet();
            HashSet<ElementId> phaseElements = new HashSet<ElementId>(phases
                .Select(item => item.ElementId));

            return RevitRepository.GetDoors()
                .Where(item => item.LevelId == Element.Id)
                .Where(item => phaseElements.Contains(item.CreatedPhaseId))
                .Select(item => new FamilyInstanceViewModel(item,
                    new PhaseViewModel((Phase) RevitRepository.GetElement(item.CreatedPhaseId), RevitRepository), RevitRepository))
                .Where(item => spatialElements.Contains(item.ToRoom?.ElementId)
                               && spatialElements.Contains(item.FromRoom?.ElementId));
        }

        public IEnumerable<FamilyInstanceViewModel> GetWindows(IEnumerable<PhaseViewModel> phases) {
            HashSet<ElementId> spatialElements = GetSpatialElementsHashSet();
            HashSet<ElementId> phaseElements = new HashSet<ElementId>(phases
                .Select(item => item.ElementId));

            return RevitRepository.GetWindows()
                .Where(item => item.LevelId == Element.Id)
                .Where(item =>
                    item.Symbol.FamilyName.IndexOf("Окн_ББлок_", StringComparison.CurrentCultureIgnoreCase) >= 0)
                .Where(item => phaseElements.Contains(item.CreatedPhaseId))
                .Select(item => new FamilyInstanceViewModel(item,
                    new PhaseViewModel((Phase) RevitRepository.GetElement(item.CreatedPhaseId), RevitRepository), RevitRepository))
                .Where(item => spatialElements.Contains(item.ToRoom?.ElementId)
                               && spatialElements.Contains(item.FromRoom?.ElementId));
        }

        public IEnumerable<SpatialElementViewModel> GetAreas() {
            return SpatialElements.Where(item => item.Element is Area);
        }

        public IEnumerable<SpatialElementViewModel> GetRooms(PhaseViewModel phase) {
            return SpatialElements
                .Where(item => item.Phase != null)
                .Where(item => item.Phase == phase)
                .Where(item => item.Element is Room);
        }

        public IEnumerable<SpatialElementViewModel> GetRooms(IEnumerable<PhaseViewModel> phases) {
            return SpatialElements
                .Where(item => item.Phase != null)
                .Where(item => phases.Contains(item.Phase))
                .Where(item => item.Element is Room);
        }

        private static IEnumerable<SpatialElementViewModel> GetSpatialElements(RevitRepository revitRepository,
            IEnumerable<SpatialElement> spatialElements) {
            return spatialElements
                .Select(item => new SpatialElementViewModel(item, revitRepository))
                .Where(item => item.IsPlaced);
        }
    }
}