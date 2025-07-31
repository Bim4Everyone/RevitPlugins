using System;

namespace RevitSleeves.Exceptions;
/// <summary>
/// Исключение, когда в не найдено ни одного экземпляра связи с конструкциями
/// </summary>
internal class StructureLinksNotFoundException : Exception {
    public StructureLinksNotFoundException() { }

    public StructureLinksNotFoundException(string message) : base(message) { }
}
