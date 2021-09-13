using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using RevitCopyStandarts.ViewModels;

namespace RevitCopyStandarts.Commands {
    internal abstract class CopyStandartsCommand : ICopyStandartsCommand {
        private readonly Document _source;
        private readonly Document _target;

        protected CopyStandartsCommand(Document source, Document target) {
            _source = source;
            _target = target;
        }

        public virtual string Name { get; set; }

        protected virtual FilteredElementCollector GetFilteredElementCollector() {
            return new FilteredElementCollector(_source);
        }

        protected virtual ElementId CopyElement(Element element, CopyPasteOptions copyOptions) {
            return ElementTransformUtils.CopyElements(_source, new[] { element.Id }, _target, Transform.Identity, copyOptions).First();
        }

        protected virtual bool IsAllowCommit(Element newElement, Element sourceElement) {
            return newElement.Name.Equals(sourceElement.Name);
        }

        protected virtual IDuplicateTypeNamesHandler GetDuplicateTypeNamesHandler() {
            return new CustomCopyHandler();
        }

        protected virtual IEnumerable<Element> FilterElements(IList<Element> elements) {
            return elements.Where(item => item.ViewSpecific == false);
        }

        public void Execute() {
            IList<Element> elements = GetFilteredElementCollector().ToElements();
            using(var transactionGroup = new TransactionGroup(_target)) {
                transactionGroup.Start($"Копирование \"{Name}\"");

                CopyPasteOptions copyOptions = new CopyPasteOptions();
                //copyOptions.SetDuplicateTypeNamesHandler(GetDuplicateTypeNamesHandler());

                foreach(Element element in FilterElements(elements)) {
                    using(var transaction = new Transaction(_target)) {
                        transaction.Start($"Копирование \"{Name} - {element.Name}\"");

                        ElementId newElementId = CopyElement(element, copyOptions);
                        if(IsAllowCommit(_target.GetElement(newElementId), element)) {
                            transaction.Commit();
                        } else {
                            transaction.RollBack();
                        }
                    }
                }

                transactionGroup.Assimilate();
            }
        }
    }
}
