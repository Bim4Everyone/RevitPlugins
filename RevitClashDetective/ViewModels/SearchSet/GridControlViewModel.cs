using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class GridControlViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<IFilterableValueProvider> _providers;
        private readonly List<int> _categoryIds;
        private readonly IEnumerable<Element> _elements;
        private int _elementsCount;

        public GridControlViewModel(RevitRepository revitRepository, Filter filter, IEnumerable<Element> elements) {
            _revitRepository = revitRepository;
            _providers = filter.GetProviders();
            _categoryIds = filter.CategoryIds;
            _elements = elements;
            InitializeColumns();
            InitializeRows();
            AddCommonInfo();

            SelectCommand = new RelayCommand(async p => await SelectElement(p));
        }

        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ObservableCollection<ExpandoObject> Rows { get; set; }

        public ICommand SelectCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand RenewCountCommand { get; }
        public int ElementsCount {
            get => _elementsCount;
            set => this.RaiseAndSetIfChanged(ref _elementsCount, value);
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
                    if((provider.StorageType == StorageType.Integer || provider.StorageType == StorageType.Double) 
                        && double.TryParse(value, out double resultValue) 
                        && resultValue != 0) {
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

        private async Task SelectElement(object p) {
            var row = p as ExpandoObject;
            if(row == null)
                return;
            ((IDictionary<string, object>) row).TryGetValue("Id", out object resultId);
            ((IDictionary<string, object>) row).TryGetValue("File", out object resultFile);
            if(resultId == null) {
                return;
            }
            if(resultId is int id) {
                var element = GetElement(id, resultFile.ToString());
                var bb = GetBoundingBox(element);
                if(bb != null) {
                    await _revitRepository.SelectAndShowElement(GetElementId(element), bb);
                }
            }
        }

        private Element GetElement(int id, string documentName) {
            var doc = _revitRepository.GetDocuments()
                .FirstOrDefault(item => _revitRepository.GetDocumentName(item).Equals(documentName, StringComparison.CurrentCultureIgnoreCase));
            if(doc != null && new ElementId(id).IsNotNull()) {
                return doc.GetElement(new ElementId(id));
            }
            return null;
        }

        private IEnumerable<ElementId> GetElementId(Element element) {
            if(_revitRepository.GetDocumentName(element.Document).Equals(_revitRepository.GetDocumentName(), StringComparison.CurrentCultureIgnoreCase)) {
                yield return element.Id;
            }
        }

        private BoundingBoxXYZ GetBoundingBox(Element element) {
            var transform = _revitRepository.GetLinkedDocumentTransform(_revitRepository.GetDocumentName(element.Document));
            if(transform != null) {
                return SolidUtils.CreateTransformed(element.GetSolid(), transform).GetBoundingBox().GetTransformedBoundingBox();
            }
            return null;
        }
    }
}
