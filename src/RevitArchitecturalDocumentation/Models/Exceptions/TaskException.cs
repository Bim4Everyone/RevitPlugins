using System;

namespace RevitArchitecturalDocumentation.Models.Exceptions {
    internal class TaskException : Exception {
        public TaskException() { }

        public TaskException(string message)
            : base(message) { }
    }
}
