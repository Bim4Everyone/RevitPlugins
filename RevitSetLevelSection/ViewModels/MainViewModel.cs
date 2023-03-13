﻿using System;
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

            SetConfig();
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
            return _revitRepository.GetRevitLinkTypes()
                .Select(item => _viewModelFactory.Create(item));
        }

        private IEnumerable<FillParamViewModel> GetFillParams() {
            yield return _viewModelFactory.Create(SharedParamsConfig.Instance.BuildingWorksLevel);
            yield return _viewModelFactory.Create(ParamOption.BuildingWorksBlock);
            yield return _viewModelFactory.Create(ParamOption.BuildingWorksSection);
            yield return _viewModelFactory.Create(ParamOption.BuildingWorksTyping);
        }

        private void LoadView(object obj) {
            LinkTypes = new ObservableCollection<LinkTypeViewModel>(GetLinkTypes());
            FillParams = new ObservableCollection<FillParamViewModel>(GetFillParams());
        }
        
        public void UpdateBuildPart(object args) {
            foreach(FillMassParamViewModel fillParam in FillParams.OfType<FillMassParamViewModel>()) {
                fillParam.CheckRussianTextCommand.Execute(null);
                fillParam.UpdatePartParamNameCommand.Execute(null);
            }
        }

        private void UpdateElements(object param) {
            SaveConfig();

            using(var transactionGroup =
                  _revitRepository.StartTransactionGroup("Установка уровня/секции")) {
                foreach(FillParamViewModel fillParamViewModel in FillParams.Where(item => item.IsEnabled)) {
                    fillParamViewModel.UpdateElements();
                }

                transactionGroup.Assimilate();
            }
        }

        private bool CanUpdateElement(object param) {
            if(LinkType == null) {
                ErrorText = "Выберите координационный файл.";
                return false;
            }

            if(LinkType != null && !LinkType.IsLoaded) {
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

            LinkType = LinkTypes
                           .FirstOrDefault(item => item.Id == settings.LinkFileId)
                       ?? LinkTypes.FirstOrDefault();

            if(LinkType != null) {
                LinkType.BuildPart = LinkType?.BuildParts.Contains(settings.BuildPart) == true
                    ? settings.BuildPart
                    : null;
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
            settings.LinkFileId = LinkType.Id;
            settings.ParamSettings.Clear();
            foreach(FillParamViewModel fillParam in FillParams) {
                ParamSettings paramSettings = fillParam.GetParamSettings();
                settings.ParamSettings.Add(paramSettings);
            }

            config.SaveProjectConfig();
        }
    }
}