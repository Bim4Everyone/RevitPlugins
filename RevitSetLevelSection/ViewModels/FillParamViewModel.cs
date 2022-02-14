using dosymep.WPF.ViewModels;

namespace RevitSetLevelSection.ViewModels {
    internal abstract class FillParamViewModel : BaseViewModel {
        public abstract void UpdateElements(bool fromProjectParam);
    }
}