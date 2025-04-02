using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;

        public ViewPortModelCreatorViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            Views = [.. _revitRepository.GetNotPlacedViews()
                .Select(v => new ViewViewModel(v))];
            ViewPortTypes = [.. _revitRepository.GetViewPortTypes()
                .Select(v => new ViewPortTypeViewModel(v))];
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        }


        public ObservableCollection<ViewViewModel> Views { get; }

        public IReadOnlyCollection<ViewPortTypeViewModel> ViewPortTypes { get; }

        public ViewViewModel SelectedView { get; set; }

        public ViewPortTypeViewModel SelectedViewPortType { get; set; }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanAcceptView() {
            // TODO добавить ErrorText
            return SelectedView is not null
                && SelectedViewPortType is not null;
        }
    }
}
