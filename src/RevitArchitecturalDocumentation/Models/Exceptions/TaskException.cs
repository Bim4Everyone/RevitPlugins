using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitArchitecturalDocumentation.Models.Exceptions {
    internal class TaskException : Exception {
        public TaskException() { }

        public TaskException(string message)
            : base(message) { }
    }
}
