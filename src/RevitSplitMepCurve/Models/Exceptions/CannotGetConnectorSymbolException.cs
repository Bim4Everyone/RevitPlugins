using System;

namespace RevitSplitMepCurve.Models.Exceptions;

/// <summary>
/// Исключение, когда не удалось получить типоразмер соединителя для вставки
/// </summary>
internal class CannotGetConnectorSymbolException : Exception {
    public CannotGetConnectorSymbolException()
        : base() {
    }

    public CannotGetConnectorSymbolException(string msg)
        : base(msg) {
    }
}
