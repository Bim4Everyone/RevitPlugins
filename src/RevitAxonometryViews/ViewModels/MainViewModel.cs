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
        private readonly ObservableCollection<HvacSystemViewModel> _hvacSystems;
        private string _categoriesFilter = string.Empty;
        private string _errorText;
        private ICollectionView _categoriesView;
        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _hvacSystems = _revitRepository.GetHvacSystems();

            //ЗАМЕЧАНИЕ: КРИТЕРИЙ НУЖНО ПОДАВАТЬ ПОСЛЕ СПИСКА
            SelectedCriteria = AxonometryConfig.SystemName;
            FilterCriterion = new ObservableCollection<string>() {
                AxonometryConfig.SystemName,
                AxonometryConfig.FopVisSystemName
            };

            ApplyViewFilter();
            CreateViewsCommand = RelayCommand.Create(CreateViews, CanCreateViews);
            //ЗАМЕЧАНИЕ: ПЕРЕИМЕНОВАТЬ В АППЛАЙ ФИЛЬТЕР КОМАНД
            SelectionFilterCommand = RelayCommand.Create(ApplyViewFilter);
        }

        public ObservableCollection<string> FilterCriterion { get; }
        public bool UseFopVisName { get; set; }
        public bool UseOneView { get; set; }
        public string SelectedCriteria { get; set; }
        public ICommand CreateViewsCommand { get; }
        public ICommand SelectionFilterCommand { get; }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICollectionView FilteredView => _categoriesView;

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
        private void ApplyViewFilter() {
            _categoriesView = CollectionViewSource.GetDefaultView(_hvacSystems);
            if(_categoriesView == null) {
                return;
            }

            if(SelectedCriteria == AxonometryConfig.FopVisSystemName) {
                _categoriesView.Filter = item => string.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystemViewModel) item).FopName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            } else {
                _categoriesView.Filter = item => string.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystemViewModel) item).SystemName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        //Обновление листа
        public void Refresh() {
            FilteredView.Refresh();
        }

        //Получаем выбранные объекты из листа и отправляем на создание в репозиторий. 
        //Реализуется через CreateViewsCommand
        public void CreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();
            _revitRepository.ExecuteViewCreation(selectedItems, UseFopVisName, UseOneView);
        }

        private bool CanCreateViews() {
            var selectedItems = _hvacSystems.Where(item => item.IsSelected).ToList();
            if(selectedItems.Count == 0) {
                ErrorText = "Не выделены системы";
                return false;
            }
            return true;
        }


        //Открытие окна навигатора
        public void ShowWindow() {
            MainWindow mainWindow = new MainWindow(this);
            mainWindow.ShowDialog();
        }
    }
}
