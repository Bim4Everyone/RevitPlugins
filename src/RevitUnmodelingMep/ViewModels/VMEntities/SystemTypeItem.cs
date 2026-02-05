using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using dosymep.WPF.ViewModels;

namespace RevitUnmodelingMep.ViewModels;

internal class SystemTypeItem : BaseViewModel {
    private string _name;
    private ObservableCollection<ConfigAssignmentItem> _configs;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public int Id { get; set; }

    public ObservableCollection<ConfigAssignmentItem> Configs {
        get => _configs;
        set {
            if(!ReferenceEquals(_configs, value)) {
                RaiseAndSetIfChanged(ref _configs, value);
                AttachConfigs(_configs);
                RaisePropertyChanged(nameof(IsAllSelected));
            }
        }
    }

    public bool IsAllSelected {
        get => Configs != null && Configs.Count > 0 && Configs.All(c => c.IsChecked);
        set {
            if(Configs == null) {
                return;
            }

            foreach(ConfigAssignmentItem config in Configs) {
                config.IsChecked = value;
            }
            RaisePropertyChanged(nameof(IsAllSelected));
        }
    }

    private void AttachConfigs(ObservableCollection<ConfigAssignmentItem> configs) {
        if(configs == null) {
            return;
        }

        configs.CollectionChanged += ConfigsOnCollectionChanged;
        foreach(ConfigAssignmentItem config in configs) {
            config.PropertyChanged += ConfigOnPropertyChanged;
        }
    }

    private void ConfigsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if(e.NewItems != null) {
            foreach(ConfigAssignmentItem item in e.NewItems) {
                item.PropertyChanged += ConfigOnPropertyChanged;
            }
        }

        if(e.OldItems != null) {
            foreach(ConfigAssignmentItem item in e.OldItems) {
                item.PropertyChanged -= ConfigOnPropertyChanged;
            }
        }

        RaisePropertyChanged(nameof(IsAllSelected));
    }

    private void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(ConfigAssignmentItem.IsChecked)) {
            RaisePropertyChanged(nameof(IsAllSelected));
        }
    }
}
