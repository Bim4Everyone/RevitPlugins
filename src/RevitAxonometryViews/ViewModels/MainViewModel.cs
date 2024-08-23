using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.Views;

namespace RevitAxonometryViews.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public ObservableCollection<string> FilterCriterion {  get; }
        public ObservableCollection<HvacSystem> DataSource { get; set; }

        public bool UseFopVisName {get; set;}
        public bool UseOneView { get; set;} 

        public string SelectedCriteria { get; set;}

        private ICollectionView _categoriesView;
        private string _categoriesFilter = string.Empty;

        public MainViewModel(RevitRepository revitRepository) {
            SelectedCriteria = AxonometryConfig.SystemName;
            _revitRepository = revitRepository;
            FilterCriterion = new ObservableCollection<string>() {
                AxonometryConfig.SystemName,
                AxonometryConfig.FopVisSystemName
            };

            DataSource = GetDataSource();
            SetCategoriesFilters();
            CreateViewsCommand = RelayCommand.Create(CreateViews);
            SelectionFilterCommand = RelayCommand.Create(SetCategoriesFilters);
        }

        public ICommand CreateViewsCommand { get; }

        public ICommand SelectionFilterCommand { get; }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }


        public string CategoriesFilter {
            get => _categoriesFilter;
            set {
                if(value != _categoriesFilter) {
                    _categoriesFilter = value;
                    _categoriesView.Refresh();
                    OnPropertyChanged(nameof(CategoriesFilter));
                }
            }
        }
        private void SetCategoriesFilters() {
            // Организуем фильтрацию списка категорий
            _categoriesView = CollectionViewSource.GetDefaultView(DataSource);
            if(SelectedCriteria == AxonometryConfig.FopVisSystemName) {
                _categoriesView.Filter = item => string.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystem) item).FopName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            } else {
                _categoriesView.Filter = item => string.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystem) item).SystemName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        public ObservableCollection<HvacSystem> GetDataSource() {
            return _revitRepository.GetHvacSystems();
        }

        public void CreateViews() {
            var selectedItems = DataSource.Where(item => item.IsSelected).ToList();
            _revitRepository.ExecuteViewCreation(selectedItems, UseFopVisName, UseOneView);
        }

        public void ShowWindow() {
            MainWindow mainWindow = new MainWindow(this);
            mainWindow.ShowDialog();
        }
    }
}
