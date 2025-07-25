using System.Linq;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.ViewModels.Placing;
internal class ErrorViewModel : BaseViewModel {
    public ErrorViewModel(ErrorModel errorModel) {
        ErrorModel = errorModel ?? throw new System.ArgumentNullException(nameof(errorModel));
        ElementsDescription = string.Join("; ",
            ErrorModel.GetDependentElements().Select(e => $"{e.Name}: {e.Id.GetIdValue()}"));
    }


    public string Message => ErrorModel.Message;

    public string ElementsDescription { get; }

    public ErrorModel ErrorModel { get; }
}
