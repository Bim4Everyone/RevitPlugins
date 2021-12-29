using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillLevelParamViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public FillLevelParamViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            RevitParam = SharedParamsConfig.Instance.Level;
        }

        public RevitParam RevitParam { get; set; }
        public string Name => $"Заполнить \"{RevitParam.Name}\"";
    }
}
