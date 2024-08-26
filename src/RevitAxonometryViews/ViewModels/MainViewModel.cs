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
        private ICollectionView _categoriesView;
        private string _categoriesFilter = string.Empty;

        public ObservableCollection<string> FilterCriterion { get; }
        public ObservableCollection<HvacSystem> DataSource { get; set; }
        public bool UseFopVisName { get; set; }
        public bool UseOneView { get; set; }
        public string SelectedCriteria { get; set; }
        public ICommand CreateViewsCommand { get; }
        public ICommand SelectionFilterCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            SelectedCriteria = AxonometryConfig.SystemName;
            FilterCriterion = new ObservableCollection<string>() {
                AxonometryConfig.SystemName,
                AxonometryConfig.FopVisSystemName
            };

            DataSource = _revitRepository.GetHvacSystems();
            SetListViewFilter();
            CreateViewsCommand = RelayCommand.Create(CreateViews);
            SelectionFilterCommand = RelayCommand.Create(SetListViewFilter);
        }

        //Текст, который подаестся в свойство фильтра для вида.
        //Нужен для SetCategoriesFilters, переопределяется каждый раз при редактировании.
        public string CategoriesFilter {
            get => _categoriesFilter;
            set {
                if(value != _categoriesFilter) {
                    _categoriesFilter = value;
                    Refresh();
                    OnPropertyChanged(nameof(CategoriesFilter));
                }
            }
        }

        //Организуем фильтрацию списка категорий
        //Реализуется через SelectionFilterCommand
        //Здесь мы определяем источник данных для листа и в его свойстве фильтров указываем актуальный критерий фильтра
        //сформированный по CategoriesFilter. В дальнейшем обновляем актуальный критерий при переключении комбобокса
        //Если комбобокс не переключен, нужно просто обновлять CategoriesFilter при введении туда текста
        private void SetListViewFilter() {
            _categoriesView = CollectionViewSource.GetDefaultView(DataSource);
            if(_categoriesView == null) {
                return;
            }

            if(SelectedCriteria == AxonometryConfig.FopVisSystemName) {
                _categoriesView.Filter = item => string.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystem) item).FopName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            } else {
                _categoriesView.Filter = item => string.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystem) item).SystemName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        //Обновление листа
        public void Refresh() {
            _categoriesView.Refresh();
        }

        //Получаем выбранные объекты из листа и отправляем на создание в репозиторий. 
        //Реализуется через CreateViewsCommand
        public void CreateViews() {
            var selectedItems = DataSource.Where(item => item.IsSelected).ToList();
            _revitRepository.ExecuteViewCreation(selectedItems, UseFopVisName, UseOneView);
        }

        //Открытие окна навигатора
        public void ShowWindow() {
            MainWindow mainWindow = new MainWindow(this);
            mainWindow.ShowDialog();
        }
    }
}
