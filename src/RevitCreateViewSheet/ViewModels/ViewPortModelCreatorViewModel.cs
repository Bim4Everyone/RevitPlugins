using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        /// <summary>
        /// Все виды в документе Revit, для которых можно создать видовые экраны. Размещенные и не размещенные.
        /// </summary>
        private readonly List<ViewViewModel> _allViews;
        /// <summary>
        /// Виды которые доступны для выбора пользователя. (Не размещенные виды)
        /// </summary>
        private readonly List<ViewViewModel> _allEnabledViews;
        /// <summary>
        /// Виды, соответствующие выбранному пользователем типу видов. Это итоговая коллекция, которую видит пользователь
        /// </summary>
        private readonly ObservableCollection<ViewViewModel> _viewsForSelection;
        private string _errorText;
        private ViewPortTypeViewModel _selectedViewPortType;
        private ViewTypeViewModel _selectedViewType;
        private ViewViewModel _selectedView;

        public ViewPortModelCreatorViewModel(RevitRepository revitRepository, ILocalizationService localizationService) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _allViews = [.. _revitRepository.GetAllViewsForViewPorts()
                .Select(v => new ViewViewModel(v))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            _allEnabledViews = [.. _allViews];
            _viewsForSelection = [.. _allEnabledViews];
            ViewsForSelection = new ReadOnlyObservableCollection<ViewViewModel>(_viewsForSelection);
            ViewPortTypes = [.. _revitRepository.GetViewPortTypes()
                .Select(v => new ViewPortTypeViewModel(v))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            ViewTypes = InitializeViewTypes();
            SelectedViewType = ViewTypes.First();
            SelectedView = ViewsForSelection.FirstOrDefault();
            SelectedViewPortType = ViewPortTypes.FirstOrDefault();
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        }

        public ViewTypeViewModel SelectedViewType {
            get => _selectedViewType;
            set {
                RaiseAndSetIfChanged(ref _selectedViewType, value);
                UpdateViewsForSelection(value?.ViewType ?? RevitViewType.Any);
            }
        }

        /// <summary>
        /// Типы видов для выбора: план этажа, план потолка, разрез и т.д.
        /// </summary>
        public IReadOnlyCollection<ViewTypeViewModel> ViewTypes { get; }

        /// <summary>
        /// Виды, соответствующие выбранному пользователем типу видов. Это итоговая коллекция, которую видит пользователь
        /// </summary>
        public ReadOnlyObservableCollection<ViewViewModel> ViewsForSelection { get; }

        public ViewViewModel SelectedView {
            get => _selectedView;
            set => RaiseAndSetIfChanged(ref _selectedView, value);
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
        public void SetDisabledViews(ICollection<View> disabledViews) {
            if(disabledViews is null) {
                throw new ArgumentNullException(nameof(disabledViews));
            }
            var disabledViewViewModels = disabledViews.Select(v => new ViewViewModel(v)).ToHashSet();
            _allEnabledViews.Clear();
            for(int i = 0; i < _allViews.Count; i++) {
                if(!disabledViewViewModels.Contains(_allViews[i])) {
                    _allEnabledViews.Add(_allViews[i]);
                }
            }
            UpdateViewsForSelection(SelectedViewType?.ViewType ?? RevitViewType.Any);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(SelectedViewType is null) {
                ErrorText = "Выберите тип вида TODO";
                return false;
            }

            if(ViewsForSelection.Count == 0) {
                ErrorText = "Нет доступных видов заданного типа TODO";
                return false;
            }

            if(SelectedView is null) {
                ErrorText = "Выберите вид TODO";
                return false;
            }

            if(SelectedViewPortType is null) {
                ErrorText = "Выберите тип видового экрана TODO";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private IReadOnlyCollection<ViewTypeViewModel> InitializeViewTypes() {
            return [.. Enum.GetValues(typeof(RevitViewType))
                .Cast<RevitViewType>()
                .Select(v => new ViewTypeViewModel(v,
                    _localizationService.GetLocalizedString($"{nameof(RevitViewType)}.{v}")))];
        }

        private void UpdateViewsForSelection(RevitViewType revitViewType) {
            _viewsForSelection.Clear();
            switch(revitViewType) {
                case RevitViewType.FloorPlan:
                case RevitViewType.CeilingPlan:
                case RevitViewType.EngineeringPlan:
                case RevitViewType.AreaPlan:
                case RevitViewType.Section:
                case RevitViewType.Elevation:
                case RevitViewType.Detail:
                case RevitViewType.ThreeD:
                case RevitViewType.Rendering:
                case RevitViewType.DraftingView:
                case RevitViewType.Legend: {
                    ViewType vType = ConvertToViewType(revitViewType);
                    foreach(var item in _allEnabledViews
                        .Where(v => v.View.ViewType == vType)
                        .OrderBy(v => v.Name, new LogicalStringComparer())) {
                        _viewsForSelection.Add(item);
                    }
                    break;
                }
                case RevitViewType.Any:
                default: {
                    foreach(var item in _allEnabledViews.OrderBy(v => v.Name, new LogicalStringComparer())) {
                        _viewsForSelection.Add(item);
                    }
                    break;
                }
            }
            SelectedView = _viewsForSelection.FirstOrDefault();
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
