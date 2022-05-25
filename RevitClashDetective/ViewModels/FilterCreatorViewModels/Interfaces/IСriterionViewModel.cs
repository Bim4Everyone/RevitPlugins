using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces {
    interface IСriterionViewModel {
        void Renew();
        bool IsEmpty();
        string GetErrorText();
        void Initialize();
        Criterion GetCriterion();
    }
}
