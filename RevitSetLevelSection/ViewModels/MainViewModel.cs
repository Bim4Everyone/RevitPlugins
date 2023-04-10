using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Diagram.Core.Layout;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IViewModelFactory _viewModelFactory;

        private string _errorText;
        private LinkTypeViewModel _linkType;
        private ObservableCollection<LinkTypeViewModel> _linkTypes;
        private ObservableCollection<FillParamViewModel> _fillParams;

        public MainViewModel(RevitRepository revitRepository, IViewModelFactory viewModelFactory) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            _viewModelFactory = viewModelFactory;

            LoadViewCommand = new RelayCommand(LoadView);
            UpdateBuildPartCommand = new RelayCommand(UpdateBuildPart);
            UpdateElementsCommand = new RelayCommand(UpdateElements, CanUpdateElement);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand UpdateBuildPartCommand { get; }
        public ICommand UpdateElementsCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        public ObservableCollection<LinkTypeViewModel> LinkTypes {
            get => _linkTypes;
            set => this.RaiseAndSetIfChanged(ref _linkTypes, value);
        }

        public ObservableCollection<FillParamViewModel> FillParams {
            get => _fillParams;
            set => this.RaiseAndSetIfChanged(ref _fillParams, value);
        }
        
        private IEnumerable<LinkTypeViewModel> GetLinkTypes() {
            return _revitRepository.GetKoordLinkTypes()
                .Select(item => _viewModelFactory.Create(item));
        }

        private IEnumerable<FillParamViewModel> GetFillParams() {
            yield return _viewModelFactory.Create(ParamOption.BuildingWorksBlock);
            yield return _viewModelFactory.Create(ParamOption.BuildingWorksSection);
            yield return _viewModelFactory.Create(ParamOption.BuildingWorksTyping);
            yield return _viewModelFactory.Create(SharedParamsConfig.Instance.BuildingWorksLevel);
        }

        private void LoadView(object obj) {
            LinkTypes = new ObservableCollection<LinkTypeViewModel>(GetLinkTypes());
            FillParams = new ObservableCollection<FillParamViewModel>(GetFillParams());
            
            // После присвоения выполняется
            // команда обновления раздела,
            // которой требуются заполненные параметры
            LinkType = LinkTypes.FirstOrDefault();
            
            SetConfig();
        }
        
        public void UpdateBuildPart(object args) {
            foreach(FillMassParamViewModel fillParam in FillParams.OfType<FillMassParamViewModel>()) {
                fillParam.CheckRussianTextCommand.Execute(null);
                fillParam.UpdatePartParamNameCommand.Execute(null);
            }
        }

        private void UpdateElements(object param) {
            SaveConfig();

            using(Transaction transaction = _revitRepository.StartTransaction("Заполнение параметров СМР")) {
                var fillParams = FillParams
                    .Where(item => item.IsEnabled)
                    .Select(item => item.CreateFillParam())
                    .ToArray();

                var elements = _revitRepository.GetElementInstances();
                using(var window = CreateProgressDialog(elements)) {
                    var progress = window.CreateProgress();
                    var cancellationToken = window.CreateCancellationToken();

                    int count = 1;
                    foreach(var element in elements) {
                        progress.Report(count++);
                        cancellationToken.ThrowIfCancellationRequested();

                        foreach(IFillParam fillParam in fillParams) {
                            fillParam.UpdateValue(element);
                        }
                    }
                }

                transaction.Commit();
            }
        }

        private IProgressDialogService CreateProgressDialog(IList<Element> elements) {
            var window = GetPlatformService<IProgressDialogService>();
            window.DisplayTitleFormat = $"Заполнение [{{0}}\\{{1}}]";
            window.MaxValue = elements.Count;
            window.StepValue = 10;

            window.Show();
            return window;
        }

        private bool CanUpdateElement(object param) {
            if(LinkType == null) {
                ErrorText = "Выберите координационный файл.";
                return false;
            }

            if(LinkType != null && !LinkType.IsLinkLoaded) {
                ErrorText = "Выбранная связь выгружена.";
                return false;
            }

            if(!FillParams.Any(item => item.IsEnabled)) {
                ErrorText = "Выберите хотя бы один параметр.";
                return false;
            }

            string errorText = FillParams
                .Select(item => item.GetErrorText())
                .FirstOrDefault(item => !string.IsNullOrEmpty(item));

            if(!string.IsNullOrEmpty(errorText)) {
                ErrorText = errorText;
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void SetConfig() {
            RevitSettings settings =
                PluginConfig.GetPluginConfig()
                    .GetSettings(_revitRepository.Document);
            if(settings == null) {
                return;
            }

            if(LinkType != null) {
                LinkType.BuildPart = LinkType.BuildParts.FirstOrDefault(item => item.Equals(settings.BuildPart));
            }

            foreach(FillParamViewModel fillParam in FillParams) {
                ParamSettings paramSettings = settings.ParamSettings
                    .FirstOrDefault(item => item.ParamId.Equals(fillParam.RevitParam.Id));

                if(paramSettings != null) {
                    fillParam.SetParamSettings(paramSettings);
                }
            }
        }

        private void SaveConfig() {
            PluginConfig config = PluginConfig.GetPluginConfig();
            RevitSettings settings = config.GetSettings(_revitRepository.Document);
            if(settings == null) {
                settings = config.AddSettings(_revitRepository.Document);
            }

            settings.BuildPart = LinkType.BuildPart;
            settings.ParamSettings.Clear();
            foreach(FillParamViewModel fillParam in FillParams) {
                ParamSettings paramSettings = fillParam.GetParamSettings();
                settings.ParamSettings.Add(paramSettings);
            }

            config.SaveProjectConfig();
        }
    }
}