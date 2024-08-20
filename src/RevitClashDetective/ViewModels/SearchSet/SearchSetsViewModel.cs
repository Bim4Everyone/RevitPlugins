using System;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class SearchSetsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly SearchSetViewModel _straightSearchSet;
        private readonly SearchSetViewModel _invertedSearchSet;
        private SearchSetViewModel _searchSet;

        public SearchSetsViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;

            Filter = filter;
            _straightSearchSet = new SearchSetViewModel(_revitRepository, Filter, new StraightRevitFilterGenerator());
            _invertedSearchSet = new SearchSetViewModel(_revitRepository, Filter, new InvertedRevitFilterGenerator());

            SearchSet = _straightSearchSet;
            Name = filter.Name;
            MessageBoxService = GetPlatformService<IMessageBoxService>();

            InversionChangedCommand = RelayCommand.Create(InversionChanged);
            ShowSetCommand = RelayCommand.Create(ShowSet);
            CloseCommand = RelayCommand.Create(Close);
        }


        public IMessageBoxService MessageBoxService { get; }

        public string Name { get; }

        public bool Inverted { get; set; }

        public ICommand InversionChangedCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowSetCommand { get; }

        public SearchSetViewModel SearchSet {
            get => _searchSet;
            set => this.RaiseAndSetIfChanged(ref _searchSet, value);
        }

        public Filter Filter { get; }

        private void InversionChanged() {
            if(Inverted) {
                SearchSet = _invertedSearchSet;
            } else {
                SearchSet = _straightSearchSet;
            }
            ShowSet();
        }

        private void Close() {
            Action action = () => {
                var command = new CreateFiltersCommand();
                command.ExecuteCommand(_revitRepository.UiApplication, Filter.Name);
            };
            _revitRepository.DoAction(action);
        }

        private void ShowSet() {
            SearchSetViewModel invertedSelectedSet;
            if(Inverted) {
                invertedSelectedSet = _straightSearchSet;
            } else {
                invertedSelectedSet = _invertedSearchSet;
            }
            try {
                _revitRepository.ShowElements(
                    invertedSelectedSet.Filter
                        .GetRevitFilter(_revitRepository.Doc, invertedSelectedSet.FilterGenerator),
                    invertedSelectedSet.Filter
                        .CategoryIds
                        .Select(c => c.AsBuiltInCategory())
                        .ToHashSet());
            } catch(InvalidOperationException ex) {
                MessageBoxService.Show(
                    ex.Message,
                    $"BIM",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
            }
        }
    }
}
