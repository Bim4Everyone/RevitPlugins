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

                foreach(ElementToDelete e in list) {
                    if(e.IsChecked) {
                        document.Delete(e.Element.Id);
                        elements.Remove(e);
                    }
                }
                t.Commit();
            }
        }
    }
}
