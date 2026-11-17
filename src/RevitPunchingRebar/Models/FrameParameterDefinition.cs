namespace RevitPunchingRebar.Models;

/// <summary>
/// Описание назначения и имени параметра по умолчанию.
/// </summary>
internal sealed class FrameParameterDefinition {
    public FrameParameterDefinition(FrameParameterRole role, string defaultName, bool isCopy) {
        Role = role;
        DefaultName = defaultName;
        IsCopy = isCopy;
    }

    public FrameParameterRole Role { get; }
    public string DefaultName { get; }
    public bool IsCopy { get; }
    public string TitleKey => $"Parameters.{Role}.Title";
    public string DescriptionKey => $"Parameters.{Role}.Description";
}
