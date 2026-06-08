namespace RevitPylonLoadAreas.Exceptions;

internal sealed class FloorTopFaceNotFoundException : LoadAreasProcessingException {
    public FloorTopFaceNotFoundException(string message)
        : base(message) {
    }
}
