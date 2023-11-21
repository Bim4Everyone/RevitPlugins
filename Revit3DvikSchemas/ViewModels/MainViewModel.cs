using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Revit3DvikSchemas.Models;

namespace Revit3DvikSchemas.ViewModels {
    internal class MainViewModel : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ViewMaker _viewMaker;
        private string _selectedElementType;



        private ObservableCollection<HvacSystemViewModel> _revitHVACSystems;



        private string _errorText;

        private bool _combineFilters;

        private bool _useFopNames;

        private ICollectionView _categoriesView;
        private string _categoriesFilter = string.Empty;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, ViewMaker viewMaker) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            bool RevitUseFopNames = UseFopNames;
            bool RevitCombineFilters = CombineFilters;

            ComboBoxVars = new ObservableCollection<String>() { "Имя системы", "ФОП_ВИС_Имя системы" };

            SelectedElementType = ComboBoxVars[0];


            _viewMaker = viewMaker;

            _revitHVACSystems = _revitRepository.GetHVACSystems();
            RevitHVACSystems = _revitHVACSystems;
            SetCategoriesFilters();

            CreateViewCommand = RelayCommand.Create(CreateViews, CanCreateView);
            SelectionFilterCommand = RelayCommand.Create(SetCategoriesFilters);
        }

        public ICommand CreateViewCommand { get; }
        public ICommand SelectionFilterCommand { get; }

        public ObservableCollection<string> ComboBoxVars { get; }

        public string SelectedElementType {
            get => _selectedElementType;
            set => RaiseAndSetIfChanged(ref _selectedElementType, value);
        }

        private void SetCategoriesFilters() {
            // Организуем фильтрацию списка категорий
            _categoriesView = CollectionViewSource.GetDefaultView(RevitHVACSystems);
            if(SelectedElementType == "ФОП_ВИС_Имя системы") {
                _categoriesView.Filter = item => String.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystemViewModel) item).FopName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            } else {
                _categoriesView.Filter = item => String.IsNullOrEmpty(CategoriesFilter) ? true :
                ((HvacSystemViewModel) item).SystemName.IndexOf(CategoriesFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            }

        }

        public ObservableCollection<HvacSystemViewModel> RevitHVACSystems { get; set; } = new ObservableCollection<HvacSystemViewModel>();

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

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool CombineFilters {
            get => _combineFilters;
            set => this.RaiseAndSetIfChanged(ref _combineFilters, value);
        }

        public bool UseFopNames {
            get => _useFopNames;
            set => this.RaiseAndSetIfChanged(ref _useFopNames, value);
        }

        private void CreateViews() {
            _viewMaker.CreateSelectedCommand(RevitHVACSystems, _useFopNames, _combineFilters);
        }

        private bool CanCreateView() {
            List<BuiltInCategory> SystemAndFopCats = _revitRepository.SystemAndFopCats;

            if(!(_revitRepository.Document.ActiveView.ViewType == ViewType.ThreeD)) {
                ErrorText = "Для старта требуется активный 3D-вид";
                return false;
            }

            if(!RevitHVACSystems.Any(x => x.IsChecked)) {
                ErrorText = "Не выбраны элементы.";
                return false;
            }

            if(!_revitRepository.Document.IsExistsSharedParam("ФОП_ВИС_Имя системы")) {
                ErrorText = "Не добавлен параметр ФОП_ВИС_Имя системы";
                return false;
            } else {
                foreach(BuiltInCategory fopCat in _revitRepository.FopNameCategoriesBIC) {
                    if(!_revitRepository.SystemAndFopCats.Contains(fopCat)) {
                        ErrorText = "Параметр ФОП_ВИС_Имя системы не назначен для категории " + fopCat.ToString();
                        return false;
                    }
                }
            }

            ErrorText = "";
            return true;
        }
    }

}

