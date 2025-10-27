using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly EntitiesTracker _entitiesTracker;
        private readonly ILocalizationService _localizationService;
        /// <summary>
        /// Все виды в документе Revit, для которых можно создать видовые экраны. Размещенные и не размещенные.
        /// </summary>
        private readonly IReadOnlyCollection<ViewViewModel> _allViews;
        /// <summary>
        /// Виды которые доступны для выбора пользователя. (Не размещенные виды)
        /// </summary>
        private readonly ObservableCollection<ViewViewModel> _viewsCanBePlacedOnThisSheet;
        private string _errorText;
        private ViewPortTypeViewModel _selectedViewPortType;
        private ViewTypeViewModel _selectedViewType;
        private ViewViewModel _selectedView;
        private string _searchViewName;

        public ViewPortModelCreatorViewModel(
            RevitRepository revitRepository,
            EntitiesTracker entitiesTracker,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _entitiesTracker = entitiesTracker ?? throw new ArgumentNullException(nameof(entitiesTracker));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _allViews = [.. _revitRepository.GetAllViewsForViewPorts()
                .Select(v => new ViewViewModel(v))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            _viewsCanBePlacedOnThisSheet = [.. _allViews];
            Views = new CollectionViewSource() { Source = _viewsCanBePlacedOnThisSheet };
            Views.Filter += ViewsFilterHandler;
            ViewPortTypes = [.. _revitRepository.GetViewPortTypes()
                .Select(v => new ViewPortTypeViewModel(v))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            ViewTypes = InitializeViewTypes();
            SelectedViewType = ViewTypes.First();
            SelectedViewPortType = ViewPortTypes.FirstOrDefault();
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
            PropertyChanged += ViewFilterPropertyChanged;
        }

        public ViewTypeViewModel SelectedViewType {
            get => _selectedViewType;
            set => RaiseAndSetIfChanged(ref _selectedViewType, value);
        }

        public string SearchViewName {
            get => _searchViewName;
            set => RaiseAndSetIfChanged(ref _searchViewName, value);
        }

        /// <summary>
        /// Типы видов для выбора: план этажа, план потолка, разрез и т.д.
        /// </summary>
        public IReadOnlyCollection<ViewTypeViewModel> ViewTypes { get; }

        /// <summary>
        /// Виды, соответствующие выбранному пользователем типу видов. Это итоговая коллекция, которую видит пользователь
        /// </summary>
        public CollectionViewSource Views { get; }

        public ViewViewModel SelectedView {
            get => _selectedView;
            set {
                RaiseAndSetIfChanged(ref _selectedView, value);
            }
        }

        /// <summary>
        /// Типоразмеры для видовых экранов
        /// </summary>
        public IReadOnlyCollection<ViewPortTypeViewModel> ViewPortTypes { get; }

        public ViewPortTypeViewModel SelectedViewPortType {
            get => _selectedViewPortType;
            set => RaiseAndSetIfChanged(ref _selectedViewPortType, value);
        }

        public ICommand AcceptViewCommand { get; }


        /// <summary>
        /// Выключает отображение заданных видов для создания видовых экранов.
        /// </summary>
        /// <param name="disabledViews">Коллекция видов, которые надо убрать из выбора для пользователя.</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public void UpdateEnabledViews(SheetModel sheetModel) {
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }
            // Особенности размещения видов на листах в ревите:
            //   - на одном листе нельзя разместить несколько экземпляров одного и того же вида
            //   - легенды можно размещать на листах повторно на других листах
            var disabledViews = _entitiesTracker.AliveViewPorts
                .Where(v => v.View.ViewType != ViewType.Legend)
                .Union(sheetModel.ViewPorts)
                .Select(v => new ViewViewModel(v.View))
                .ToHashSet();
            var enabledViews = _allViews
                .Where(v => !disabledViews.Contains(v))
                .OrderBy(v => v.Name, new LogicalStringComparer());
            _viewsCanBePlacedOnThisSheet.Clear();
            foreach(var view in enabledViews) {
                _viewsCanBePlacedOnThisSheet.Add(view);
            }
            SelectedView = null;
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(SelectedViewType is null) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.ViewTypeNotSet");
                return false;
            }

            if(Views?.View.IsEmpty ?? true) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.ViewsNotFound");
                return false;
            }

            if(SelectedView is null) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.ViewNotSet");
                return false;
            }

            if(SelectedViewPortType is null) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.ViewPortTypeNotSet");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void ViewFilterPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SelectedViewType)
                || e.PropertyName == nameof(SearchViewName)) {
                Views?.View.Refresh();
            }
        }

        private IReadOnlyCollection<ViewTypeViewModel> InitializeViewTypes() {
            return [.. Enum.GetValues(typeof(RevitViewType))
                .Cast<RevitViewType>()
                .Select(v => new ViewTypeViewModel(v,
                    _localizationService.GetLocalizedString($"{nameof(RevitViewType)}.{v}")))];
        }

        private void ViewsFilterHandler(object sender, FilterEventArgs e) {
            if(e.Item is ViewViewModel view) {
                if(SelectedViewType is not null
                    && SelectedViewType.ViewType != RevitViewType.Any
                    && view.View.ViewType != ConvertToViewType(SelectedViewType.ViewType)) {
                    e.Accepted = false;
                    return;
                }
                if(!string.IsNullOrWhiteSpace(SearchViewName)) {
                    string str = SearchViewName.ToLower();
                    e.Accepted = view.Name.ToLower().Contains(str);
                    return;
                }
                e.Accepted = true;
            }
        }

        private ViewType ConvertToViewType(RevitViewType revitViewType) {
            return revitViewType switch {
                RevitViewType.FloorPlan => ViewType.FloorPlan,
                RevitViewType.CeilingPlan => ViewType.CeilingPlan,
                RevitViewType.EngineeringPlan => ViewType.EngineeringPlan,
                RevitViewType.AreaPlan => ViewType.AreaPlan,
                RevitViewType.Section => ViewType.Section,
                RevitViewType.Elevation => ViewType.Elevation,
                RevitViewType.Detail => ViewType.Detail,
                RevitViewType.ThreeD => ViewType.ThreeD,
                RevitViewType.Rendering => ViewType.Rendering,
                RevitViewType.DraftingView => ViewType.DraftingView,
                RevitViewType.Legend => ViewType.Legend,
                _ => throw new NotSupportedException()
            };
        }
    }
}
