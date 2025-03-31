using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Comparators;
using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {

    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private readonly FamilyLoadOptions _familyLoadOptions;
        private string _errorText;
        private ObservableCollection<LinkViewModel> _links;
        private ObservableCollection<LinkViewModel> _selectedLinks;
        private ObservableCollection<SheetViewModel> _sheets;
        private ObservableCollection<SheetViewModel> _selectedSheets;
        private ObservableCollection<GroupParameterViewModel> _groupParameters;
        private GroupParameterViewModel _selectedGroupParameter;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService,
            FamilyLoadOptions familyLoadOptions) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;
            _familyLoadOptions = familyLoadOptions;

            LoadViewCommand = RelayCommand.Create(LoadView);
            ReloadLinksCommand = RelayCommand.Create(ReloadLinks, CanReloadLinks);
            UpdateSelectedSheetsCommand = RelayCommand.Create<object>(UpdateSelectedSheets);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand ReloadLinksCommand { get; }
        public ICommand UpdateSelectedSheetsCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<LinkViewModel> Links {
            get => _links;
            set => RaiseAndSetIfChanged(ref _links, value);
        }

        public ObservableCollection<LinkViewModel> SelectedLinks {
            get => _selectedLinks;
            set => RaiseAndSetIfChanged(ref _selectedLinks, value);
        }

        public ObservableCollection<SheetViewModel> Sheets {
            get => _sheets;
            set => RaiseAndSetIfChanged(ref _sheets, value);
        }

        public ObservableCollection<SheetViewModel> SelectedSheets {
            get => _selectedSheets;
            set => RaiseAndSetIfChanged(ref _selectedSheets, value);
        }

        public ObservableCollection<GroupParameterViewModel> GroupParameters {
            get => _groupParameters;
            set => RaiseAndSetIfChanged(ref _groupParameters, value);
        }

        public GroupParameterViewModel SelectedGroupParameter {
            get => _selectedGroupParameter;
            set => RaiseAndSetIfChanged(ref _selectedGroupParameter, value);
        }

        private void LoadView() {

            Links = GetLinks();
            SelectedLinks = new ObservableCollection<LinkViewModel>();

            GroupParameters = GetGroupParameters();
            SelectedGroupParameter = GroupParameters.First();

            Sheets = GetSheets();
            SelectedSheets = new ObservableCollection<SheetViewModel>();

            // Подписка на события для обновления Sheets
            PropertyChanged += OnPropertyChanged;

            // Подписка на события в LinkViewModel
            foreach(var link in Links) {
                link.PropertyChanged += OnLinkChanged;
            }
            LoadConfig();
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            var selectedLinkIds = setting?.SelectedLinks ?? new List<ElementId>();
            SelectedLinks.Clear();
            ObservableCollection<LinkViewModel> links = new ObservableCollection<LinkViewModel>();
            foreach(var linkId in selectedLinkIds) {
                var link = Links.FirstOrDefault(link => link.Id == linkId);
                if(link != null) {
                    links.Add(link);
                    link.IsChecked = true;
                }
            }
            string selectedGroupParameterId = setting?.GroupParameter ?? string.Empty;
            foreach(GroupParameterViewModel groupParameter in GroupParameters) {
                var param = GroupParameters.FirstOrDefault(param => param.Id == selectedGroupParameterId);
                if(param != null) {
                    SelectedGroupParameter = param;
                }
            }
            SelectedLinks = links;
        }

        private void OnLinkChanged(object sender, PropertyChangedEventArgs e) {
            if(sender is LinkViewModel link) {
                switch(e.PropertyName) {
                    case nameof(LinkViewModel.IsLoaded):
                        AddLinkSheets(link);
                        break;
                    case nameof(LinkViewModel.IsChecked):
                        if(!SelectedLinks.Contains(link)) {
                            SelectedLinks.Add(link);
                            if(link.IsLoaded) {
                                AddLinkSheets(link);
                            }
                        } else {
                            if(SelectedLinks.Contains(link)) {
                                DeleteLinkSheets(link);
                                SelectedLinks.Remove(link);
                            }
                        }
                        break;
                }
            }
        }

        // В зависимости от SelectedGroupParameter обновляем сортировку в Sheets
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SelectedGroupParameter)) {
                UpdateGroupParameter();
            }
        }

        // Загружаем с основного документа все линки через _revitRepository
        private ObservableCollection<LinkViewModel> GetLinks() {
            ObservableCollection<LinkViewModel> links = [];
            foreach(LinkTypeElement linkTypeElement in _revitRepository.GetLinkTypeElements()) {
                links.Add(new LinkViewModel(linkTypeElement));
            }
            return links;
        }

        // Добавляем листы из связей и из основного документа в Sheets.
        private ObservableCollection<SheetViewModel> GetSheets() {
            var mainDocumentSheets = new ObservableCollection<SheetViewModel>();

            foreach(SheetElement sheet in _revitRepository.SheetElements) {
                SheetViewModel sheetViewModel = new SheetViewModel(_localizationService, sheet) {
                    GroupParameter = SelectedGroupParameter.Parameter
                };
                mainDocumentSheets.Add(sheetViewModel);
            }
            return mainDocumentSheets;
        }

        private ObservableCollection<SheetViewModel> GetLinkSheetViewModels(LinkViewModel linkViewModel) {
            ObservableCollection<SheetViewModel> sheetViewModels = new ObservableCollection<SheetViewModel>();
            Document linkDocument = _revitRepository.GetLinkDocument(linkViewModel);
            var linkDocumentSheets = _revitRepository.GetSheetElements(linkDocument);

            foreach(SheetElement sheet in linkDocumentSheets) {
                SheetViewModel sheetViewModel = new SheetViewModel(_localizationService, sheet, linkViewModel) {
                    GroupParameter = SelectedGroupParameter.Parameter,
                };
                sheetViewModels.Add(sheetViewModel);
            }
            SortSheets(sheetViewModels);
            return sheetViewModels;
        }

        // Добавляем листы связей в _sheets.
        private void AddLinkSheets(LinkViewModel linkViewModel) {
            foreach(var sheet in GetLinkSheetViewModels(linkViewModel)) {
                _sheets.Add(sheet);
            }
            SortSheets(Sheets);
        }

        // Удаляем листы связей из _sheets
        private void DeleteLinkSheets(LinkViewModel linkViewModel) {
            for(int i = _sheets.Count - 1; i >= 0; i--) {
                if(_sheets[i].LinkViewModel != null && Sheets[i].LinkViewModel.Id == linkViewModel.Id) {
                    _sheets.RemoveAt(i);
                }
            }
        }

        // Добавляем GroupParameterViewModel в список GroupParameters
        private ObservableCollection<GroupParameterViewModel> GetGroupParameters() {
            ObservableCollection<GroupParameterViewModel> list = new ObservableCollection<GroupParameterViewModel>();
            IList<RevitParam> listOfParameter = _revitRepository.GetGroupParameters();
            if(listOfParameter.Count > 0) {
                foreach(RevitParam parameter in listOfParameter) {
                    list.Add(new GroupParameterViewModel(_localizationService, parameter));
                }
            }
            if(list.Count == 0) {
                list.Add(new GroupParameterViewModel(_localizationService));
            }
            return list;
        }


        // Метод обновления листов в Sheets в зависимости от параметра
        private void UpdateGroupParameter() {
            ObservableCollection<SheetViewModel> sheetViewModels = [];
            foreach(SheetViewModel sheetViewModel in _sheets) {
                sheetViewModel.GroupParameter = SelectedGroupParameter.Parameter;
                sheetViewModels.Add(sheetViewModel);
            }
            SortSheets(Sheets);
        }

        // Метод сортировки листов в Sheets
        private void SortSheets(ObservableCollection<SheetViewModel> sheetviewmodels) {
            List<SheetViewModel> sortedList = [.. sheetviewmodels];
            sortedList.Sort(new SheetViewModelComparer());
            sheetviewmodels.Clear();
            foreach(var sheet in sortedList) {
                sheetviewmodels.Add(sheet);
            }
        }

        // Метод обновления нескольких ссылок
        private void ReloadLinks() {
            using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
                progressDialogService.MaxValue = _selectedLinks.Count;
                progressDialogService.StepValue = progressDialogService.MaxValue / 10;
                string progressTitleLinks = _localizationService.GetLocalizedString("MainViewModel.ProgressTitleLinks");
                progressDialogService.DisplayTitleFormat = progressTitleLinks;
                var progress = progressDialogService.CreateProgress();
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                int i = 0;
                foreach(var selectedLink in _selectedLinks) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i++);
                    selectedLink.ReloadLinkType();
                }
            }
            Sheets = GetSheets();
            SelectedSheets.Clear();
        }

        private bool CanReloadLinks() {
            if(SelectedLinks != null) {
                if(SelectedLinks.Count == 0) {
                    return false;
                }
            }
            return true;
        }

        //Метод обновления списка SelectedSheets в зависимости от выбора юзера
        private void UpdateSelectedSheets(object selectedItems) {
            if(selectedItems is IList items) {
                SelectedSheets.Clear();
                foreach(var item in items) {
                    if(item is SheetViewModel sheet) {
                        SelectedSheets.Add(sheet);
                    }
                }
            }
        }

        private void AcceptView() {
            SaveConfig();
            IEnumerable<IGrouping<string, SheetViewModel>> groupedSheets = SelectedSheets
                .GroupBy(sheet => sheet.AlbumName);

            foreach(IGrouping<string, SheetViewModel> groupSheets in groupedSheets) {
                string groupKey = _revitRepository.LegalizeString(groupSheets.Key);

                TempFamilyDocument tempFamDoc = new TempFamilyDocument(
                    _localizationService, _revitRepository, _familyLoadOptions, groupKey);

                ViewDrafting viewDrafting = _revitRepository.GetViewDrafting(groupKey);

                _revitRepository.DeleteFamilyInstance(viewDrafting);

                List<FamilyInstance> familyInstanceList = CreateInstances(tempFamDoc, groupSheets, viewDrafting);

                FamilySymbol familySymbol = tempFamDoc.FamilySymbol;
                if(!_revitRepository.IsExistView(familySymbol.Name)) {
                    ScheduleElement scheduleElement = new ScheduleElement(
                        _localizationService, _revitRepository, familySymbol, familyInstanceList.First());
                }
            }
        }

        private bool CanAcceptView() {
            if(SelectedSheets != null) {
                if(SelectedSheets.Count == 0) {
                    ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorText");
                    return false;
                }
            }
            ErrorText = null;
            return true;
        }

        private List<FamilyInstance> CreateInstances(
            TempFamilyDocument tempFamilyDocument,
            IGrouping<string, SheetViewModel> groupSheets,
            ViewDrafting viewDrafting) {

            List<FamilyInstance> familyInstanceList = [];
            using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
                progressDialogService.MaxValue = groupSheets.ToList().Count;
                progressDialogService.StepValue = progressDialogService.MaxValue / 10;
                string progressTitleLinks = _localizationService.GetLocalizedString("MainViewModel.ProgressTitleSheets");
                progressDialogService.DisplayTitleFormat = progressTitleLinks;
                var progress = progressDialogService.CreateProgress();
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                string transactionNamePlace = _localizationService.GetLocalizedString("MainViewModel.TransactionNamePlace");
                using(Transaction t = _revitRepository.Document.StartTransaction(transactionNamePlace)) {
                    int i = 0;
                    foreach(var sheetViewModel in groupSheets) {
                        ct.ThrowIfCancellationRequested();
                        progress.Report(i++);
                        familyInstanceList.AddRange(GetFamilyInstances(sheetViewModel, tempFamilyDocument, viewDrafting));
                    }

                    if(familyInstanceList.Count == 0) {
                        FamilyInstance familyInstance = tempFamilyDocument.CreateInstance(viewDrafting, "", "", "");
                        familyInstanceList.Add(familyInstance);
                    }
                    t.Commit();
                }
            }
            return familyInstanceList;
        }

        private List<FamilyInstance> GetFamilyInstances(
            SheetViewModel sheetViewModel, TempFamilyDocument tempFamilyDocument, ViewDrafting viewDrafting) {
            List<FamilyInstance> familyInstanceList = [];
            Document document = sheetViewModel.LinkViewModel == null
                ? _revitRepository.Document
                : _revitRepository.GetLinkDocument(sheetViewModel.LinkViewModel);
            var schedules = _revitRepository.GetScheduleInstances(document, sheetViewModel.ViewSheet);

            if(schedules != null) {
                var instances = tempFamilyDocument.PlaceFamilyInstances(viewDrafting, sheetViewModel, schedules);
                familyInstanceList.AddRange(instances);
            }
            return familyInstanceList;
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);
            setting.SelectedLinks = SelectedLinks
                .Select(link => link.Id)
                .ToList();
            setting.GroupParameter = SelectedGroupParameter.Id;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
