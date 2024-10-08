using System;
using System.IO;
using System.Windows;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitScheduleImport.Models;
using RevitScheduleImport.Services;

namespace RevitScheduleImport.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IOpenFileDialogService _fileDialogService;
        private readonly IMessageBoxService _messageBoxService;

        private string _initialDirectory;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService,
            IOpenFileDialogService fileDialogService,
            IMessageBoxService messageBoxService) {

            _pluginConfig = pluginConfig
                ?? throw new System.ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository
                ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));
            _fileDialogService = fileDialogService
                ?? throw new System.ArgumentNullException(nameof(fileDialogService));
            _messageBoxService = messageBoxService
                ?? throw new ArgumentNullException(nameof(messageBoxService));
            LoadConfig();
        }


        public ILocalizationService LocalizationService => _localizationService;

        public IMessageBoxService MessageBoxService => _messageBoxService;

        public IOpenFileDialogService OpenFileDialogService => _fileDialogService;

        public string InitialDirectory {
            get => _initialDirectory;
            set => this.RaiseAndSetIfChanged(ref _initialDirectory, value);
        }


        public bool ExecuteImportCommand() {
            _fileDialogService.InitialDirectory = InitialDirectory;
            var dialogResult = _fileDialogService.ShowDialog();
            if(dialogResult) {
                InitialDirectory = _fileDialogService.File.DirectoryName;
                SaveConfig();
                try {
                    var importer = GetPlatformService<ScheduleImporter>();
                    var transactionName = _localizationService.GetLocalizedString("Revit.Transaction");
                    importer.ImportSchedule(
                        _fileDialogService.File.FullName,
                        transactionName,
                        out string[] failedSheets);

                    if(failedSheets.Length > 0) {
                        var header = _localizationService.GetLocalizedString("Warnings.NotAllSheetsImported");
                        var msg = $"{header}\n{string.Join("\n", failedSheets)}";
                        _messageBoxService.Show(
                            msg,
                            _localizationService.GetLocalizedString("Errors.Severity.Warning"),
                            System.Windows.MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                } catch(FileNotFoundException) {
                    ShowLocalizedErrorMessage("Errors.FileNotFoundException", MessageBoxImage.Error);
                } catch(IOException) {
                    ShowLocalizedErrorMessage("Errors.IOException", MessageBoxImage.Error);
                }
            }
            return dialogResult;
        }


        private void ShowLocalizedErrorMessage(string localizedContentName, MessageBoxImage messageImage) {
            _messageBoxService.Show(
                _localizationService.GetLocalizedString(localizedContentName),
                _localizationService.GetLocalizedString("Errors.Severity.Error"),
                System.Windows.MessageBoxButton.OK,
                messageImage);
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            InitialDirectory = setting?.InitialDirectory
                ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.InitialDirectory = InitialDirectory;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
