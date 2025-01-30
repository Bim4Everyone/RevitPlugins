using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitKrChecker.ViewModels {
    internal class ReportGroupingVM : BaseViewModel {
        private string _notSelectedItem;
        private List<string> _groupingList;
        private IEnumerable<ReportItemVM> _reportResultCollection;

        private string _selectedFirstLevelGrouping;
        private string _selectedSecondLevelGrouping;
        private string _selectedThirdLevelGrouping;

        public ReportGroupingVM() {
            NotSelectedItem = SelectedFirstLevelGrouping = SelectedSecondLevelGrouping = SelectedThirdLevelGrouping
                = "<Нет>";
            GroupingList = GetGroupingList();

            FirstLevelGroupingChangedCommand = RelayCommand.Create(FirstLevelGroupingChanged);
            SecondLevelGroupingChangedCommand = RelayCommand.Create(SecondLevelGroupingChanged);
            ThirdLevelGroupingChangedCommand = RelayCommand.Create(ThirdLevelGroupingChanged);
        }

        public ICommand FirstLevelGroupingChangedCommand { get; }
        public ICommand SecondLevelGroupingChangedCommand { get; }
        public ICommand ThirdLevelGroupingChangedCommand { get; }


        public string NotSelectedItem {
            get => _notSelectedItem;
            set => this.RaiseAndSetIfChanged(ref _notSelectedItem, value);
        }

        public List<string> GroupingList {
            get => _groupingList;
            set => this.RaiseAndSetIfChanged(ref _groupingList, value);
        }

        public string SelectedFirstLevelGrouping {
            get => _selectedFirstLevelGrouping;
            set => this.RaiseAndSetIfChanged(ref _selectedFirstLevelGrouping, value);
        }

        public string SelectedSecondLevelGrouping {
            get => _selectedSecondLevelGrouping;
            set => this.RaiseAndSetIfChanged(ref _selectedSecondLevelGrouping, value);
        }

        public string SelectedThirdLevelGrouping {
            get => _selectedThirdLevelGrouping;
            set => this.RaiseAndSetIfChanged(ref _selectedThirdLevelGrouping, value);
        }

        public void SetCollection(IEnumerable<ReportItemVM> collection) {
            _reportResultCollection = collection;
        }

        private List<string> GetGroupingList() {
            List<string> groupingList = new List<string>() { NotSelectedItem };
            foreach(var prop in typeof(ReportItemVM).GetProperties()) {
                groupingList.Add(prop.Name);
            }
            return groupingList;
        }

        private void FirstLevelGroupingChanged() {
            if(SelectedFirstLevelGrouping is null) {
                return;
            }
            if(SelectedFirstLevelGrouping == NotSelectedItem) {
                SelectedSecondLevelGrouping = SelectedThirdLevelGrouping = NotSelectedItem;
            }
            ReportResultGroupingUpdate();
        }

        private void SecondLevelGroupingChanged() {
            if(SelectedSecondLevelGrouping is null) {
                return;
            }
            if(SelectedSecondLevelGrouping == NotSelectedItem) {
                SelectedThirdLevelGrouping = NotSelectedItem;
            }
            ReportResultGroupingUpdate();
        }

        private void ThirdLevelGroupingChanged() {
            if(SelectedThirdLevelGrouping is null) {
                return;
            }
            ReportResultGroupingUpdate();
        }

        private void ReportResultGroupingUpdate() {
            CollectionView reportResultCollectionView = (CollectionView) CollectionViewSource.GetDefaultView(_reportResultCollection);

            if(reportResultCollectionView is null) {
                return;
            }
            reportResultCollectionView.GroupDescriptions.Clear();

            if(!string.IsNullOrEmpty(SelectedFirstLevelGrouping) && SelectedFirstLevelGrouping != NotSelectedItem) {
                PropertyGroupDescription group1 = new PropertyGroupDescription(SelectedFirstLevelGrouping);
                reportResultCollectionView.GroupDescriptions.Add(group1);
            }

            if(!string.IsNullOrEmpty(SelectedSecondLevelGrouping) && SelectedSecondLevelGrouping != NotSelectedItem) {
                PropertyGroupDescription group2 = new PropertyGroupDescription(SelectedSecondLevelGrouping);
                reportResultCollectionView.GroupDescriptions.Add(group2);
            }

            if(!string.IsNullOrEmpty(SelectedThirdLevelGrouping) && SelectedThirdLevelGrouping != NotSelectedItem) {
                PropertyGroupDescription group3 = new PropertyGroupDescription(SelectedThirdLevelGrouping);
                reportResultCollectionView.GroupDescriptions.Add(group3);
            }
        }
    }
}
