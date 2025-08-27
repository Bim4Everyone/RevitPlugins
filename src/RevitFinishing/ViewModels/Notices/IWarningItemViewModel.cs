namespace RevitFinishing.ViewModels.Notices;

internal interface IWarningItemViewModel {
    string ElementIdInfo { get; }
    string ElementName { get; }
    string CategoryInfo { get; }
    string PhaseName { get; }
    string LevelName { get; }
}
