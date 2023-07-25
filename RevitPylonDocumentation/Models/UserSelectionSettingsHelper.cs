using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitPylonDocumentation.Models {
    internal class UserSelectionSettingsHelper : BaseViewModel {

        private bool _needWorkWithGeneralView;
        public bool NeedWorkWithGeneralView {
            get => _needWorkWithGeneralView;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithGeneralView, value);
        }
    }
}
