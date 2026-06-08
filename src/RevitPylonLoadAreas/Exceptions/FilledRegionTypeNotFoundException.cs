namespace RevitPylonLoadAreas.Exceptions;

internal sealed class FilledRegionTypeNotFoundException : LoadAreasProcessingException {
    public FilledRegionTypeNotFoundException(string message)
        : base(message) {
    }
}
