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
        private readonly IModelObjectService _objectService;

        private string _errorText;
        private string _targetFolder;
        private ModelObjectViewModel _sourceFolder;

        private ModelObjectViewModel _selectedObject;
        private ObservableCollection<ModelObjectViewModel> _modelObjects;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            IModelObjectService objectService,
            IOpenFolderDialogService openFolderDialogService) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _objectService = objectService;

            OpenFolderDialogService = openFolderDialogService;
            ModelObjects = new ObservableCollection<ModelObjectViewModel>();

            LoadViewCommand = RelayCommand.CreateAsync(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            OpenFromFoldersCommand = RelayCommand.CreateAsync(OpenFromFolder, CanOpenFromFolder);
            OpenFolderDialogCommand = RelayCommand.Create(OpenFolderDialog, CanOpenFolderDialog);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand OpenFromFoldersCommand { get; }
        public ICommand OpenFolderDialogCommand { get; }

        public IOpenFolderDialogService OpenFolderDialogService { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string TargetFolder {
            get => _targetFolder;
            set {
                if(Directory.Exists(value)) {
                    this.RaiseAndSetIfChanged(ref _targetFolder, value);
                }
            }
        }

        public ModelObjectViewModel SourceFolder {
            get => _sourceFolder;
            set => this.RaiseAndSetIfChanged(ref _sourceFolder, value);
        }

        public ModelObjectViewModel SelectedObject {
            get => _selectedObject;
            set => this.RaiseAndSetIfChanged(ref _selectedObject, value);
        }

        public ObservableCollection<ModelObjectViewModel> ModelObjects {
            get => _modelObjects;
            set => this.RaiseAndSetIfChanged(ref _modelObjects, value);
        }

        private async Task LoadView() {
            await LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(TargetFolder)) {
                ErrorText = "Выберите папку назначения.";
                return false;
            }

            if(!Directory.Exists(TargetFolder)) {
                ErrorText = "Выберите существующую папку назначения.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private async Task OpenFromFolder() {
            await AddModelObjects(await _objectService.SelectModelObjectDialog());
        }

        private bool CanOpenFromFolder() {
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

        private async Task LoadConfig() {
            TargetFolder = _pluginConfig?.TargetFolder;
            await AddModelObjects(await _objectService.GetFromString(_pluginConfig?.SourceFolder));
        }

        private void SaveConfig() {
            _pluginConfig.TargetFolder = TargetFolder;
            _pluginConfig.SourceFolder = SourceFolder.FullName;
            _pluginConfig.SaveProjectConfig();
        }

        private async Task AddModelObjects(ModelObject modelObject) {
            ModelObjects.Clear();
            if(modelObject != null) {
                SourceFolder = new ModelObjectViewModel(modelObject);
                foreach(ModelObject child in await modelObject.GetChildrenObjects()) {
                    ModelObjects.Add(new ModelObjectViewModel(child));
                }
            }
        }
    }
}
