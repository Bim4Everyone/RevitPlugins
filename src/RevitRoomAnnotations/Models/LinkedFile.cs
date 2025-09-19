using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RevitRoomAnnotations.Models
{
    internal class LinkedFile {
        public LinkedFile(RevitLinkInstance linkInstance, Document doc) {
            LinkInstance = linkInstance ?? throw new ArgumentNullException(nameof(linkInstance));
            var type = (RevitLinkType) doc.GetElement(linkInstance.GetTypeId());
            IsLoaded = type?.GetLinkedFileStatus() == LinkedFileStatus.Loaded;
            Name = linkInstance.Name;
        }

        public RevitLinkInstance LinkInstance { get; }
        public bool IsLoaded { get; }
        public string Name { get; }
    }
}
