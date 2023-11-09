using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

using RevitClashDetective.Models.Interfaces;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    /// <summary>
    /// Динамический Grid для перечисления элементов, попадающих в заданный фильтр. 
    /// Использовать для проверки фильтра в настройках расстановки заданий на отверстия в активном файле ВИС
    /// </summary>
    internal class GridControlViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<IFilterableValueProvider> _providers;
        private readonly IEnumerable<Element> _elements;
        private int _elementsCount;

        public GridControlViewModel(RevitRepository revitRepository, Filter filter, IEnumerable<Element> elements) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(filter is null) {
                throw new ArgumentNullException(nameof(filter));
            }
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));

            _providers = filter.GetProviders();
            InitializeColumns();
            InitializeRows();
            AddCommonInfo();

            SelectCommand = new RelayCommand(p => SelectElement(p));
        }

        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ObservableCollection<ExpandoObject> Rows { get; set; }

        public ICommand SelectCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand RenewCountCommand { get; }
        public int ElementsCount {
            get => _elementsCount;
            set => RaiseAndSetIfChanged(ref _elementsCount, value);
        }

        private void InitializeColumns() {
            Columns = new ObservableCollection<ColumnViewModel>(_providers
                .Select(item => new ColumnViewModel() {
                    FieldName = item.Name,
                    Header = $"Параметр: {item.DisplayValue}"
                })
                .GroupBy(item => item.FieldName)
                .Select(item => item.First()));
        }

        private void InitializeRows() {
            Rows = new ObservableCollection<ExpandoObject>();
            foreach(var element in _elements) {
                IDictionary<string, object> row = new ExpandoObject();
                foreach(var provider in _providers) {
                    string value = provider.GetElementParamValue(element)?.DisplayValue;
                    if((provider.StorageType == StorageType.Integer || provider.StorageType == StorageType.Double)
                        && double.TryParse(value, out double resultValue)
                        && resultValue != 0) {
                        AddValue(row, provider.Name, resultValue);
                    } else if(!string.IsNullOrEmpty(value) && value != "0") {
                        AddValue(row, provider.Name, value);
                    }

                }
                row.Add("Category", element.Category?.Name);
                row.Add("FamilyName", element.GetTypeId().IsNotNull()
                    ? (element.Document.GetElement(element.GetTypeId()) as ElementType)?.FamilyName
                    : null);
                row.Add("Name", element.Name);
                row.Add("Id", element.Id.GetIdValue());
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
            Columns.Insert(0, new ColumnViewModel() { FieldName = "FamilyName", Header = "Имя семейства" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Category", Header = "Категория" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Id", Header = "Id" });
        }


        private void SelectElement(object p) {
            if(!(p is ExpandoObject row))
                return;
            ((IDictionary<string, object>) row).TryGetValue("Id", out object resultId);
            if(resultId == null) {
                return;
            }
            var element = GetElement(resultId);
            if(element != null) {
                _revitRepository.SelectAndShowElement(new[] { new ElementModel(element) });
            }
        }

        private Element GetElement(object id) {
            ElementId elementId = ElementId.InvalidElementId;
#if REVIT_2023_OR_LESS
            if(id is int idInt) {
                elementId = new ElementId(idInt);
            }
#else
            if(id is long idLong) {
                elementId = new ElementId(idLong);
            }
#endif
            return _revitRepository.Doc.GetElement(elementId);
        }
    }
}
