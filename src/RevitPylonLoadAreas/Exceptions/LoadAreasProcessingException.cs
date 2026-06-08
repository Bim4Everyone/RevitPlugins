using System;

namespace RevitPylonLoadAreas.Exceptions;

internal class LoadAreasProcessingException : Exception {
    public LoadAreasProcessingException() {
    }

    public LoadAreasProcessingException(string message)
        : base(message) {
    }
}
