using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels {
    internal class ProviderViewModel : BaseViewModel {
        private bool _isSelected;
        private string name;
        private IProvider provider;

        public ProviderViewModel(ParameterFilterElement filter) {
            Name = filter.Name;
            provider = new FilterProvider(filter);
        }

        public bool IsSelected { 
            get => _isSelected; 
            set => this.RaiseAndSetIfChanged(ref _isSelected, value); 
        }

        public string Name {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
    }
}
