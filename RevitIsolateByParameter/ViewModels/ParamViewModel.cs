using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitIsolateByParameter.Models;

namespace RevitIsolateByParameter.ViewModels {
    internal class ParamViewModel {
        private readonly Document _document;
        public SharedParam Param { get; }
        public string Name { get; }
        public IList<ParamValue> Values { get; }

        public ParamViewModel(Document document, SharedParam parameter) {
            _document = document;
            Param = parameter;
            Name = parameter.Name;
            Values = GetParameterValues(parameter);
        }

        private IList<ParamValue> GetParameterValues(SharedParam parameter) {
            return new FilteredElementCollector(_document, _document.ActiveView.Id)
                .ToElements()
                .Where(x => x.IsExistsParam(parameter))
                .Select(x => new ParamValue(x.GetParamValue<string>(parameter)))
                .GroupBy(x => x.Value)
                .Select(g => g.First())
                .OrderBy(i => i.Value)
                .ToList();
        }
    }
}
