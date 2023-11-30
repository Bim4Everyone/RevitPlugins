using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly IModelObjectService _rsObjectService;
        private readonly IModelObjectService _fileSystemObjectService;

        private string _errorText;
        private string _saveProperty;

        private string _targetFolder;
        private ModelObjectViewModel _selectedObject;
        private ObservableCollection<ModelObjectViewModel> _modelObjects;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            [RsNeeded] IModelObjectService rsObjectService,
            [FileSystemNeeded] IModelObjectService fileSystemObjectService,
            IOpenFolderDialogService openFolderDialogService) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _rsObjectService = rsObjectService;
            _fileSystemObjectService = fileSystemObjectService;

            OpenFolderDialogService = openFolderDialogService;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            OpenFolderDialogCommand = RelayCommand.Create(OpenFolderDialog, CanOpenFolderDialog);

            OpenFromRsCommand = RelayCommand.CreateAsync(OpenFromRs, CanOpenFromRs);
            OpenFromFoldersCommand = RelayCommand.CreateAsync(OpenFromFolder, CanOpenFromFolder);
            RemoveModelFolderCommand = RelayCommand.Create(RemoveModelFolder, CanRemoveModelFolder);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand OpenFolderDialogCommand { get; }

        public ICommand OpenFromRsCommand { get; }
        public ICommand OpenFromFoldersCommand { get; }
        public ICommand RemoveModelFolderCommand { get; }

        public IOpenFolderDialogService OpenFolderDialogService { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        public string TargetFolder {
            get => _targetFolder;
            set {
                if(Directory.Exists(value)) {
                    this.RaiseAndSetIfChanged(ref _targetFolder, value);
                }
            }
        }

        public ModelObjectViewModel SelectedObject {
            get => _selectedObject;
            set => this.RaiseAndSetIfChanged(ref _selectedObject, value);
        }

        public ObservableCollection<ModelObjectViewModel> ModelObjects {
            get => _modelObjects;
            set => this.RaiseAndSetIfChanged(ref _modelObjects, value);
        }

        private void LoadView() {
            LoadConfig();
            ModelObjects = new ObservableCollection<ModelObjectViewModel>();
        }

        private void AcceptView() {
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = "Введите значение сохраняемого свойства.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void OpenFolderDialog() {
            if(OpenFolderDialogService.ShowDialog()) {
                TargetFolder = OpenFolderDialogService.Folder.FullName;
            }
        }

        private bool CanOpenFolderDialog() {
            return true;
        }

        private async Task OpenFromRs() {
            await AddModelObjects(_rsObjectService);
        }

        private bool CanOpenFromRs() {
            return true;
        }

        private async Task OpenFromFolder() {
            await AddModelObjects(_fileSystemObjectService);
        }

        private bool CanOpenFromFolder() {
            return true;
        }

        private void RemoveModelFolder() {
            throw new System.NotImplementedException();
        }

        private bool CanRemoveModelFolder() {
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }

        private async Task AddModelObjects(IModelObjectService modelObjectService) {
            foreach(ModelObject modelObject in await modelObjectService.OpenModelObjectDialog()) {
                var modelObjectViewModel = new ModelObjectViewModel(modelObject);
                modelObjectViewModel.LoadChildrenCommand.Execute(null);
                ModelObjects.Add(modelObjectViewModel);
            }
        }
    }
}
