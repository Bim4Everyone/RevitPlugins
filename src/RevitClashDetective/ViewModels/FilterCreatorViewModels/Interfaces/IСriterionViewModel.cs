using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces;
internal interface IСriterionViewModel {
    void Renew();
    bool IsEmpty();
    string GetErrorText();
    void Initialize();
    Criterion GetCriterion();
}
