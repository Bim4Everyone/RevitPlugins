using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.Interfaces {
    internal interface ICriterionViewModel {
        void Renew();
        bool IsEmpty();
        string GetErrorText();
        void Initialize();
        Criterion GetCriterion();
    }
}
