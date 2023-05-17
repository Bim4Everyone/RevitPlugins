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
        public static void DeleteSelectedCommand(Document document, List<ElementToDelete> elements) 
        {
            using(Transaction t = new Transaction(document)) {
                t.Start("BIM: Удалить неиспользуемые");
                elements.Where(e => e.IsChecked).ToList().ForEach(e => { document.Delete(e.Element.Id); } );
                t.Commit();
            }
        }
    }
}
