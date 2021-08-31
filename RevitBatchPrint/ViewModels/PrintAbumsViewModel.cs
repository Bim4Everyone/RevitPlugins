using System.Collections.Generic;
using System.ComponentModel;

using dosymep.WPF.ViewModels;

namespace RevitBatchPrint.ViewModels {
    public class PrintAbumsViewModel : BaseViewModel {
        private PrintAlbumViewModel _selectedName;
        private List<PrintAlbumViewModel> _names;
        private PrintSettingsViewModel _printSettings = new PrintSettingsViewModel();

        public PrintAlbumViewModel SelectedAlbum {
            get => _selectedName;
            set => this.RaiseAndSetIfChanged(ref _selectedName, value);
        }

        public List<PrintAlbumViewModel> Albums {
            get => _names;
            set => this.RaiseAndSetIfChanged(ref _names, value);
        }

        public PrintSettingsViewModel PrintSettings {
            get => _printSettings;
            set => this.RaiseAndSetIfChanged(ref _printSettings, value);
        }
    }
}
