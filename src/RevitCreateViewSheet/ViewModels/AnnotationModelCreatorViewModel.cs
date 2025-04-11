using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit.Comparators;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationModelCreatorViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;
        private AnnotationSymbolTypeViewModel _selectedAnnotation;

        public AnnotationModelCreatorViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
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
                ErrorText = "В проекте отсутствуют семейства аннотаций TODO";
                return false;
            }

            if(SelectedAnnotationSymbolType is null) {
                ErrorText = "Выберите типоразмер аннотации TODO";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
