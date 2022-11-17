
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Mvvm;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillMassParamViewModel : FillParamViewModel {
        private readonly MainViewModel _mainViewModel;
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;
        private DesignOptionsViewModel _designOption;

        private RevitParam _revitParam;
        private string _errorText;
        private string _partParamName;
        private string _selectedParamName;

        public FillMassParamViewModel(MainViewModel mainViewModel, RevitRepository revitRepository) {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));

            CheckRussianTextCommand = new RelayCommand(CheckRussianText);
            UpdatePartParamNameCommand = new RelayCommand(UpdatePartParamName);
        }

        public ICommand CheckRussianTextCommand { get; }
        public ICommand UpdatePartParamNameCommand { get; }

        public override RevitParam RevitParam {
            get => _revitParam;
            set => this.RaiseAndSetIfChanged(ref _revitParam, value);
        }

        public bool IsRequired { get; set; }
        public string AdskParamName { get; set; }

        public string PartParamName {
            get => _partParamName;
            set => this.RaiseAndSetIfChanged(ref _partParamName, value);
        }

        public string SelectedParamName {
            get => _selectedParamName;
            set => this.RaiseAndSetIfChanged(ref _selectedParamName, value);
        }

        public string Name => $"Обновить \"{RevitParam.Name}\"";

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        public DesignOptionsViewModel DesignOption {
            get => _designOption;
            set => this.RaiseAndSetIfChanged(ref _designOption, value);
        }

        public string ErrorText {
            get => _errorText;
            set {
                this.RaiseAndSetIfChanged(ref _errorText, value);
                this.RaisePropertyChanged(nameof(HasError));
            }
        }

        public override string GetErrorText() {
            if(!IsEnabled) {
                return null;
            }

            if(DesignOption == null) {
                return "Выберите вариант с формообразующими.";
            }

            if(DesignOption.CountMassElements == 0) {
                return $"В варианте \"{DesignOption.Name}\" нет формообразующих.";
            }

            if(IsRequired) {
                string partParamName = PartParamName + _mainViewModel.LinkType.BuildPart;
                var paramOption = new ParamOption() {
                    SharedRevitParam = RevitParam, ProjectRevitParamName = partParamName, AdskParamName = AdskParamName
                };
                return DesignOption.GetMassObjects()
                    .All(item => item.IsExistsParamValue(paramOption))
                    ? null
                    : $"У формообразующих не заполнен параметр \"{RevitParam.Name}\".";
            }

            return null;
        }

        public override void UpdateElements() {
            string partParamName = PartParamName + _mainViewModel.LinkType.BuildPart;
            var paramOption = new ParamOption() {
                SharedRevitParam = RevitParam, ProjectRevitParamName = partParamName, AdskParamName = AdskParamName
            };
            _revitRepository.UpdateElements(paramOption, DesignOption.Transform, DesignOption.GetMassObjects());
        }

        private bool HasNotRussianLetters() {
            if(DesignOption == null) {
                return false;
            }

            string partParamName = PartParamName + _mainViewModel.LinkType.BuildPart;
            var paramOption = new ParamOption() {
                SharedRevitParam = RevitParam, ProjectRevitParamName = partParamName, AdskParamName = AdskParamName
            };
            return DesignOption.GetMassObjects()
                .Any(item => HasNotRussianLetters(item.GetParamValue<string>(paramOption)));
        }

        private bool HasNotRussianLetters(string text) {
            if(string.IsNullOrEmpty(text)) {
                return false;
            }

            return !Regex.IsMatch(text, @"^[А-Яа-я0-9,\s]+$");
        }

        public override ParamSettings GetParamSettings() {
            return new ParamSettings() {
                IsEnabled = IsEnabled, ElementId = DesignOption?.Id, PropertyName = RevitParam.Id
            };
        }

        public override void SetParamSettings(ParamSettings paramSettings) {
            IsEnabled = paramSettings.IsEnabled;
            DesignOption = _mainViewModel.LinkType?.DesignOptions
                               .FirstOrDefault(item => item.Id == paramSettings.ElementId)
                           ?? _mainViewModel.LinkType?.DesignOptions.FirstOrDefault();
        }

        private void CheckRussianText(object args) {
            ErrorText = HasNotRussianLetters()
                ? "Найдены формообразующие содержащие запрещенные символы."
                : null;
        }

        private void UpdatePartParamName(object args) {
            var partNames = _mainViewModel.LinkType.GetPartNames(new[] {PartParamName}).ToArray();
            SelectedParamName = partNames.FirstOrDefault(item => item.Equals(_mainViewModel.LinkType.BuildPart)) ??
                                partNames.FirstOrDefault();
        }
    }
}