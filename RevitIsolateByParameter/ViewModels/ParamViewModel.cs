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


namespace RevitIsolateByParameter.ViewModels {
    internal class ParamViewModel {

        private const string ParameterNoValueText = "<Параметр не заполнен>";

        private readonly Document _document;
        public SharedParam Param { get; }
        public string Name { get; }
        public IList<string> Values { get; }

        public ParamViewModel(Document document, SharedParam parameter) {
            _document = document;
            Param = parameter;
            Name = parameter.Name;
            Values = GetParameterValues(parameter);
        }

        private IList<string> GetParameterValues(SharedParam parameter) {
            return new FilteredElementCollector(_document, _document.ActiveView.Id)
                .ToElements()
                .Where(x => x.IsExistsParam(parameter))
                .Select(x => x.GetParamValue<string>(parameter))
                .Select(x => x ?? ParameterNoValueText)
                .Distinct()
                .OrderBy(i => i)
                .ToList();
        }
    }
}
