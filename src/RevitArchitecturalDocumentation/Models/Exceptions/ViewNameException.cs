using System;

namespace RevitArchitecturalDocumentation.Models.Exceptions {
    internal class ViewNameException : Exception {
        public ViewNameException() { }

        public ViewNameException(string message)
            : base(message) { }
    }
}
