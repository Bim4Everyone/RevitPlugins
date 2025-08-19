using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.ViewModels.Filtration;
internal interface ICriterionViewModel {
    void Renew();
    bool IsEmpty();
    string GetErrorText();
    void Initialize();
    Criterion GetCriterion();
}
