using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal interface ITableExporter {
        void Export(string path, DeclarationDataTable declarationTable);
    }
}
