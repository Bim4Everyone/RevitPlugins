using System;

namespace RevitSplitMepCurve.Models.Exceptions;

/// <summary>
/// Исключение, когда не удалось создать коннектор
/// </summary>
internal class CannotCreateConnectorException : Exception {
    public CannotCreateConnectorException()
        : base() {
    }

    public CannotCreateConnectorException(string msg)
        : base(msg) {
    }
}
