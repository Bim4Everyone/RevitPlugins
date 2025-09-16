using System;

namespace RevitSleeves.Exceptions;
/// <summary>
/// Исключение, когда ось обрабатываемой стены не является прямой линией
/// </summary>
internal class WallNotLineException : Exception {
    public WallNotLineException() { }

    public WallNotLineException(string msg) : base(msg) { }
}
