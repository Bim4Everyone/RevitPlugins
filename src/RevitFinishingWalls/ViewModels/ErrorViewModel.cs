using dosymep.WPF.ViewModels;

namespace RevitFinishingWalls.ViewModels {
    internal class ErrorViewModel : BaseViewModel {
        public ErrorViewModel() {
        }


        public string Message { get; set; }

        public string Title => "Ошибки создания отделки стен";
    }
}
