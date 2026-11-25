namespace RevitPunchingRebar.Models;

/// <summary>
/// Описание назначения параметра, имени и значения переключателя по умолчанию.
/// </summary>
internal sealed class FrameParameterDefinition {
    public FrameParameterDefinition(
        FrameParameterRole role, string defaultName, bool isCopy, bool? defaultYesNoValue = null) {
        Role = role;
        DefaultName = defaultName;
        IsCopy = isCopy;
        DefaultYesNoValue = defaultYesNoValue;
    }

    public FrameParameterRole Role { get; }
    public string DefaultName { get; }
    public bool IsCopy { get; }
    public bool? DefaultYesNoValue { get; }
    public string TitleKey => $"Parameters.{Role}.Title";
    public string DescriptionKey => $"Parameters.{Role}.Description";
}
