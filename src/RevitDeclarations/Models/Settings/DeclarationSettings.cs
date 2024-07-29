using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class DeclarationSettings {
        public ICollection<RoomPriority> Priorities => PrioritiesConfig.Priorities;
        public PrioritiesConfig PrioritiesConfig { get; set; }
        public IReadOnlyCollection<RoomPriority> UsedPriorities => PrioritiesConfig
            .Priorities
            .Where(x => x.MaxRoomAmount > 0)
            .ToList();

        public Phase SelectedPhase { get; set; }
        public int Accuracy { get; set; }
        public bool LoadUtp { get; set; }
        public IReadOnlyCollection<string> MainRoomNames => PrioritiesConfig
                .Priorities
                .Select(x => x.Name)
                .ToList();
        public string[] BannedRoomNames => PrioritiesConfig.BannedRoomNames;
        public ParametersViewModel ParametersVM { get; set; }

        public Parameter FilterRoomsParam => ParametersVM.SelectedFilterRoomsParam;
        public string FilterRoomsValue => ParametersVM.FilterRoomsValue;
        public Parameter GroupingBySectionParam => ParametersVM.SelectedGroupingBySectionParam;
        public Parameter GroupingByGroupParam => ParametersVM.SelectedGroupingByGroupParam;
        public Parameter MultiStoreyParam => ParametersVM.SelectedMultiStoreyParam;

        public Parameter ApartmentFullNumberParam => ParametersVM.SelectedFullApartNumParam;
        public Parameter DepartmentParam => ParametersVM.SelectedDepartmentParam;
        public Parameter LevelParam => ParametersVM.SelectedLevelParam;
        public Parameter SectionParam => ParametersVM.SelectedSectionParam;
        public Parameter BuildingParam => ParametersVM.SelectedBuildingParam;
        public Parameter ApartmentNumberParam => ParametersVM.SelectedApartNumParam;
        public Parameter ApartmentAreaParam => ParametersVM.SelectedApartAreaParam;
        public Parameter ApartmentAreaCoefParam => ParametersVM.SelectedApartAreaCoefParam;
        public Parameter ApartmentAreaLivingParam => ParametersVM.SelectedApartAreaLivingParam;
        public Parameter RoomsAmountParam => ParametersVM.SelectedRoomsAmountParam;
        public string ProjectName => ParametersVM.ProjectName;
        public Parameter ApartmentAreaNonSumParam => ParametersVM.SelectedApartAreaNonSumParam;
        public Parameter RoomsHeightParam => ParametersVM.SelectedRoomsHeightParam;

        public Parameter RoomAreaParam => ParametersVM.SelectedRoomAreaParam;
        public Parameter RoomAreaCoefParam => ParametersVM.SelectedRoomAreaCoefParam;

        public IReadOnlyCollection<Parameter> AllParameters => new List<Parameter>() {
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
            int prioritiesLength = PrioritiesConfig.Priorities.Count;
            newNames.Sort();
            foreach(var name in newNames) {
                PrioritiesConfig.Priorities.Add(new RoomPriority(prioritiesLength, name) {
                    IsNonConfig = true
                });
                prioritiesLength++;
            }
        }
    }
}
