using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private LinkInstanceViewModel _linkInstance;

        public MainViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            FillParams = new ObservableCollection<BaseViewModel>(GetFillParams());
            LinkInstances = new ObservableCollection<LinkInstanceViewModel>(GetLinkInstances());
            DesignOptions = new ObservableCollection<DesingOptionsViewModel>(GetDesignOptions());
        }

        public LinkInstanceViewModel LinkInstance {
            get => _linkInstance;
            set => this.RaiseAndSetIfChanged(ref _linkInstance, value);
        }

        public ObservableCollection<BaseViewModel> FillParams { get; }
        public ObservableCollection<LinkInstanceViewModel> LinkInstances { get; }
        public ObservableCollection<DesingOptionsViewModel> DesignOptions { get; }

        private IEnumerable<BaseViewModel> GetFillParams() {
            yield return new FillLevelParamViewModel(_revitRepository);
            yield return new FillParamViewModel(_revitRepository) { RevitParam = SharedParamsConfig.Instance.RoomArea };
            yield return new FillParamViewModel(_revitRepository) { RevitParam = SharedParamsConfig.Instance.RoomAreaRatio };
            yield return new FillParamViewModel(_revitRepository) { RevitParam = SharedParamsConfig.Instance.RoomSectionShortName };
        }

        private IEnumerable<DesingOptionsViewModel> GetDesignOptions() {
            return _revitRepository.GetDesignOptions().Select(item => new DesingOptionsViewModel(item, _revitRepository));
        }

        private IEnumerable<LinkInstanceViewModel> GetLinkInstances() {
            return _revitRepository.GetLinkInstances()
                .Select(item => new LinkInstanceViewModel(item, _revitRepository));
        }
    }
}