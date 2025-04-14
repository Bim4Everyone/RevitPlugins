using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private string _errorText;
        private AnnotationSymbolTypeViewModel _selectedAnnotation;

        public AnnotationModelCreatorViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new System.ArgumentNullException(nameof(localizationService));
            AnnotationSymbolTypes = [.. _revitRepository.GetAllAnnotationSymbols()
                .Select(a => new AnnotationSymbolTypeViewModel(a))
                .OrderBy(a => a.RichName, new LogicalStringComparer())];
            SelectedAnnotationSymbolType = AnnotationSymbolTypes.FirstOrDefault();
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
        }


        public IReadOnlyCollection<AnnotationSymbolTypeViewModel> AnnotationSymbolTypes { get; }

        public AnnotationSymbolTypeViewModel SelectedAnnotationSymbolType {
            get => _selectedAnnotation;
            set => RaiseAndSetIfChanged(ref _selectedAnnotation, value);
        }

        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }


        private bool CanAcceptView() {
            if(AnnotationSymbolTypes.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("Errors.AnnotationsNotFound");
                return false;
            }

            if(SelectedAnnotationSymbolType is null) {
                ErrorText = _localizationService.GetLocalizedString("Errors.Validation.AnnotationNotSet");
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
