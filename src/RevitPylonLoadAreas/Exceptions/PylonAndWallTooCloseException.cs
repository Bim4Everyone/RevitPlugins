namespace RevitPylonLoadAreas.Exceptions;

internal sealed class PylonAndWallTooCloseException : LoadAreasProcessingException {
    public PylonAndWallTooCloseException(string message)
        : base(message) {
    }
}
