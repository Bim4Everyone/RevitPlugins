using dosymep.WPF.ViewModels;

namespace RevitSetLevelSection.ViewModels {
    internal abstract class FillParamViewModel : BaseViewModel {
        public abstract bool IsEnabled { get; set; }
        public abstract string GetErrorText(bool fromRevitParam);
        public abstract void UpdateElements(bool fromProjectParam);
    }
}