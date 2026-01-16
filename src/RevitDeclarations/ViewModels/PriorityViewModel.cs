using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
public class PriorityViewModel {
    public PriorityViewModel(RoomPriority priority) {
        Priority = priority;
        OrdinalNumber = priority.OrdinalNumber;
        Name = priority.Name;
        AreaCoefficient = priority.AreaCoefficient.ToString();

        IsSummer = priority.IsSummer ? "Да" : "Нет";

        IsLiving = priority.IsLiving ? "Да" : "Нет";
    }

    public RoomPriority Priority { get; }
    public int OrdinalNumber { get; }
    public string Name { get; }
    public string AreaCoefficient { get; }
    public string IsSummer { get; }
    public string IsLiving { get; }
}
