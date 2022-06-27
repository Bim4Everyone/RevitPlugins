using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class GridControlViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<IFilterableValueProvider> _providers;
        private readonly IEnumerable<Element> _elements;

        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ObservableCollection<ExpandoObject> Rows { get; set; }
        public GridControlViewModel(RevitRepository revitRepository, IEnumerable<IFilterableValueProvider> providers, IEnumerable<Element> elements) {
            _revitRepository = revitRepository;
            _providers = providers;
            _elements = elements;
            InitializeColumns();
            InitializeRows();
            AddCommonInfo();
        }

        private void InitializeColumns() {
            Columns = new ObservableCollection<ColumnViewModel>(
                _providers.Select(item => new ColumnViewModel() {
                    FieldName = item.Name,
                    Header = $"Параметр: { item.Name}"
                }).GroupBy(item => item.FieldName)
                .Select(item => item.First()));
        }

        private void InitializeRows() {
            Rows = new ObservableCollection<ExpandoObject>();
            foreach(var element in _elements) {
                IDictionary<string, object> row = new ExpandoObject();
                foreach(var column in Columns) {
                    row.Add(column.FieldName, GetParamValue(element, column.FieldName));
                }
                row.Add("File", _revitRepository.GetDocumentName(element.Document));
                row.Add("Category", element?.Category?.Name);
                row.Add("Name", element.Name);
                row.Add("Id", element.Id.IntegerValue);
                Rows.Add((ExpandoObject) row);
            }
        }

        private object GetParamValue(Element element, string paramName) {
            object result = null;
            if(element.IsExistsParam(paramName)) {
                result = element.GetParam(paramName).AsValueString();
                if(string.IsNullOrEmpty((string) result)) {
                    result = element.GetParamValueOrDefault(paramName);
                }
            }

            if(result != null) {
                return result;
            }
            var typeId = element.GetTypeId();
            if(typeId == null)
                return null;
            var type = _revitRepository.GetElement(typeId);
            if(type == null)
                return null;
            return GetParamValue(type, paramName);
        }

        private void AddCommonInfo() {
            Columns.Add(new ColumnViewModel() { FieldName = "Id", Header = "Id" });
            Columns.Add(new ColumnViewModel() { FieldName = "File", Header = "Файл" });
            Columns.Add(new ColumnViewModel() { FieldName = "Category", Header = "Категория" });
            Columns.Add(new ColumnViewModel() { FieldName = "Name", Header = "Имя типоразмера" });
        }
    }
}
