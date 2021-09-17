﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;
using dosymep.WPF.ViewModels;

using Superfilter.Models;
using Superfilter.Views;

namespace Superfilter.ViewModels {
    internal class CategoryViewModel : SelectableObjectViewModel<Category> {
        private ParamsView _paramsView;
        private ObservableCollection<ParametersViewModel> parameters;

        public CategoryViewModel(Category category, IEnumerable<Element> elements)
            : base(category) {
            Category = category;
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

        public ObservableCollection<ParametersViewModel> Parameters {
            get {
                if(parameters == null) {
                    parameters = new ObservableCollection<ParametersViewModel>(GetParamsViewModel());
                    foreach(ParametersViewModel item in parameters) {
                        item.PropertyChanged += ParametersViewModelPropertyChanged;
                    }
                }

                return parameters;
            }
        }

        public override string DisplayData {
            get => Category?.Name ?? "Без категории";
        }

        public int Count {
            get => Elements.Count;
        }


        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true) {
                return Elements;
            }

            if(IsSelected == null) {
                return Parameters.SelectMany(item => item.GetSelectedElements());
            }

            return Enumerable.Empty<Element>();
        }

        private IEnumerable<ParametersViewModel> GetParamsViewModel() {
            return Elements
                .SelectMany(element => element.GetOrderedParameters().Where(param => param.HasValue))
                .GroupBy(param => param, new ParamComparer())
                .Select(param => new ParametersViewModel(param.Key.Definition, param))
                .OrderBy(param => param.DisplayData);
        }

        private void ParametersViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName.Equals(nameof(ParameterViewModel.IsSelected))) {
                UpdateSelection(parameters);
            }
        }
    }
}
