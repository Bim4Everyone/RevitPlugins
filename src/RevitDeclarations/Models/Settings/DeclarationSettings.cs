using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal abstract class DeclarationSettings {
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

        public virtual ParametersViewModel ParametersVM { get; set; }
        public Parameter FilterRoomsParam => ParametersVM.SelectedFilterRoomsParam;
        public string FilterRoomsValue => ParametersVM.FilterRoomsValue;
        public Parameter GroupingBySectionParam => ParametersVM.SelectedGroupingBySectionParam;
        public Parameter GroupingByGroupParam => ParametersVM.SelectedGroupingByGroupParam;
        public Parameter MultiStoreyParam => ParametersVM.SelectedMultiStoreyParam;
        public virtual Parameter DepartmentParam => ParametersVM.SelectedDepartmentParam;
        public Parameter LevelParam => ParametersVM.SelectedLevelParam;
        public Parameter ApartmentNumberParam => ParametersVM.SelectedApartNumParam;
        public Parameter SectionParam => ParametersVM.SelectedSectionParam;
        public Parameter BuildingParam => ParametersVM.SelectedBuildingParam;
        public Parameter ApartmentAreaParam => ParametersVM.SelectedApartAreaParam;
        public string ProjectName => ParametersVM.ProjectName;
        public Parameter RoomAreaParam => ParametersVM.SelectedRoomAreaParam;
        public virtual Parameter RoomAreaCoefParam => ParametersVM.SelectedRoomAreaParam;
        public Parameter RoomNameParam => ParametersVM.SelectedRoomNameParam;
        public Parameter RoomNumberParam => ParametersVM.SelectedRoomNumberParam;
        public virtual IReadOnlyCollection<Parameter> AllParameters => new List<Parameter>() { };

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
