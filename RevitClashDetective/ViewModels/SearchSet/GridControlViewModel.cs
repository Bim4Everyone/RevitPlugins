using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class GridControlViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<IFilterableValueProvider> _providers;
        private readonly List<int> _categoryIds;
        private readonly IEnumerable<Element> _elements;

        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ObservableCollection<ExpandoObject> Rows { get; set; }
        public GridControlViewModel(RevitRepository revitRepository, Filter filter, IEnumerable<Element> elements) {
            _revitRepository = revitRepository;
            _providers = filter.GetProviders();
            _categoryIds = filter.CategoryIds;
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
                foreach(var provider in _providers) {
                    string value = provider.GetElementParamValue(_categoryIds.ToArray(), element).DisplayValue;
                    if(double.TryParse(value, out double resultValue) && resultValue != 0) {
                        AddValue(row, provider.Name, resultValue);
                    } else if(!string.IsNullOrEmpty(value) && value != "0") {
                        AddValue(row, provider.Name, value);
                    }
                }
                row.Add("File", _revitRepository.GetDocumentName(element.Document));
                row.Add("Category", element?.Category?.Name);
                row.Add("Name", element.Name);
                row.Add("Id", element.Id.IntegerValue);
                Rows.Add((ExpandoObject) row);
            }
        }

        private void AddValue<T>(IDictionary<string, object> row, string key, T value) {
            if(!row.ContainsKey(key)) {
                row.Add(key, value);
            } else {
                row[key] = value;
            }
        }

        private void AddCommonInfo() {
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Name", Header = "Имя типоразмера" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Category", Header = "Категория" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Id", Header = "Id" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "File", Header = "Файл" });
        }
    }
}
