using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit.Comparators;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly List<ViewViewModel> _allViews;
        private readonly ObservableCollection<ViewViewModel> _enabledViews;
        private string _errorText;

        public ViewPortModelCreatorViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _allViews = [.. _revitRepository.GetAllViewsForViewPorts()
                .Select(v => new ViewViewModel(v))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            _enabledViews = [.. _allViews];
            EnabledViews = new ReadOnlyObservableCollection<ViewViewModel>(_enabledViews);
            ViewPortTypes = [.. _revitRepository.GetViewPortTypes()
                .Select(v => new ViewPortTypeViewModel(v))
                .OrderBy(a => a.Name, new LogicalStringComparer())];
            SelectedView = EnabledViews.FirstOrDefault();
            SelectedViewPortType = ViewPortTypes.FirstOrDefault();
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        }


        public ReadOnlyObservableCollection<ViewViewModel> EnabledViews { get; }

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
            _enabledViews.Clear();
            for(int i = 0; i < _allViews.Count; i++) {
                if(!disabledViewViewModels.Contains(_allViews[i])) {
                    _enabledViews.Add(_allViews[i]);
                }
            }
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            if(EnabledViews.Count == 0) {
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
    }
}
