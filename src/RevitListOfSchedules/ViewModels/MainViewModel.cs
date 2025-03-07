using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

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

            Links = GetLinks();
            SelectedLinks = new ObservableCollection<LinkViewModel>();

            GroupParameters = GetGroupParameters();
            SelectedGroupParameter = GroupParameters.LastOrDefault();

            Sheets = GetSheets();
            SelectedSheets = new ObservableCollection<SheetViewModel>();

            // Подписка на события для обновления Sheets
            PropertyChanged += OnPropertyChanged;

            // Подписка на события в LinkViewModel
            foreach(var link in Links) {
                link.SelectionChanged += OnLinkSelectionChanged;
            }

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

        // Загружаем с основного документа все линки через _revitRepository
        private ObservableCollection<LinkViewModel> GetLinks() {
            ObservableCollection<LinkViewModel> links = [];
            foreach(LinkTypeElement linkTypeElement in _revitRepository.GetLinkTypeElements()) {
                links.Add(new LinkViewModel(linkTypeElement));
            }
            return links;
        }

        // При изменении события, выполняем метод добавления/удаления в SelectedSheets отмеченный линк и добавляем/удаляем
        // листы в/из Sheets
        private void OnLinkSelectionChanged(object sender, EventArgs e) {
            if(sender is LinkViewModel link) {
                if(link.IsChecked) {
                    if(!SelectedLinks.Contains(link)) {
                        SelectedLinks.Add(link);
                        AddLinkSheets(link);
                    }
                } else {
                    if(SelectedLinks.Contains(link)) {
                        SelectedLinks.Remove(link);
                        DeleteLinkSheets(link);
                    }
                }
            }
        }

        // Добавляем листы из связей и из основного документа в Sheets.
        private ObservableCollection<SheetViewModel> GetSheets() {
            var mainDocumentSheets = _revitRepository.GetSheetElements(_revitRepository.Document)
                .Select(sheetElement => new SheetViewModel(sheetElement) {
                    SheetParameter = SelectedGroupParameter.Parameter
                })
                .ToList();

            foreach(var linkViewModel in SelectedLinks) {
                Document linkDocument = _revitRepository.GetLinkDocument(linkViewModel);
                var linkDocumentSheets = _revitRepository.GetSheetElements(linkDocument)
                    .Select(sheetElement => new SheetViewModel(sheetElement, linkViewModel) {
                        SheetParameter = SelectedGroupParameter.Parameter
                    });
                mainDocumentSheets.AddRange(linkDocumentSheets);
            }
            return SortSheets(new ObservableCollection<SheetViewModel>(mainDocumentSheets));
        }


        // Добавляем листы связей в _sheets.
        private void AddLinkSheets(LinkViewModel linkViewModel) {
            foreach(SheetElement sheetElement in _revitRepository.GetSheetElements(_revitRepository.GetLinkDocument(linkViewModel))) {
                _sheets.Add(new SheetViewModel(sheetElement, linkViewModel) {
                    SheetParameter = SelectedGroupParameter.Parameter
                });
            }
            UpdateGroupParameter();
        }

        // Удаляем листы связей из _sheets
        private void DeleteLinkSheets(LinkViewModel linkViewModel) {
            for(int i = _sheets.Count - 1; i >= 0; i--) {
                if(_sheets[i].LinkViewModel.Id == linkViewModel.Id) {
                    _sheets.RemoveAt(i);
                }
            }
        }

        // Добавляем GroupParameterViewModel в список GroupParameters
        private ObservableCollection<GroupParameterViewModel> GetGroupParameters() {
            var firstSheet = _revitRepository.GetViewSheets(_revitRepository.Document)
                .FirstOrDefault();
            ObservableCollection<GroupParameterViewModel> list = new ObservableCollection<GroupParameterViewModel>();
            if(firstSheet != null) {
                IList<RevitParam> listOfParameter = _revitRepository.GetBrowserParameters(firstSheet);
                if(listOfParameter.Count > 0) {
                    foreach(RevitParam parameter in listOfParameter) {
                        list.Add(new GroupParameterViewModel(parameter));
                    }
                }
            }
            if(list.Count == 0) {
                list.Add(new GroupParameterViewModel());
            }
            return list;
        }

        // В зависимости от SelectedGroupParameter обновляем сортировку в Sheets
        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(SelectedGroupParameter)) {
                UpdateGroupParameter();
            }
        }

        // Метод обновления листов в Sheets в зависисмости от параметра
        private void UpdateGroupParameter() {
            var newSheets = new ObservableCollection<SheetViewModel>(
                _sheets
                .Select(sheetViewModel => {
                    sheetViewModel.SheetParameter = SelectedGroupParameter.Parameter;
                    return sheetViewModel;
                }));

            Sheets = SortSheets(newSheets);
        }

        // Метод сортировки листов в Sheets
        private ObservableCollection<SheetViewModel> SortSheets(ObservableCollection<SheetViewModel> sheetViewModels) {
            var sortedSheets = sheetViewModels
                .OrderBy(sheetViewModel => GetIntFromString(sheetViewModel.Number))
                .OrderBy(sheetViewModel => sheetViewModel.AlbumName)
                .ToList();
            return new ObservableCollection<SheetViewModel>(sortedSheets);
        }

        // Метод для перевода текстового поля в число для сортировки
        private double GetIntFromString(string stringParameter) {
            if(int.TryParse(stringParameter, out var value)) {
                return value;
            }
            return 0;
        }

        // Метод обновления нескольких ссылок
        private void ReloadLinks() {

            using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
                progressDialogService.MaxValue = _selectedLinks.Count;
                progressDialogService.StepValue = progressDialogService.MaxValue / 10;
                progressDialogService.DisplayTitleFormat = "Обновление связанных файлов... [{0}]\\[{1}]";
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
        }

        private bool CanReloadLinks() {
            if(SelectedLinks.Count > 0) {
                return true;
            } else {
                return false;
            }
        }

        // Метод обновления списка SelectedSheets в зависимости от выбора юзера
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

        private void LoadView() {
            //LoadConfig();
        }

        private void AcceptView() {
            //SaveConfig();

            IEnumerable<IGrouping<string, SheetViewModel>> groupedSheets = SelectedSheets
                .GroupBy(sheet => sheet.AlbumName);

            foreach(IGrouping<string, SheetViewModel> groupSheets in groupedSheets) {

                string groupKey = groupSheets.Key;

                TempFamilyDocument tempFamilyDocument = new TempFamilyDocument(
                    _localizationService, _revitRepository, _familyLoadOptions, groupKey);

                ViewDrafting viewDrafting = _revitRepository.GetViewDrafting(groupKey);
                _revitRepository.DeleteFamilyInstance(viewDrafting);

                using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
                    progressDialogService.MaxValue = groupSheets.ToList().Count;
                    progressDialogService.StepValue = progressDialogService.MaxValue / 10;
                    progressDialogService.DisplayTitleFormat = "Обработка листов... [{0}]\\[{1}]";
                    var progress = progressDialogService.CreateProgress();
                    var ct = progressDialogService.CreateCancellationToken();
                    progressDialogService.Show();


                    FamilyInstance familyInstance = null;
                    int i = 0;
                    foreach(var sheetViewModel in groupSheets) {
                        ct.ThrowIfCancellationRequested();
                        progress.Report(i++);
                        Document document = sheetViewModel.LinkViewModel == null
                        ? _revitRepository.Document
                        : _revitRepository.GetLinkDocument(sheetViewModel.LinkViewModel);

                        var schedules = _revitRepository.GetScheduleInstances(document, sheetViewModel.ViewSheet);
                        if(schedules != null) {
                            familyInstance = tempFamilyDocument.PlaceFamilyInstance(viewDrafting, sheetViewModel, schedules)
                                .First();
                        }
                    }
                    FamilySymbol familySymbol = tempFamilyDocument.FamilySymbol;
                    if(!_revitRepository.IsExistView(familySymbol.Name)) {
                        ScheduleElement scheduleElement = new ScheduleElement(
                            _localizationService, _revitRepository, familySymbol, familyInstance);
                    }
                }
            }
        }

        private bool CanAcceptView() {
            if(SelectedSheets.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorText");
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            //SelectedLinks = setting?.SelectedLinks ?? SelectedLinks;
            //SelectedGroupParameter = setting?.SelectedGroupParameter ?? GroupParameters.Last();
            //SelectedSheets = setting?.SelectedSheets ?? SelectedSheets;
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);
            //setting.SelectedLinks = SelectedLinks;
            //setting.SelectedGroupParameter = SelectedGroupParameter;
            //setting.SelectedSheets = SelectedSheets;
            _pluginConfig.SaveProjectConfig();
        }
    }
}


