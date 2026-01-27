using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

namespace RevitTagAllCategories.Models.Filtration
{
    internal class DataProvider : IDataProvider {
        public ICollection<Category> GetCategories() {
            throw new NotImplementedException();
        }

        public ICollection<Document> GetDocuments() {
            throw new NotImplementedException();
        }

        public ICollection<IParam> GetParams(ICollection<Category> categories) {
            throw new NotImplementedException();
        }
    }
}
