using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

using pyRevitLabs.Json;

namespace dosymep {
    /// <summary>
    /// Класс десерализации JSON строки
    /// </summary>
    internal class JsonNetSerializer : ISerializer {
        /// <inheritdoc/>
        public T Deserialize<T>(string text) {
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}
