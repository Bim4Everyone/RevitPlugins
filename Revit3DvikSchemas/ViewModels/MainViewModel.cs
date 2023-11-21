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

        public ICommand CreateViewCommand { get; }

        private ObservableCollection<HvacSystemViewModel> _revitHVACSystems;

        private string _errorText;

        private string _filterBy;

        private bool _combineFilters;

        private bool _useFopNames;

        private ICollectionView _categoriesView;


        private void SetCategoriesFilters() {
            // Организуем фильтрацию списка категорий
            _categoriesView = CollectionViewSource.GetDefaultView(RevitHVACSystems);
            _categoriesView.Filter = item => String.IsNullOrEmpty(FilterBy) ? true :
                ((HvacSystemViewModel) item).SystemName.IndexOf(FilterBy, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, ViewMaker viewMaker) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;


            RevitHVACSystems = _revitRepository.GetHVACSystems();

            bool RevitUseFopNames = UseFopNames;
            bool RevitCombineFilters = CombineFilters;

            CreateViewCommand = RelayCommand.Create(CreateViews, CanCreateView);
            _viewMaker = viewMaker;

        }


        public ObservableCollection<HvacSystemViewModel> RevitHVACSystems {
            get => _revitHVACSystems;
            set => this.RaiseAndSetIfChanged(ref _revitHVACSystems, value);
        }

        public string FilterBy {
            get => _filterBy;
            set => this.RaiseAndSetIfChanged(ref _filterBy, value);
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

