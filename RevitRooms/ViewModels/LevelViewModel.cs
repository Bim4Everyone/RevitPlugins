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

namespace RevitRooms.ViewModels {
    internal class LevelViewModel : ElementViewModel<Level> {
        private bool _isSelected;

        public LevelViewModel(Level level, RevitRepository revitRepository, IEnumerable<SpatialElement> spatialElements) : base(level, revitRepository) {
            SpartialElements = new ObservableCollection<SpatialElementViewModel>(GetSpatialElements(revitRepository, spatialElements));
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
#if D2020 || R2020
        public string Elevation {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, DisplayUnitType.DUT_METERS).ToString("0.000", CultureInfo.InvariantCulture); }
        }
#elif D2021 || R2021 || D2022 || R2022
        public string Elevation {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, UnitTypeId.Meters).ToString("0.000", CultureInfo.InvariantCulture); }
        }
#endif

        public int RoomsCount {
            get { return SpartialElements.Count; }
        }

        public override string LevelName {
            get { return Name; }
        }

        public ObservableCollection<SpatialElementViewModel> SpartialElements { get; }

        public IEnumerable<DoorViewModel> GetDoors(IEnumerable<PhaseViewModel> phases) {
            return RevitRepository.GetDoors()
                .SelectMany(item => phases.Select(phase => new DoorViewModel(item, phase, RevitRepository)))
                .Where(item => item.LevelId == Element.Id)
                .Where(item => SpartialElements.Contains(item.ToRoom) || SpartialElements.Contains(item.FromRoom));
        }

        public IEnumerable<SpatialElementViewModel> GetAreas() {
            return SpartialElements.Where(item => item.Element is Area);
        }

        public IEnumerable<SpatialElementViewModel> GetRooms(PhaseViewModel phase) {
            return SpartialElements.Where(item => item.Phase != null).Where(item => item.Phase == phase).Where(item => item.Element is Room);
        }

        public IEnumerable<SpatialElementViewModel> GetRooms(IEnumerable<PhaseViewModel> phases) {
            return SpartialElements.Where(item => item.Phase != null).Where(item => phases.Contains(item.Phase)).Where(item => item.Element is Room);
        }

        private static IEnumerable<SpatialElementViewModel> GetSpatialElements(RevitRepository revitRepository, IEnumerable<SpatialElement> spatialElements) {
            return spatialElements.Select(item => new SpatialElementViewModel(item, revitRepository)).Where(item => item.IsPlaced);
        }
    }
}
