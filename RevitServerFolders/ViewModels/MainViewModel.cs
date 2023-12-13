using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly IModelObjectService _objectService;

        private string _errorText;
        private string _targetFolder;
        private string _sourceFolder;

        private ModelObjectViewModel _selectedObject;
        private ObservableCollection<ModelObjectViewModel> _modelObjects;

        private bool _isExportRooms;
        private bool _isExportRoomsVisible;

        public MainViewModel(PluginConfig pluginConfig,
            IModelObjectService objectService,
            IOpenFolderDialogService openFolderDialogService,
            IProgressDialogFactory progressDialogFactory) {
            _pluginConfig = pluginConfig;
            _objectService = objectService;

            OpenFolderDialogService = openFolderDialogService;
            ProgressDialogFactory = progressDialogFactory;
            ModelObjects = new ObservableCollection<ModelObjectViewModel>();

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            OpenFromFoldersCommand = RelayCommand.CreateAsync(OpenFromFolder, CanOpenFromFolder);
            OpenFolderDialogCommand = RelayCommand.Create(OpenFolderDialog, CanOpenFolderDialog);
            SourceFolderChangedCommand = RelayCommand.CreateAsync(SourceFolderChanged, CanSourceFolderChanged);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand OpenFromFoldersCommand { get; }
        public ICommand OpenFolderDialogCommand { get; }
        public ICommand SourceFolderChangedCommand { get; }

        public IOpenFolderDialogService OpenFolderDialogService { get; }
        public IProgressDialogFactory ProgressDialogFactory { get; }

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

        public string SourceFolder {
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

        public bool IsExportRooms {
            get => _isExportRooms;
            set => this.RaiseAndSetIfChanged(ref _isExportRooms, value);
        }

        public bool IsExportRoomsVisible {
            get => _isExportRoomsVisible;
            set => this.RaiseAndSetIfChanged(ref _isExportRoomsVisible, value);
        }
        
        protected virtual void LoadConfigImpl() { }
        protected virtual void SaveConfigImpl() { }
        protected virtual void AcceptViewImpl() { }

        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
            AcceptViewImpl();
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

            if(SourceFolder == null) {
                ErrorText = "Выберите папку источника.";
                return false;
            }

            if(ModelObjects.Count == 0) {
                ErrorText = "Выберите папку источника c моделями.";
                return false;
            }

            if(!ModelObjects
                   .Any(item => !item.SkipObject)) {
                ErrorText = "Все модели помечены признаком пропустить.";
                return false;
            }

            string duplicateModelObject = ModelObjects
                .Where(item => !item.SkipObject)
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > 1)
                .Select(item => item.Key)
                .FirstOrDefault();

            if(!string.IsNullOrEmpty(duplicateModelObject)) {
                ErrorText = $"Папка источника содержит дубликаты \"{duplicateModelObject}\".";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private async Task OpenFromFolder() {
            ModelObject modelObject = await _objectService.SelectModelObjectDialog(SourceFolder);
            SourceFolder = modelObject.FullName;
            await AddModelObjects(modelObject);
        }

        private bool CanOpenFromFolder() {
            return true;
        }

        private void OpenFolderDialog() {
            if(OpenFolderDialogService.ShowDialog(TargetFolder)) {
                TargetFolder = OpenFolderDialogService.Folder.FullName;
            }
        }

        private bool CanOpenFolderDialog() {
            return true;
        }

        private async Task SourceFolderChanged() {
            try {
                ModelObjects.Clear();
                await AddModelObjects(await _objectService.GetFromString(SourceFolder));
            } catch {
                // pass
            }
        }

        private bool CanSourceFolderChanged() {
            return true;
        }

        private void LoadConfig() {
            TargetFolder = _pluginConfig.TargetFolder;
            SourceFolder = _pluginConfig.SourceFolder;

            LoadConfigImpl();
        }

        private void SaveConfig() {
            _pluginConfig.TargetFolder = TargetFolder;
            _pluginConfig.SourceFolder = SourceFolder;
            _pluginConfig.SkippedObjects = ModelObjects
                .Where(item => item.SkipObject)
                .Select(item => item.FullName)
                .ToArray();


            SaveConfigImpl();
            _pluginConfig.SaveProjectConfig();
        }

        private async Task AddModelObjects(ModelObject modelObject) {
            ModelObjects.Clear();
            if(modelObject != null) {
                IEnumerable<ModelObject> modelObjects = await modelObject.GetChildrenObjects();

                modelObjects = modelObjects
                    .OrderBy(item => item.Name);

                foreach(ModelObject child in modelObjects) {
                    ModelObjects.Add(new ModelObjectViewModel(child));
                }

                foreach(ModelObjectViewModel modelObjectViewModel in ModelObjects) {
                    modelObjectViewModel.SkipObject = _pluginConfig.SkippedObjects?
                        .Contains(modelObjectViewModel.FullName, StringComparer.OrdinalIgnoreCase) == true;
                }
            }
        }
    }
}
