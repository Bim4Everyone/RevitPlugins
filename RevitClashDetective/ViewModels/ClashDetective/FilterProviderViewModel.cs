using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.ClashDetective.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class FilterProviderViewModel : BaseViewModel, IProviderViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Filter _filter;
        private bool _isSelected;
        private string _name;

        public FilterProviderViewModel(RevitRepository revitRepository, Filter filter) {
            _revitRepository = revitRepository;
            _filter = filter;
            Name = filter.Name;
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public IProvider GetProvider(Document doc, Transform transform) {
            return new FilterProvider(doc, _filter, transform);
        }
    }
}
