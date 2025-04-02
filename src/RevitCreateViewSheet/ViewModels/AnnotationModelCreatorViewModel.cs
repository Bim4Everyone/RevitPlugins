using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;

        public AnnotationModelCreatorViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
            AnnotationSymbolTypes = [.. _revitRepository.GetAllAnnotationSymbols()
                .Select(a => new AnnotationSymbolTypeViewModel(a))];
            AcceptView = RelayCommand.Create(() => { }, CanAcceptView);
        }


        public IReadOnlyCollection<AnnotationSymbolTypeViewModel> AnnotationSymbolTypes { get; }

        public AnnotationSymbolTypeViewModel SelectedAnnotationSymbolType { get; set; }

        public ICommand AcceptView { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }



        private bool CanAcceptView() {
            // TODO добавить ErrorText
            return SelectedAnnotationSymbolType is not null;
        }
    }
}
