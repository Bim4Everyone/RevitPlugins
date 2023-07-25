using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitPylonDocumentation.Models {
    internal class UserSelectionSettings : BaseViewModel {

        private bool _needWorkWithGeneralView = false;
        private bool _needWorkWithGeneralPerpendicularView = false;
        private bool _needWorkWithTransverseViewFirst = false;
        private bool _needWorkWithTransverseViewSecond = false;
        private bool _needWorkWithTransverseViewThird = false;
        private bool _needWorkWithRebarSchedule = false;
        private bool _needWorkWithMaterialSchedule = false;
        private bool _needWorkWithSystemPartsSchedule = false;
        private bool _needWorkWithIFCPartsSchedule = false;
        private bool _needWorkWithLegend = false;


        public bool NeedWorkWithGeneralView {
            get => _needWorkWithGeneralView;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithGeneralView, value);
        }
        public bool NeedWorkWithGeneralPerpendicularView {
            get => _needWorkWithGeneralPerpendicularView;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithGeneralPerpendicularView, value);
        }

        public bool NeedWorkWithTransverseViewFirst {
            get => _needWorkWithTransverseViewFirst;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithTransverseViewFirst, value);
        }

        public bool NeedWorkWithTransverseViewSecond {
            get => _needWorkWithTransverseViewSecond;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithTransverseViewSecond, value);
        }

        public bool NeedWorkWithTransverseViewThird {
            get => _needWorkWithTransverseViewThird;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithTransverseViewThird, value);
        }

        public bool NeedWorkWithRebarSchedule {
            get => _needWorkWithRebarSchedule;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithRebarSchedule, value);
        }

        public bool NeedWorkWithMaterialSchedule {
            get => _needWorkWithMaterialSchedule;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithMaterialSchedule, value);
        }

        public bool NeedWorkWithSystemPartsSchedule {
            get => _needWorkWithSystemPartsSchedule;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithSystemPartsSchedule, value);
        }

        public bool NeedWorkWithIFCPartsSchedule {
            get => _needWorkWithIFCPartsSchedule;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithIFCPartsSchedule, value);
        }

        public bool NeedWorkWithLegend {
            get => _needWorkWithLegend;
            set => this.RaiseAndSetIfChanged(ref _needWorkWithLegend, value);
        }
    }
}
