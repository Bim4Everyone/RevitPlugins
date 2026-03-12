using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
public class PriorityViewModel {
    public PriorityViewModel(RoomPriority priority, ILocalizationService localizationService) {
        Priority = priority;
        OrdinalNumber = priority.OrdinalNumber;
        Name = priority.Name;
        AreaCoefficient = priority.AreaCoefficient.ToString();

        IsSummer = priority.IsSummer 
            ? localizationService.GetLocalizedString("Window.Yes") 
            : localizationService.GetLocalizedString("Window.No");

        IsLiving = priority.IsLiving
            ? localizationService.GetLocalizedString("Window.Yes")
            : localizationService.GetLocalizedString("Window.No");
    }

    public RoomPriority Priority { get; }
    public int OrdinalNumber { get; }
    public string Name { get; }
    public string AreaCoefficient { get; }
    public string IsSummer { get; }
    public string IsLiving { get; }
}
