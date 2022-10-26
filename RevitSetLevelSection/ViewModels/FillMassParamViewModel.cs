
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
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

        public FillMassParamViewModel(MainViewModel mainViewModel, RevitRepository revitRepository) {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }

        public override RevitParam RevitParam {
            get => _revitParam;
            set => this.RaiseAndSetIfChanged(ref _revitParam, value);
        }
        
        public string PartParamName {
            get => _partParamName;
            set => this.RaiseAndSetIfChanged(ref _partParamName, value);
        }

        public string Name => $"Обновить \"{RevitParam.Name}\"";

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        public DesignOptionsViewModel DesignOption {
            get => _designOption;
            set {
                this.RaiseAndSetIfChanged(ref _designOption, value);
                if(HasNotRussianLetters()) {
                    ErrorText =
                        "В данном варианте содержится формообразующий элемент со значением параметра содержащий запрещенные символы.";
                } else {
                    ErrorText = null;
                }
            }
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

            return DesignOption.CountMassElements == 0
                ? $"В выбранном варианте \"{DesignOption.Name}\" нет формообразующих."
                : null;
        }

        public override void UpdateElements() {
            string partParamName = PartParamName + _mainViewModel.BuildPart;
            var paramOption = new ParamOption() {SharedRevitParam = RevitParam, ProjectRevitParamName = partParamName};
            _revitRepository.UpdateElements(paramOption, DesignOption.Transform, DesignOption.GetMassObjects());
        }

        private bool HasNotRussianLetters() {
            if(DesignOption == null) {
                return false;
            }

            string partParamName = PartParamName + _mainViewModel.BuildPart;
            var paramOption = new ParamOption() {SharedRevitParam = RevitParam, ProjectRevitParamName = partParamName};
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
    }
}