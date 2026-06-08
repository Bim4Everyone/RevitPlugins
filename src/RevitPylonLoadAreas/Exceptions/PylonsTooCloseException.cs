namespace RevitPylonLoadAreas.Exceptions;

internal sealed class PylonsTooCloseException : LoadAreasProcessingException {
    public PylonsTooCloseException(string message)
        : base(message) {
    }
}
