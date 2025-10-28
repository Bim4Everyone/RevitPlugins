using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
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
        private readonly ObservableCollection<AnnotationSymbolTypeViewModel> _allAnnotations;
        private string _errorText;
        private AnnotationSymbolTypeViewModel _selectedAnnotation;
        private string _searchAnnotationName;

        public AnnotationModelCreatorViewModel(
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService ?? throw new System.ArgumentNullException(nameof(localizationService));
            _allAnnotations = [.. _revitRepository.GetAllAnnotationSymbols()
                .Select(a => new AnnotationSymbolTypeViewModel(a))
                .OrderBy(a => a.RichName, new LogicalStringComparer())];
            AnnotationSymbolTypes = new CollectionViewSource() { Source = _allAnnotations };
            AnnotationSymbolTypes.Filter += AnnotationsFilterHandler;
            AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
            PropertyChanged += AnnotationsFilterPropertyChanged;
        }


        public CollectionViewSource AnnotationSymbolTypes { get; }

        public AnnotationSymbolTypeViewModel SelectedAnnotationSymbolType {
            get => _selectedAnnotation;
            set => RaiseAndSetIfChanged(ref _selectedAnnotation, value);
        }

        public ICommand AcceptViewCommand { get; }

        public string SearchAnnotationName {
            get => _searchAnnotationName;
            set => RaiseAndSetIfChanged(ref _searchAnnotationName, value);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }


        private bool CanAcceptView() {
            if(AnnotationSymbolTypes?.View.IsEmpty ?? true) {
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

        private void AnnotationsFilterHandler(object sender, FilterEventArgs e) {
            if(e.Item is AnnotationSymbolTypeViewModel annotation) {
                if(!string.IsNullOrWhiteSpace(SearchAnnotationName)) {
                    string str = SearchAnnotationName.ToLower();
                    e.Accepted = annotation.RichName.ToLower().Contains(str);
                    return;
                }
                e.Accepted = true;
            }
        }

        private void AnnotationsFilterPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SearchAnnotationName)) {
                AnnotationSymbolTypes?.View.Refresh();
            }
        }
    }
}
