using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels {
    internal class ProvidersViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private bool _isSelected;
        private string _name;

        public ProvidersViewModel(RevitRepository revitRepository, List<RevitLinkInstance> links, ParameterFilterElement filter) {
            _revitRepository = revitRepository;
            Name = filter.Name;
            InitializeProviders(filter, links);
        }

        public ProvidersViewModel(RevitRepository revitRepository, ParameterFilterElement filter) {
            _revitRepository = revitRepository;
            Name = filter.Name;
            Providers = new List<IProvider>();
            Providers.Add(new FilterProvider(_revitRepository.Doc, filter, Transform.Identity));
        }

        public List<IProvider> Providers { get; private set; }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private void InitializeProviders(ParameterFilterElement filter, List<RevitLinkInstance> links) {
            Providers = new List<IProvider>();
            foreach(var link in links) {
                Providers.Add(new FilterProvider(link.GetLinkDocument(), filter, link.GetTransform()));
            }
            Providers.Add(new FilterProvider(_revitRepository.Doc, filter, Transform.Identity));
        }
    }
}
