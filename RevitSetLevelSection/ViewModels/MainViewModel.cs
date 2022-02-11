using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public MainViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            FillParams = new ObservableCollection<BaseViewModel>(GetFillParams());
            DesignOptions = new ObservableCollection<DesingOptionsViewModel>(GetDesignOptions());
        }

        public ObservableCollection<BaseViewModel> FillParams { get; }
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
    }
}