using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitArchitecturalDocumentation.Models.Exceptions {
    internal class ViewNameException : Exception {
        public ViewNameException() { }

        public ViewNameException(string message)
            : base(message) { }
    }
}
