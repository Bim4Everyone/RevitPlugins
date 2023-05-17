using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitDeleteUnused.Models;

namespace RevitDeleteUnused.Commands {
    internal class DeleteCommand 
    {
        public static void DeleteSelectedCommand(Document document, ObservableCollection<ElementToDelete> elements) 
        {
            var list = elements.ToList();
            using(Transaction t = new Transaction(document)) {
                t.Start("BIM: Удалить неиспользуемые");

                foreach(ElementToDelete elementToDelete in list) {
                    if(elementToDelete.IsChecked) {
                        document.Delete(elementToDelete.Element.Id);
                        elements.Remove(elementToDelete);
                    }
                }
                t.Commit();
            }
        }
    }
}
