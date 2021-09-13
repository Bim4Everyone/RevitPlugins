using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyOptionalStandartsCommand : CopyStandartsCommand {
        public CopyOptionalStandartsCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public string BuiltInCategoryName { get; set; }

        protected override FilteredElementCollector GetFilteredElementCollector() {
            var revitApi = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(item => item.GetName().Name.Equals("RevitAPI"));
            return base.GetFilteredElementCollector()
                .OfClass(Type.GetType(BuiltInCategoryName + "," + revitApi.GetName(), true, true));
        }
    }
}
