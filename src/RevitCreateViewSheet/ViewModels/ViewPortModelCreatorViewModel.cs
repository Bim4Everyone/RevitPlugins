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
        private readonly List<ViewViewModel> _allViews;
        private readonly List<ViewViewModel> _allEnabledViews;
        private readonly ObservableCollection<ViewViewModel> _viewsForSelection;
        private string _errorText;
        private ViewTypeViewModel _selectedViewType;

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
                // TODO сделать обновление списка видов для выбора
            }
        }

        public IReadOnlyCollection<ViewTypeViewModel> ViewTypes { get; }

        public ReadOnlyObservableCollection<ViewViewModel> ViewsForSelection { get; }

        public IReadOnlyCollection<ViewPortTypeViewModel> ViewPortTypes { get; }

        public ViewViewModel SelectedView { get; set; }

        public ViewPortTypeViewModel SelectedViewPortType { get; set; }

        public ICommand AcceptViewCommand { get; }

        /// <summary>
        /// Убирает возможность выбрать заданные виды для создания видовых экранов.
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
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(ViewsForSelection.Count == 0) {
                ErrorText = "Все виды уже добавлены на листы TODO";
                return false;
            }

            if(ViewPortTypes.Count == 0) {
                ErrorText = "В проекте отсутствуют типоразмеры видовых экранов TODO";
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
    }
}
