using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
internal abstract class DeclarationSettings {
    public ICollection<RoomPriority> Priorities => PrioritiesConfig.Priorities;
    public PrioritiesConfig PrioritiesConfig { get; set; }
    public IReadOnlyCollection<RoomPriority> UsedPriorities => PrioritiesConfig
        .Priorities
        .Where(x => x.MaxRoomAmount > 0)
        .ToList();

    public Phase SelectedPhase { get; set; }
    public int AccuracyForArea { get; set; }
    public int AccuracyForLength { get; set; }
    public bool LoadUtp { get; set; }
    public IReadOnlyCollection<string> MainRoomNames => PrioritiesConfig
            .Priorities
            .Select(x => x.Name)
            .ToList();
    public string[] BannedRoomNames => PrioritiesConfig.BannedRoomNames;

    public Parameter FilterRoomsParam { get; set; }
    public string[] FilterRoomsValues { get; set; }
    public Parameter GroupingBySectionParam { get; set; }
    public Parameter GroupingByGroupParam { get; set; }
    public Parameter MultiStoreyParam { get; set; }
    public virtual Parameter DepartmentParam { get; set; }
    public Parameter LevelParam { get; set; }
    public Parameter ApartmentNumberParam { get; set; }
    public Parameter SectionParam { get; set; }
    public Parameter BuildingParam { get; set; }
    public Parameter ApartmentAreaParam { get; set; }
    public string ProjectName { get; set; }
    public Parameter RoomAreaParam { get; set; }
    public Parameter RoomAreaCoefParam { get; set; }
    public Parameter RoomNameParam { get; set; }
    public Parameter RoomNumberParam { get; set; }

    /// <summary>
    /// Список параметров для проверки их наличия в проекте
    /// </summary>
    public IReadOnlyCollection<Parameter> AllParameters { get; set; }

    public void UpdatePriorities(List<string> newNames) {
        int prioritiesLength = PrioritiesConfig.Priorities.Count;
        newNames.Sort();
        foreach(string name in newNames) {
            PrioritiesConfig.Priorities.Add(new RoomPriority(prioritiesLength, name) {
                IsNonConfig = true
            });
            prioritiesLength++;
        }
    }
}
