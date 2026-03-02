using System;

namespace RevitParamsChecker.Exceptions;

/// <summary>
/// Исключение, если у элемента нет заданного параметра
/// </summary>
internal class ParamNotFoundException : Exception {
    public ParamNotFoundException() {
    }

    public ParamNotFoundException(string msg)
        : base(msg) {
    }
}
