using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class DeclarationSettings {
        private readonly List<RoomPriority> _roomPriorities;
        private readonly List<string> _mainNames;
        private readonly PrioritiesConfig _prioritiesConfig;

        public DeclarationSettings() {
            _prioritiesConfig = new PrioritiesConfig();
            _roomPriorities = _prioritiesConfig.Priorities;

            _mainNames = _roomPriorities
                .Select(x => x.NameLower)
                .ToList();
        }

        public List<RoomPriority> Priorities => _roomPriorities;
        public PrioritiesConfig PrioritiesConfig => _prioritiesConfig;
        public List<RoomPriority> UsedPriorities => _roomPriorities
            .Where(x => x.MaxRoomAmount > 0)
            .ToList();

        public Phase SelectedPhase { get; set; }
        public int Accuracy { get; set; }
        public bool LoadUtp { get; set; }
        public List<string> MainRoomNames => _mainNames;
        public string[] BannedRoomNames => _prioritiesConfig.BannedRoomNames;
        public ParametersViewModel ViewModel { get; set; }

        public Parameter FilterRoomsParam => ViewModel.SelectedFilterRoomsParam;
        public string FilterRoomsValue => ViewModel.FilterRoomsValue;
        public Parameter GroupingBySectionParam => ViewModel.SelectedGroupingBySectionParam;
        public Parameter GroupingByGroupParam => ViewModel.SelectedGroupingByGroupParam;
        public Parameter MultiStoreyParam => ViewModel.SelectedMultiStoreyParam;

        public Parameter ApartmentFullNumberParam => ViewModel.SelectedFullApartNumParam;
        public Parameter DepartmentParam => ViewModel.SelectedDepartmentParam;
        public Parameter LevelParam => ViewModel.SelectedLevelParam;
        public Parameter SectionParam => ViewModel.SelectedSectionParam;
        public Parameter BuildingParam => ViewModel.SelectedBuildingParam;
        public Parameter ApartmentNumberParam => ViewModel.SelectedApartNumParam;
        public Parameter ApartmentAreaParam => ViewModel.SelectedApartAreaParam;
        public Parameter ApartmentAreaCoefParam => ViewModel.SelectedApartAreaCoefParam;
        public Parameter ApartmentAreaLivingParam => ViewModel.SelectedApartAreaLivingParam;
        public Parameter RoomsAmountParam => ViewModel.SelectedRoomsAmountParam;
        public string ProjectName => ViewModel.ProjectName;
        public Parameter ApartmentAreaNonSumParam => ViewModel.SelectedApartAreaNonSumParam;
        public Parameter RoomsHeightParam => ViewModel.SelectedRoomsHeightParam;

        public Parameter RoomAreaParam => ViewModel.SelectedRoomAreaParam;
        public Parameter RoomAreaCoefParam => ViewModel.SelectedRoomAreaCoefParam;

        public List<Parameter> AllParameters => new List<Parameter>() {
            FilterRoomsParam,
            GroupingBySectionParam,
            GroupingByGroupParam,
            MultiStoreyParam,
            ApartmentFullNumberParam,
            DepartmentParam,
            LevelParam,
            SectionParam,
            BuildingParam,
            ApartmentNumberParam,
            ApartmentAreaParam,
            ApartmentAreaCoefParam,
            ApartmentAreaLivingParam,
            RoomsAmountParam,
            ApartmentAreaNonSumParam,
            RoomsHeightParam,
            RoomAreaParam,
            RoomAreaCoefParam
        };

        public void UpdatePriorities(List<string> newNames) {
            int prioritiesLength = _roomPriorities.Count;
            newNames.Sort();
            foreach(var name in newNames) {
                _roomPriorities.Add(new RoomPriority(prioritiesLength, name) {
                    IsOther = true
                });
                prioritiesLength++;
            }
        }
    }
}
