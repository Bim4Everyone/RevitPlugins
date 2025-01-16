using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels {
    internal class LinkToUpdateViewModel : BaseViewModel {
        private readonly ILinkPair _linkPair;

        public LinkToUpdateViewModel(ILinkPair linkPair) {
            _linkPair = linkPair ?? throw new System.ArgumentNullException(nameof(linkPair));
        }



    }
}
