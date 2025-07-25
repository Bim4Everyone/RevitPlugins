using System;

namespace RevitSleeves.Exceptions;
internal class CannotCreateSleeveException : Exception {
    public CannotCreateSleeveException() { }


    public CannotCreateSleeveException(string message) : base(message) { }
}
