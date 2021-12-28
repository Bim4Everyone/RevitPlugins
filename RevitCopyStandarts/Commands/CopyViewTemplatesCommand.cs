using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyViewTemplatesCommand : CopyStandartsCommand {
        public CopyViewTemplatesCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public override string Name => "Шаблон";

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType();
        }

        protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
            return elements.Cast<View>().Where(item => item.IsTemplate);
        }
    }
}
