using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyColorFillSchemesCommand : ICopyStandartsCommand {
        private readonly Document _source;
        private readonly Document _target;

        public CopyColorFillSchemesCommand(Document source, Document target) {
            _source = source;
            _target = target;
        }

        public void Execute() {
            IList<Element> sourceElements = new FilteredElementCollector(_source)
                .OfCategory(BuiltInCategory.OST_ColorFillSchema)
                .ToElements();

            IList<Element> targetElements = new FilteredElementCollector(_target)
                .OfCategory(BuiltInCategory.OST_ColorFillSchema)
                .ToElements();

            using(var transaction = new Transaction(_target)) {
                transaction.Start($"Копирование \"Схема цветов\"");

                _target.Delete(targetElements.Intersect(sourceElements, new FillSchemaEqualityComparer()).Select(item => item.Id).ToArray());
                ElementTransformUtils.CopyElements(_source, sourceElements.Select(item => item.Id).ToArray(), _target, Transform.Identity, new CopyPasteOptions());

                transaction.Commit();
            }
        }
    }

    internal class FillSchemaEqualityComparer : IEqualityComparer<Element> {
        public bool Equals(Element x, Element y) {
            return x?.Name.Equals(y.Name) == true;
        }

        public int GetHashCode(Element obj) {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }
}
