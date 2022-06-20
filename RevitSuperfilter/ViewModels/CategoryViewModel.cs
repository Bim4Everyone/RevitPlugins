﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSuperfilter.Models;
using RevitSuperfilter.Views;

namespace RevitSuperfilter.ViewModels {
    internal class CategoryViewModel : SelectableObjectViewModel<Category> {
        private ParamsView _paramsView;
        private ObservableCollection<IParametersViewModel> _parameters;

        private string _filterValue;
        private string _buttonFilterName;
        private bool _currentSelection = true;
        private ICollectionView _parametersView;
        private readonly RevitRepository _revitRepository;

        public CategoryViewModel(Category category, IEnumerable<Element> elements, RevitRepository revitRepository)
            : base(category) {
            Category = category;
            _revitRepository = revitRepository;
            Elements = new ObservableCollection<Element>(elements);
        }

        public Category Category { get; }
        public ObservableCollection<Element> Elements { get; }

        public ParamsView ParamsView {
            get {
                if(_paramsView == null) {
                    _paramsView = new ParamsView() { DataContext = this };
                }

                return _paramsView;
            }
        }

        public ICollectionView ParametersView {
            get {
                if(_parameters == null) {
                    _parameters = new ObservableCollection<IParametersViewModel>(GetParamsViewModel().Where(item => item.Count > 1));
                    foreach(IParametersViewModel item in _parameters) {
                        item.PropertyChanged += ParametersViewModelPropertyChanged;
                    }

                    _parametersView = CollectionViewSource.GetDefaultView(_parameters);
                    _parametersView.Filter = item => Filter(item as IParametersViewModel);
                }

                return _parametersView;
            }
        }

        public override string DisplayData {
            get => Category?.Name ?? "Без категории";
        }

        public int Count {
            get => Elements.Count - Elements.OfType<ElementType>().Count();
        }

        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true) {
                return Elements;
            }

            if(IsSelected == null) {
                return _parameters.SelectMany(item => item.GetSelectedElements());
            }

            return Enumerable.Empty<Element>();
        }

        private IEnumerable<IParametersViewModel> GetParamsViewModel() {
            var elementTypes = Elements
                .Select(item => item.GetTypeId())
                .Distinct()
                .Select(item => _revitRepository.GetElement(item))
                .OfType<ElementType>();


            var elementType = new ParametersTypeNameViewModel(ObjectData, elementTypes);
            var family = new ParametersFamilyNameViewModel(ObjectData, elementTypes);
            var paramViewModels = Elements
                .SelectMany(element => element.GetOrderedParameters())
                .GroupBy(param => param, new ParamComparer())
                .Select(param => new ParametersViewModel(param.Key.Definition, param))
                .OrderBy(param => param.DisplayData);

            return new IParametersViewModel[] { family, elementType }.Union(paramViewModels);
        }

        private void ParametersViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName.Equals(nameof(ParameterViewModel.IsSelected))) {
                UpdateSelection(_parameters);
            }
        }

        #region Filter

        public string FilterValue {
            get => _filterValue;
            set {
                this.RaiseAndSetIfChanged(ref _filterValue, value);
                ParametersView.Refresh();
            }
        }

        public string ButtonFilterName {
            get => _buttonFilterName;
            set => this.RaiseAndSetIfChanged(ref _buttonFilterName, value);
        }

        private bool Filter(IParametersViewModel param) {
            if(string.IsNullOrEmpty(FilterValue)) {
                return true;
            }

            return param.DisplayData.IndexOf(FilterValue, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        #endregion

    }
}
